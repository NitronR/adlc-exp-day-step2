using System.Text.Json;
using OuterloopLabApi.Exceptions;

namespace OuterloopLabApi.Providers;

internal static class CurrencyRateResponseMapper
{
    public static RateQuote Map(JsonElement root, string requestedFrom, string requestedTo)
    {
        var upperFrom = requestedFrom.ToUpperInvariant();
        var upperTo = requestedTo.ToUpperInvariant();

        // Frankfurter v2 (rate endpoint): { "base": "USD", "quote": "EUR", "rate": 0.86971, "date": "2026-08-01" }
        if (TryReadDecimal(root, "rate", out var singleRate))
        {
            return new RateQuote
            {
                Rate = singleRate,
                ProviderDate = TryReadString(root, "date"),
                ProviderBaseCurrency = TryReadString(root, "base") ?? upperFrom,
                ProviderQuoteCurrency = TryReadString(root, "quote") ?? upperTo,
            };
        }

        // Resilience layer for schema drift: support alternate rate property names.
        // Examples of upstream drift we must tolerate: { rates: { EUR: 0.92 } } vs { conversion_rates: { EUR: 0.92 } }
        var rateContainer = root.TryGetProperty("rates", out var ratesProp)
            ? ratesProp
            : root.TryGetProperty("conversion_rates", out var conversionRatesProp)
                ? conversionRatesProp
                : default;

        if (rateContainer.ValueKind == JsonValueKind.Object && rateContainer.TryGetProperty(upperTo, out var rateValue))
        {
            if (rateValue.ValueKind == JsonValueKind.Number && rateValue.TryGetDecimal(out var rateFromContainer))
            {
                return new RateQuote
                {
                    Rate = rateFromContainer,
                    ProviderDate = TryReadString(root, "date"),
                    ProviderBaseCurrency = TryReadString(root, "base") ?? upperFrom,
                    ProviderQuoteCurrency = upperTo,
                };
            }
        }

        throw new UpstreamCurrencyProviderUnavailableException("External currency provider returned an unusable rate payload.");
    }

    private static bool TryReadDecimal(JsonElement element, string propertyName, out decimal value)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetDecimal(out value);
        }

        value = default;
        return false;
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }
}
