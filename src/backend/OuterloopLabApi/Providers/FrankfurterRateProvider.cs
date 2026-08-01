using System.Net.Http;
using System.Text.Json;
using OuterloopLabApi.Currency;
using OuterloopLabApi.Exceptions;

namespace OuterloopLabApi.Providers;

public sealed class FrankfurterRateProvider : ICurrencyRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly CurrencyApiSettings _settings;

    public FrankfurterRateProvider(HttpClient httpClient, CurrencyApiSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public async Task<RateQuote> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        try
        {
            var apiBaseUrl = ResolveApiBaseUrl(_settings.BaseUrl);
            var requestUri = $"{apiBaseUrl.TrimEnd('/')}/v2/rate/{fromCurrency.ToUpperInvariant()}/{toCurrency.ToUpperInvariant()}";
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new UpstreamCurrencyProviderUnavailableException("External currency provider returned an error.");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return CurrencyRateResponseMapper.Map(doc.RootElement, fromCurrency, toCurrency);
        }
        catch (UpstreamCurrencyProviderUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new UpstreamCurrencyProviderUnavailableException("External currency provider request timed out.", ex);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new UpstreamCurrencyProviderUnavailableException("External currency provider is unavailable.", ex);
        }
    }

    private static string ResolveApiBaseUrl(string baseUrl)
    {
        // Frankfurter hosts the public API at api.frankfurter.dev, while the docs live at frankfurter.dev.
        if (baseUrl.StartsWith("https://frankfurter.dev", StringComparison.OrdinalIgnoreCase))
            return baseUrl.Replace("https://frankfurter.dev", "https://api.frankfurter.dev", StringComparison.OrdinalIgnoreCase);

        // If the caller already provided api.frankfurter.dev (or another compatible host), use as-is.
        return baseUrl;
    }
}
