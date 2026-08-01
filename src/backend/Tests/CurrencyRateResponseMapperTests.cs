using System.Text.Json;
using OuterloopLabApi.Exceptions;
using OuterloopLabApi.Providers;
using Xunit;

public sealed class CurrencyRateResponseMapperTests
{
    [Fact]
    public void Maps_FrankfurterV2_RatePayload()
    {
        var json = "{\"date\":\"2026-08-01\",\"base\":\"USD\",\"quote\":\"EUR\",\"rate\":0.86971}";
        using var doc = JsonDocument.Parse(json);

        var quote = CurrencyRateResponseMapper.Map(doc.RootElement, "USD", "EUR");

        Assert.Equal(0.86971m, quote.Rate);
        Assert.Equal("2026-08-01", quote.ProviderDate);
        Assert.Equal("USD", quote.ProviderBaseCurrency);
        Assert.Equal("EUR", quote.ProviderQuoteCurrency);
    }

    [Fact]
    public void Maps_RatesContainer_Property()
    {
        var json = "{\"date\":\"2026-08-01\",\"base\":\"USD\",\"rates\":{\"EUR\":0.9}}";
        using var doc = JsonDocument.Parse(json);

        var quote = CurrencyRateResponseMapper.Map(doc.RootElement, "USD", "EUR");

        Assert.Equal(0.9m, quote.Rate);
        Assert.Equal("2026-08-01", quote.ProviderDate);
        Assert.Equal("USD", quote.ProviderBaseCurrency);
        Assert.Equal("EUR", quote.ProviderQuoteCurrency);
    }

    [Fact]
    public void Maps_ConversionRatesContainer_Property()
    {
        var json = "{\"date\":\"2026-08-01\",\"base\":\"USD\",\"conversion_rates\":{\"EUR\":0.88}}";
        using var doc = JsonDocument.Parse(json);

        var quote = CurrencyRateResponseMapper.Map(doc.RootElement, "USD", "EUR");

        Assert.Equal(0.88m, quote.Rate);
        Assert.Equal("2026-08-01", quote.ProviderDate);
        Assert.Equal("USD", quote.ProviderBaseCurrency);
        Assert.Equal("EUR", quote.ProviderQuoteCurrency);
    }

    [Fact]
    public void Throws_WhenPayload_IsUnusable()
    {
        var json = "{\"date\":\"2026-08-01\",\"base\":\"USD\"}";
        using var doc = JsonDocument.Parse(json);

        Assert.Throws<UpstreamCurrencyProviderUnavailableException>(() =>
            CurrencyRateResponseMapper.Map(doc.RootElement, "USD", "EUR"));
    }
}
