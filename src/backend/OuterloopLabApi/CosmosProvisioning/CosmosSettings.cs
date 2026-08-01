using System;

namespace OuterloopLabApi.CosmosProvisioning;

public sealed record CosmosSettings
{
    public required string CosmosDbUri { get; init; }
    public required string CosmosDbDatabase { get; init; }
    public required string CosmosDbContainer { get; init; }
    public required string CosmosDbAccountName { get; init; }
    public required string CosmosDbResourceGroup { get; init; }
    public required string CosmosDbRegion { get; init; }
    public required string ManagedIdentityClientId { get; init; }

    // Used only for best-effort ARM provisioning.
    public string? AzureSubscriptionId { get; init; }

    public static CosmosSettings FromEnvironment()
    {
        string Require(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Missing required environment variable: {key}");
            return value;
        }

        return new CosmosSettings
        {
            CosmosDbUri = Require("COSMOS_DB_URI"),
            CosmosDbDatabase = Require("COSMOS_DB_DATABASE"),
            CosmosDbContainer = Require("COSMOS_DB_CONTAINER"),
            CosmosDbAccountName = Require("COSMOS_DB_ACCOUNT_NAME"),
            CosmosDbResourceGroup = Require("COSMOS_DB_RESOURCE_GROUP"),
            CosmosDbRegion = Require("COSMOS_DB_REGION"),
            ManagedIdentityClientId = Require("AZURE_MANAGED_IDENTITY_CLIENT_ID"),
            AzureSubscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID"),
        };
    }
}
