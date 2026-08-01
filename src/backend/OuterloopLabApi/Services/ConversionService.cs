using OuterloopLabApi.Auditing;
using OuterloopLabApi.Exceptions;
using OuterloopLabApi.Model;
using OuterloopLabApi.Providers;

namespace OuterloopLabApi.Services;

public sealed class ConversionService
{
    private readonly ICurrencyRateProvider _rateProvider;
    private readonly IAuditRepository _auditRepository;

    public ConversionService(ICurrencyRateProvider rateProvider, IAuditRepository auditRepository)
    {
        _rateProvider = rateProvider;
        _auditRepository = auditRepository;
    }

    public async Task<ConversionResponse> ConvertAsync(ConversionRequest request, CancellationToken cancellationToken)
    {
        var from = request.From.Trim().ToUpperInvariant();
        var to = request.To.Trim().ToUpperInvariant();

        var quote = await _rateProvider.GetRateAsync(from, to, cancellationToken);

        var executedAtUtc = DateTime.UtcNow;
        var auditId = Guid.NewGuid().ToString("n");

        // Financial rounding: 2 decimals, midpoint away from zero.
        var convertedAmount = RoundMoney(request.Amount * quote.Rate);

        var document = new AuditRecordDocument
        {
            Id = auditId,
            From = from,
            To = to,
            SourceAmount = request.Amount,
            RateApplied = quote.Rate,
            ConvertedAmount = convertedAmount,
            ProviderDate = quote.ProviderDate,
            ExecutedAtUtc = executedAtUtc,
        };

        await _auditRepository.CreateAsync(document, cancellationToken);

        return Map(document);
    }

    public async Task<ConversionResponse> GetAuditAsync(string auditId, CancellationToken cancellationToken)
    {
        var record = await _auditRepository.GetByIdAsync(auditId, cancellationToken);
        if (record is null)
            throw new AuditNotFoundException(auditId);

        return Map(record);
    }

    private static decimal RoundMoney(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static ConversionResponse Map(AuditRecordDocument doc) => new ConversionResponse
    {
        AuditId = doc.Id,
        From = doc.From,
        To = doc.To,
        SourceAmount = doc.SourceAmount,
        RateApplied = doc.RateApplied,
        ConvertedAmount = doc.ConvertedAmount,
        ProviderDate = doc.ProviderDate,
        ExecutedAtUtc = doc.ExecutedAtUtc,
    };
}
