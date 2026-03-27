using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cofrox.App.Services;

public static class LegalDocumentRenderer
{
    public static void Render(StackPanel host, string markdown)
    {
        host.Children.Clear();

        var paragraphLines = new List<string>();
        var lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();

            if (string.IsNullOrWhiteSpace(line))
            {
                FlushParagraph(host, paragraphLines);
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                FlushParagraph(host, paragraphLines);
                host.Children.Add(CreateStyledTextBlock(StripInlineFormatting(line[2..]), "DisplayTextStyle", new Thickness(0, 0, 0, 12)));
                continue;
            }

            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushParagraph(host, paragraphLines);
                host.Children.Add(CreateStyledTextBlock(StripInlineFormatting(line[3..]), "SectionHeadingStyle", new Thickness(0, 16, 0, 8)));
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                FlushParagraph(host, paragraphLines);
                host.Children.Add(CreateBodyTextBlock("\u2022 " + StripInlineFormatting(line[2..]), new Thickness(12, 0, 0, 6)));
                continue;
            }

            if (line.StartsWith("*", StringComparison.Ordinal) && line.EndsWith("*", StringComparison.Ordinal) && line.Length > 2)
            {
                FlushParagraph(host, paragraphLines);
                host.Children.Add(CreateBodyTextBlock(StripInlineFormatting(line.Trim('*')), new Thickness(0, 0, 0, 8), italic: true));
                continue;
            }

            paragraphLines.Add(line);
        }

        FlushParagraph(host, paragraphLines);
    }

    private static void FlushParagraph(StackPanel host, List<string> paragraphLines)
    {
        if (paragraphLines.Count == 0)
        {
            return;
        }

        host.Children.Add(CreateBodyTextBlock(StripInlineFormatting(string.Join(" ", paragraphLines)), new Thickness(0, 0, 0, 8)));
        paragraphLines.Clear();
    }

    private static string StripInlineFormatting(string text) =>
        text.Replace("**", string.Empty, StringComparison.Ordinal)
            .Replace("`", string.Empty, StringComparison.Ordinal);

    private static TextBlock CreateStyledTextBlock(string text, string styleKey, Thickness margin)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            Margin = margin,
            TextWrapping = TextWrapping.WrapWholeWords,
            IsTextSelectionEnabled = true,
        };

        if (Microsoft.UI.Xaml.Application.Current.Resources[styleKey] is Style style)
        {
            textBlock.Style = style;
        }

        return textBlock;
    }

    private static TextBlock CreateBodyTextBlock(string text, Thickness margin, bool italic = false) =>
        new()
        {
            Text = text,
            Margin = margin,
            TextWrapping = TextWrapping.WrapWholeWords,
            IsTextSelectionEnabled = true,
            FontStyle = italic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal,
        };
}
