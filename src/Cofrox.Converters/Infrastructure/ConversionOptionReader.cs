namespace Cofrox.Converters.Infrastructure;

internal static class ConversionOptionReader
{
    public static string GetString(IReadOnlyDictionary<string, object?> options, string key, string fallback = "")
    {
        if (!options.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value.ToString() ?? fallback;
    }

    public static double GetDouble(IReadOnlyDictionary<string, object?> options, string key, double fallback = 0)
    {
        if (!options.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value switch
        {
            double number => number,
            float number => number,
            int number => number,
            long number => number,
            decimal number => (double)number,
            _ when double.TryParse(value.ToString(), out var parsed) => parsed,
            _ => fallback,
        };
    }

    public static bool GetBool(IReadOnlyDictionary<string, object?> options, string key, bool fallback = false)
    {
        if (!options.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value switch
        {
            bool boolean => boolean,
            _ when bool.TryParse(value.ToString(), out var parsed) => parsed,
            _ => fallback,
        };
    }
}
