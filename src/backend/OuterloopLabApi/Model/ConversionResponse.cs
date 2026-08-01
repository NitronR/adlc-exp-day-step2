using System.Text.Json.Serialization;

namespace OuterloopLabApi.Model;

public sealed class ConversionResponse
{
    [JsonPropertyName("auditId")]
    public required string AuditId { get; init; }

    [JsonPropertyName("from")]
    public required string From { get; init; }

    [JsonPropertyName("to")]
    public required string To { get; init; }

    [JsonPropertyName("sourceAmount")]
    public required decimal SourceAmount { get; init; }

    [JsonPropertyName("rateApplied")]
    public required decimal RateApplied { get; init; }

    [JsonPropertyName("convertedAmount")]
    public required decimal ConvertedAmount { get; init; }

    [JsonPropertyName("providerDate")]
    public string? ProviderDate { get; init; }

    [JsonPropertyName("executedAtUtc")]
    public required DateTime ExecutedAtUtc { get; init; }
}
