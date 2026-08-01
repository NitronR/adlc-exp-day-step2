namespace OuterloopLabApi.Providers;

public sealed record RateQuote
{
    public required decimal Rate { get; init; }
    public string? ProviderDate { get; init; }
    public string? ProviderBaseCurrency { get; init; }
    public string? ProviderQuoteCurrency { get; init; }
}
