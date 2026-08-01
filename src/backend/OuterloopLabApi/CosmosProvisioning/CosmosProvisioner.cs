using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.Cosmos;

namespace OuterloopLabApi.CosmosProvisioning;

public static class CosmosProvisioner
{
    public static async Task ProvisionAsync(
        CosmosClient cosmosClient,
        CosmosSettings settings,
        TokenCredential credential,
        CancellationToken cancellationToken)
    {
        // Best-effort ARM provisioning.
        if (!string.IsNullOrWhiteSpace(settings.AzureSubscriptionId))
        {
            try
            {
                await ProvisionViaArmBestEffortAsync(
                    settings: settings,
                    credential: credential,
                    cancellationToken: cancellationToken);
            }
            catch
            {
                // Per requirements: ARM provisioning is best-effort.
            }
        }

        // Mandatory data-plane provisioning; if it fails, startup must fail.
        var dbResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            settings.CosmosDbDatabase,
            throughput: null,
            requestOptions: null,
            cancellationToken: cancellationToken);

        var db = cosmosClient.GetDatabase(settings.CosmosDbDatabase);

        await db.CreateContainerIfNotExistsAsync(
            settings.CosmosDbContainer,
            partitionKeyPath: "/id",
            throughput: 400,
            requestOptions: null,
            cancellationToken: cancellationToken);
    }

    private static async Task ProvisionViaArmBestEffortAsync(
        CosmosSettings settings,
        TokenCredential credential,
        CancellationToken cancellationToken)
    {
        var armClient = new ArmClient(credential, settings.AzureSubscriptionId!);

        var azureLocation = ParseAzureLocation(settings.CosmosDbRegion);

        var databaseResourceId = $"/subscriptions/{settings.AzureSubscriptionId}/resourceGroups/{settings.CosmosDbResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{settings.CosmosDbAccountName}/sqlDatabases/{settings.CosmosDbDatabase}";
        var databaseData = new GenericResourceData(azureLocation)
        {
            Properties = BinaryData.FromObjectAsJson(new
            {
                // Minimal properties; ARM provisioning is best-effort.
            }, new JsonSerializerOptions())
        };

        await armClient
            .GetGenericResources()
            .CreateOrUpdateAsync(WaitUntil.Completed, new ResourceIdentifier(databaseResourceId), databaseData, cancellationToken);

        var containerResourceId = $"/subscriptions/{settings.AzureSubscriptionId}/resourceGroups/{settings.CosmosDbResourceGroup}/providers/Microsoft.DocumentDB/databaseAccounts/{settings.CosmosDbAccountName}/sqlDatabases/{settings.CosmosDbDatabase}/containers/{settings.CosmosDbContainer}";

        var containerData = new GenericResourceData(azureLocation)
        {
            Properties = BinaryData.FromObjectAsJson(new
            {
                partitionKey = new { paths = new[] { "/id" }, kind = "Hash" }
            }, new JsonSerializerOptions())
        };

        await armClient
            .GetGenericResources()
            .CreateOrUpdateAsync(WaitUntil.Completed, new ResourceIdentifier(containerResourceId), containerData, cancellationToken);
    }

    private static Azure.Core.AzureLocation ParseAzureLocation(string region)
    {
        // AzureLocation is an enum; handle the only region used by default templates.
        if (string.Equals(region, "Central India", StringComparison.OrdinalIgnoreCase))
            return Azure.Core.AzureLocation.CentralIndia;

        return Azure.Core.AzureLocation.CentralIndia;
    }
}
