using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Cofrox.Converters.Infrastructure;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cofrox.Converters.Engines;

public sealed class DataConversionEngine(IFormatCatalog formatCatalog) : ConversionEngineBase(formatCatalog)
{
    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly ISerializer _yamlSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public override bool CanHandle(string sourceExtension, string targetExtension)
    {
        var source = FormatCatalog.GetByExtension(sourceExtension);
        var target = FormatCatalog.GetByExtension(targetExtension);
        return source.Family is FileFamily.Data or FileFamily.Spreadsheet &&
               target.Family is FileFamily.Data or FileFamily.Spreadsheet;
    }

    public override Task<ConversionResult> ConvertAsync(ConversionJob job, IProgress<double>? progress, CancellationToken cancellationToken) =>
        Task.Run(() =>
        {
            var startedAt = DateTimeOffset.Now;
            try
            {
                progress?.Report(0.1);
                var sourceExtension = Path.GetExtension(job.SourceFile.SourcePath).TrimStart('.').ToLowerInvariant();
                var targetExtension = Path.GetExtension(job.OutputPath).TrimStart('.').ToLowerInvariant();

                var document = LoadToJsonNode(job.SourceFile.SourcePath, sourceExtension);
                progress?.Report(0.5);
                SaveFromJsonNode(document, job.OutputPath, targetExtension, job.Options);
                progress?.Report(1);

                return new ConversionResult
                {
                    Status = ConversionStatus.Completed,
                    OutputPath = job.OutputPath,
                    Message = "Data conversion completed.",
                    Duration = DateTimeOffset.Now - startedAt,
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    Status = ConversionStatus.Failed,
                    Message = ex.Message,
                    Duration = DateTimeOffset.Now - startedAt,
                };
            }
        }, cancellationToken);

    private JsonNode LoadToJsonNode(string path, string extension)
    {
        var text = File.ReadAllText(path);
        return extension switch
        {
            "json" => JsonNode.Parse(text) ?? new JsonObject(),
            "yaml" or "yml" => JsonSerializer.SerializeToNode(_yamlDeserializer.Deserialize<object>(text)) ?? new JsonObject(),
            "xml" => ConvertXmlToJson(XDocument.Parse(text)),
            "csv" => ConvertTableToJson(text, ','),
            "tsv" => ConvertTableToJson(text, '\t'),
            _ => throw new NotSupportedException($"Data conversion from .{extension} is not supported yet."),
        };
    }

    private void SaveFromJsonNode(JsonNode document, string path, string extension, IReadOnlyDictionary<string, object?> options)
    {
        switch (extension)
        {
            case "json":
                File.WriteAllText(path, WriteJson(document, ConversionOptionReader.GetString(options, "indentation", "2")));
                break;
            case "yaml":
            case "yml":
                File.WriteAllText(path, _yamlSerializer.Serialize(JsonSerializer.Deserialize<object>(document.ToJsonString()) ?? new { }));
                break;
            case "xml":
                ConvertJsonToXml(document).Save(path);
                break;
            case "csv":
                WriteTable(path, document, GetDelimiter(ConversionOptionReader.GetString(options, "delimiter", "comma")));
                break;
            case "tsv":
                WriteTable(path, document, '\t');
                break;
            default:
                throw new NotSupportedException($"Data conversion to .{extension} is not supported yet.");
        }
    }

    private static JsonNode ConvertTableToJson(string text, char delimiter)
    {
        using var reader = new StringReader(text);
        using var csv = new CsvReader(
            reader,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(),
            });

        var rows = new JsonArray();
        foreach (var record in csv.GetRecords<dynamic>())
        {
            var objectNode = new JsonObject();
            foreach (var pair in (IDictionary<string, object>)record)
            {
                objectNode[pair.Key] = pair.Value?.ToString();
            }

            rows.Add(objectNode);
        }

        return rows;
    }

    private static void WriteTable(string path, JsonNode document, char delimiter)
    {
        var rows = document as JsonArray ?? new JsonArray(document);
        var flattenedRows = rows.Select(static node => FlattenJson(node as JsonObject ?? new JsonObject())).ToArray();
        var headers = flattenedRows.SelectMany(static row => row.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        using var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        using var csv = new CsvWriter(
            writer,
            new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(),
            });

        foreach (var header in headers)
        {
            csv.WriteField(header);
        }
        csv.NextRecord();

        foreach (var row in flattenedRows)
        {
            foreach (var header in headers)
            {
                csv.WriteField(row.TryGetValue(header, out var value) ? value : string.Empty);
            }

            csv.NextRecord();
        }
    }

    private static Dictionary<string, string?> FlattenJson(JsonObject node, string prefix = "")
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in node)
        {
            var key = string.IsNullOrWhiteSpace(prefix) ? pair.Key : $"{prefix}.{pair.Key}";
            switch (pair.Value)
            {
                case JsonObject childObject:
                    foreach (var nested in FlattenJson(childObject, key))
                    {
                        result[nested.Key] = nested.Value;
                    }

                    break;
                case JsonArray childArray:
                    result[key] = childArray.ToJsonString();
                    break;
                default:
                    result[key] = pair.Value?.ToString();
                    break;
            }
        }

        return result;
    }

    private static string WriteJson(JsonNode document, string indentation)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indentation != "minified",
        };

        return indentation == "tab"
            ? document.ToJsonString(options).Replace("  ", "\t", StringComparison.Ordinal)
            : document.ToJsonString(options);
    }

    private static char GetDelimiter(string option) => option switch
    {
        "tab" => '\t',
        "semicolon" => ';',
        "pipe" => '|',
        _ => ',',
    };

    private static JsonNode ConvertXmlToJson(XDocument document)
    {
        var root = document.Root ?? throw new InvalidOperationException("XML document has no root element.");
        return new JsonObject
        {
            [root.Name.LocalName] = ConvertElement(root),
        };
    }

    private static JsonNode ConvertElement(XElement element)
    {
        if (!element.HasElements)
        {
            return element.Value;
        }

        var json = new JsonObject();
        foreach (var group in element.Elements().GroupBy(static child => child.Name.LocalName))
        {
            if (group.Count() == 1)
            {
                json[group.Key] = ConvertElement(group.First());
            }
            else
            {
                var array = new JsonArray();
                foreach (var item in group)
                {
                    array.Add(ConvertElement(item));
                }

                json[group.Key] = array;
            }
        }

        return json;
    }

    private static XDocument ConvertJsonToXml(JsonNode node)
    {
        var rootElement = node switch
        {
            JsonObject obj when obj.Count == 1 => obj.First(),
            _ => new KeyValuePair<string, JsonNode?>("root", node),
        };

        return new XDocument(ConvertNode(rootElement.Key, rootElement.Value));
    }

    private static XElement ConvertNode(string name, JsonNode? node) => node switch
    {
        null => new XElement(name),
        JsonValue value => new XElement(name, value.ToJsonString().Trim('"')),
        JsonArray array => new XElement(name, array.Select(item => ConvertNode("item", item))),
        JsonObject obj => new XElement(name, obj.Select(pair => ConvertNode(pair.Key, pair.Value))),
        _ => new XElement(name),
    };
}
