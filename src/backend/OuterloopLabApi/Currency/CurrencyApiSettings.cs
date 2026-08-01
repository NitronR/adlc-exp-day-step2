using System;

namespace OuterloopLabApi.Currency;

public sealed class CurrencyApiSettings
{
    public string BaseUrl { get; }

    public static CurrencyApiSettings FromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("CURRENCY_API_BASE_URL");
        var baseUrl = string.IsNullOrWhiteSpace(value)
            ? "https://frankfurter.dev"
            : value!;

        return new CurrencyApiSettings(baseUrl);
    }


    public CurrencyApiSettings(string baseUrl)
    {
        BaseUrl = baseUrl;
    }
}
