using '../main.bicep'

// ---------------------------------------------------------------------------
// Production environment parameters
// CosmosDB runs in provisioned throughput mode for predictable latency.
// Provide the AKS managed identity object ID via az deployment group create
// --parameters aksManagedIdentityObjectId=<objectId> at deploy time.
// ---------------------------------------------------------------------------

param environment = 'prod'
param location = 'francecentral'
param projectName = 'rh-optimerp'
param serviceName = 'training'
