using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Auditing;
using OuterloopLabApi.Exceptions;
using OuterloopLabApi.Model;
using OuterloopLabApi.Providers;
using OuterloopLabApi.Services;
using OuterloopLabApi.Controllers;
using Xunit;

public sealed class ConversionControllerTests
{
    private sealed class ThrowingRateProvider : ICurrencyRateProvider
    {
        public Task<RateQuote> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken)
            => throw new UpstreamCurrencyProviderUnavailableException("unavailable");
    }

    private sealed class NoopAuditRepository : IAuditRepository
    {
        public Task CreateAsync(AuditRecordDocument document, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<AuditRecordDocument?> GetByIdAsync(string auditId, CancellationToken cancellationToken) => Task.FromResult<AuditRecordDocument?>(null);
    }

    [Fact]
    public async Task Convert_Returns503ProblemDetails_WhenUpstreamProviderUnavailable()
    {
        var provider = new ThrowingRateProvider();
        var repo = new NoopAuditRepository();
        var service = new ConversionService(provider, repo);
        var controller = new ConversionController(service);

        var request = new ConversionRequest { Amount = 10m, From = "USD", To = "EUR" };
        var result = await controller.Convert(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, objectResult.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(503, problem.Status);
        Assert.Equal("Currency provider unavailable", problem.Title);
    }
}
