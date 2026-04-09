using '../main.bicep'

// ---------------------------------------------------------------------------
// Dev environment parameters
// CosmosDB runs in serverless mode (no throughput cost during idle).
// Key Vault RBAC assignment is skipped until an AKS identity is provisioned.
// ---------------------------------------------------------------------------

param environment = 'dev'
param location = 'francecentral'
param projectName = 'rh-optimerp'
param serviceName = 'training'
