@description('Prefix applied to every resource name.')
param resourcePrefix string

@description('Azure region for the Key Vault.')
param location string

@description('Application Insights connection string to store as a secret.')
@secure()
param appInsightsConnectionString string

@description('Resource tags.')
param tags object

@description('Object ID of the AKS managed identity granted secret-read access.')
param aksManagedIdentityObjectId string = ''

// ---------------------------------------------------------------------------
// Key Vault (RBAC access model — no legacy access policies)
// CosmosDB connection string is written by the cosmosdb module directly,
// avoiding secret exposure through Bicep output plane.
// ---------------------------------------------------------------------------
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${resourcePrefix}-kv'
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 30
    enabledForDeployment: false
    enabledForTemplateDeployment: false
    enabledForDiskEncryption: false
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// ---------------------------------------------------------------------------
// Secrets
// ---------------------------------------------------------------------------
resource appInsightsSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'appinsights-connection-string'
  properties: {
    value: appInsightsConnectionString
    attributes: {
      enabled: true
    }
  }
}

// ---------------------------------------------------------------------------
// RBAC: grant AKS managed identity "Key Vault Secrets User" on the vault
// Only deployed when the identity object ID is provided
// ---------------------------------------------------------------------------
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource aksSecretReaderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(aksManagedIdentityObjectId)) {
  name: guid(keyVault.id, aksManagedIdentityObjectId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: aksManagedIdentityObjectId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------
@description('Key Vault name — used as a Helm value for CSI driver mounts.')
output keyVaultName string = keyVault.name

@description('Key Vault URI.')
output keyVaultUri string = keyVault.properties.vaultUri

@description('Key Vault resource ID.')
output keyVaultId string = keyVault.id
