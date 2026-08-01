namespace OuterloopLabApi.Exceptions;

public sealed class AuditNotFoundException : Exception
{
    public AuditNotFoundException(string auditId)
        : base($"Audit record not found: {auditId}")
    {
        AuditId = auditId;
    }

    public string AuditId { get; }
}
