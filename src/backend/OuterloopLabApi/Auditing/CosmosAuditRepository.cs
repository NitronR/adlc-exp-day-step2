using System.Net;
using Microsoft.Azure.Cosmos;
using OuterloopLabApi.Model;

namespace OuterloopLabApi.Auditing;

public sealed class CosmosAuditRepository : IAuditRepository
{
    private readonly Container _container;

    public CosmosAuditRepository(Container container)
    {
        _container = container;
    }

    public async Task CreateAsync(AuditRecordDocument document, CancellationToken cancellationToken)
    {
        await _container.CreateItemAsync(document, new PartitionKey(document.Id), cancellationToken: cancellationToken);
    }

    public async Task<AuditRecordDocument?> GetByIdAsync(string auditId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _container.ReadItemAsync<AuditRecordDocument>(
                auditId,
                new PartitionKey(auditId),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
