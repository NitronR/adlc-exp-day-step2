using System.Threading;
using System.Threading.Tasks;

using OuterloopLabApi.Model;

namespace OuterloopLabApi.Auditing;

public interface IAuditRepository
{
    Task CreateAsync(AuditRecordDocument document, CancellationToken cancellationToken);
    Task<AuditRecordDocument?> GetByIdAsync(string auditId, CancellationToken cancellationToken);
}
