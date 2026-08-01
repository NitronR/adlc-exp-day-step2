using System;

namespace OuterloopLabApi.Currency;

public sealed record CurrencyApiSettings
{
    public required string BaseUrl { get; init; }

    public static CurrencyApiSettings FromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL");
        var baseUrl = string.IsNullOrWhiteSpace(value)
            ? "https://frankfurter.dev"
            : value!;

        return new CurrencyApiSettings { BaseUrl = baseUrl };
    }

    public CurrencyApiSettings(string baseUrl)
    {
        BaseUrl = baseUrl;
    }
}
