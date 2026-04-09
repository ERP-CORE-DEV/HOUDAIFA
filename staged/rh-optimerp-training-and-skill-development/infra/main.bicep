@description('Target deployment environment.')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Azure region for all resources.')
param location string = 'francecentral'

@description('Top-level project name used as a resource prefix.')
param projectName string = 'rh-optimerp'

@description('Short service identifier used in resource names.')
param serviceName string = 'training'

// ---------------------------------------------------------------------------
// Derived naming
// ---------------------------------------------------------------------------
var resourcePrefix = '${projectName}-${serviceName}-${environment}'

var commonTags = {
  environment: environment
  project: projectName
  service: serviceName
  team: 'HOUDAIFA'
  managedBy: 'bicep'
}

// ---------------------------------------------------------------------------
// Log Analytics + Application Insights
// (deployed first — Key Vault stores the connection string)
// ---------------------------------------------------------------------------
module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring-deployment'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    tags: commonTags
  }
}

// ---------------------------------------------------------------------------
// Key Vault
// Must be deployed before CosmosDB so the cosmosdb module can reference it
// to store the connection string directly (avoiding secret exposure in outputs).
// ---------------------------------------------------------------------------
module keyvault 'modules/keyvault.bicep' = {
  name: 'keyvault-deployment'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    appInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
    tags: commonTags
  }
}

// ---------------------------------------------------------------------------
// CosmosDB
// Writes its own connection string secret into Key Vault after account creation.
// ---------------------------------------------------------------------------
module cosmosdb 'modules/cosmosdb.bicep' = {
  name: 'cosmosdb-deployment'
  params: {
    resourcePrefix: resourcePrefix
    location: location
    environment: environment
    keyVaultName: keyvault.outputs.keyVaultName
    tags: commonTags
  }
}

// ---------------------------------------------------------------------------
// Outputs — consumed by Helm values injection in CI/CD
// ---------------------------------------------------------------------------
@description('CosmosDB account endpoint (non-secret).')
output cosmosDbEndpoint string = cosmosdb.outputs.endpoint

@description('Name of the Key Vault holding all service secrets.')
output keyVaultName string = keyvault.outputs.keyVaultName

@description('Key Vault URI.')
output keyVaultUri string = keyvault.outputs.keyVaultUri

@description('Application Insights connection string secret name in Key Vault.')
output appInsightsSecretName string = 'appinsights-connection-string'

@description('CosmosDB connection string secret name in Key Vault.')
output cosmosDbSecretName string = 'cosmosdb-connection-string'

@description('Log Analytics workspace resource ID.')
output logAnalyticsWorkspaceId string = monitoring.outputs.logAnalyticsWorkspaceId
