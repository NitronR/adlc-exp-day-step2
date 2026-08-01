using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OuterloopLabApi.Model;

public sealed class ConversionRequest
{
    [JsonPropertyName("amount")]
    [Range(typeof(decimal), "0.0000000001", "79228162514264337593543950335")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("from")]
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public required string From { get; init; }

    [JsonPropertyName("to")]
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public required string To { get; init; }
}
