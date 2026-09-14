// Aspire AppHost for the OrcaHello NotificationSystem.
//
// This orchestrates the existing isolated-worker Azure Functions app for local development:
// a single `dotnet run` (or `aspire run`) starts the Functions host together with an
// Azurite storage emulator and the Aspire dashboard (logs, traces, metrics) — no live
// Azure resources or shared credentials required.
//
// The Functions app itself is left completely unchanged.

var builder = DistributedApplication.CreateBuilder(args);

// Azure Storage, run locally as the Azurite emulator. Used here as the Functions host
// storage (the AzureWebJobsStorage the runtime needs to schedule triggers and leases).
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

builder.AddAzureFunctionsProject<Projects.NotificationSystem>("notificationsystem")
    .WithHostStorage(storage);

// Follow-ups (kept out of this first, intentionally minimal change):
//   * Wire the app's own bindings to emulators — the "srkwfound" queue and "EmailList"
//     table (via the "OrcaNotificationStorageSetting" connection), and a Cosmos DB emulator
//     for the "aifororcasmetadatastore_DOCUMENTDB" triggers — so the full pipeline runs
//     locally end-to-end. Until then those connections come from user-secrets /
//     local.settings.json exactly as they do today.
//   * Optionally add a ServiceDefaults project for OpenTelemetry wiring.

builder.Build().Run();
