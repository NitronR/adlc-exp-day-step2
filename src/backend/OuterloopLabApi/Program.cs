using Azure.Identity;
using Microsoft.Azure.Cosmos;

using OuterloopLabApi.Auditing;
using OuterloopLabApi.Currency;
using OuterloopLabApi.CosmosProvisioning;
using OuterloopLabApi.Providers;
using OuterloopLabApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Explicitly bind from environment variables (no appsettings fallbacks).
var cosmosSettings = CosmosSettings.FromEnvironment();
var currencyApiBaseUrl = CurrencyApiSettings.FromEnvironment().BaseUrl;

var managedIdentityClientId = cosmosSettings.ManagedIdentityClientId;
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ManagedIdentityClientId = managedIdentityClientId,
});

// Startup lifecycle requirement: attempt ARM provisioning first (best-effort),
// then do token-authenticated data-plane Create*IfNotExistsAsync.
var cosmosClient = new CosmosClient(cosmosSettings.CosmosDbUri, credential);

await CosmosProvisioner.ProvisionAsync(
    cosmosClient: cosmosClient,
    settings: cosmosSettings,
    credential: credential,
    cancellationToken: CancellationToken.None);

builder.Services.AddControllers();

builder.Services.AddSingleton<IAuditRepository>(sp =>
    new CosmosAuditRepository(
        cosmosClient.GetDatabase(cosmosSettings.CosmosDbDatabase).GetContainer(cosmosSettings.CosmosDbContainer)));

builder.Services.AddHttpClient<FrankfurterRateProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddTransient<ICurrencyRateProvider>(sp =>
    sp.GetRequiredService<FrankfurterRateProvider>());

builder.Services.AddSingleton(new CurrencyApiSettings(currencyApiBaseUrl));
builder.Services.AddSingleton<ConversionService>();

var app = builder.Build();

app.MapControllers();

app.Run();
