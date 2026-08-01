using OuterloopLabApi.Auditing;
using OuterloopLabApi.Exceptions;
using OuterloopLabApi.Model;
using OuterloopLabApi.Providers;
using OuterloopLabApi.Services;
using Xunit;

public sealed class ConversionServiceTests
{
    private sealed class FakeRateProvider : ICurrencyRateProvider
    {
        private readonly RateQuote _quote;

        public FakeRateProvider(RateQuote quote)
        {
            _quote = quote;
        }

        public Task<RateQuote> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
            => Task.FromResult(_quote);
    }

    private sealed class FakeAuditRepository : IAuditRepository
    {
        public AuditRecordDocument? Created { get; private set; }
        public string? LastCreatedId { get; private set; }

        public Task CreateAsync(AuditRecordDocument document, CancellationToken cancellationToken)
        {
            Created = document;
            LastCreatedId = document.Id;
            return Task.CompletedTask;
        }

        public Task<AuditRecordDocument?> GetByIdAsync(string auditId, CancellationToken cancellationToken)
        {
            return Task.FromResult(auditId == Created?.Id ? Created : null);
        }
    }

    [Fact]
    public async Task ConvertAsync_RoundsAwayFromZero_AndPersistsAuditRecord()
    {
        var repo = new FakeAuditRepository();
        var provider = new FakeRateProvider(new RateQuote
        {
            Rate = 1m,
            ProviderDate = "2026-08-01",
            ProviderBaseCurrency = "USD",
            ProviderQuoteCurrency = "EUR",
        });

        var service = new ConversionService(provider, repo);
        var request = new ConversionRequest { Amount = 1.005m, From = "USD", To = "EUR" };

        var before = DateTime.UtcNow;
        var response = await service.ConvertAsync(request, CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.Equal("USD", response.From);
        Assert.Equal("EUR", response.To);
        Assert.Equal(1.005m, response.SourceAmount);
        Assert.Equal(1m, response.RateApplied);
        Assert.Equal(1.01m, response.ConvertedAmount);
        Assert.Equal("2026-08-01", response.ProviderDate);
        Assert.NotEqual(default, response.ExecutedAtUtc);
        Assert.InRange(response.ExecutedAtUtc, before, after);

        Assert.NotNull(repo.Created);
        Assert.Equal(response.AuditId, repo.Created!.Id);
        Assert.Equal(1.01m, repo.Created.ConvertedAmount);
    }

    [Fact]
    public async Task GetAuditAsync_ReturnsStoredRecord()
    {
        var repo = new FakeAuditRepository();
        var provider = new FakeRateProvider(new RateQuote { Rate = 0.9m, ProviderDate = "2026-08-01" });
        var service = new ConversionService(provider, repo);

        var request = new ConversionRequest { Amount = 100m, From = "USD", To = "EUR" };
        var createdResponse = await service.ConvertAsync(request, CancellationToken.None);

        var fetched = await service.GetAuditAsync(createdResponse.AuditId, CancellationToken.None);
        Assert.Equal(createdResponse.AuditId, fetched.AuditId);
        Assert.Equal(createdResponse.ConvertedAmount, fetched.ConvertedAmount);
        Assert.Equal(createdResponse.ExecutedAtUtc, fetched.ExecutedAtUtc);
    }

    [Fact]
    public async Task GetAuditAsync_ThrowsWhenNotFound()
    {
        var repo = new FakeAuditRepository();
        var provider = new FakeRateProvider(new RateQuote { Rate = 0.9m });
        var service = new ConversionService(provider, repo);

        await Assert.ThrowsAsync<AuditNotFoundException>(() =>
            service.GetAuditAsync("does-not-exist", CancellationToken.None));
    }
}
