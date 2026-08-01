namespace OuterloopLabApi.Exceptions;

public sealed class UpstreamCurrencyProviderUnavailableException : Exception
{
    public UpstreamCurrencyProviderUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
