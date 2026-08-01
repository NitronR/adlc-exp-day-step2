using System.Threading;
using System.Threading.Tasks;

namespace OuterloopLabApi.Providers;

public interface ICurrencyRateProvider
{
    Task<RateQuote> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}
