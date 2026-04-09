@description('Prefix applied to every resource name.')
param resourcePrefix string

@description('Azure region for CosmosDB account.')
param location string

@description('Target deployment environment — controls capacity mode.')
@allowed(['dev', 'staging', 'prod'])
param environment string

@description('Resource tags.')
param tags object

@description('Name of the Key Vault where the connection string secret will be written.')
param keyVaultName string

// ---------------------------------------------------------------------------
// Capacity mode: serverless for dev/staging, provisioned for prod
// ---------------------------------------------------------------------------
var isProduction = environment == 'prod'

var accountProperties = isProduction ? {
  databaseAccountOfferType: 'Standard'
  locations: [
    {
      locationName: location
      failoverPriority: 0
      isZoneRedundant: false
    }
  ]
  consistencyPolicy: {
    defaultConsistencyLevel: 'Session'
  }
  enableAutomaticFailover: false
  capabilities: []
} : {
  databaseAccountOfferType: 'Standard'
  locations: [
    {
      locationName: location
      failoverPriority: 0
      isZoneRedundant: false
    }
  ]
  consistencyPolicy: {
    defaultConsistencyLevel: 'Session'
  }
  enableAutomaticFailover: false
  capabilities: [
    { name: 'EnableServerless' }
  ]
}

// ---------------------------------------------------------------------------
// CosmosDB account
// ---------------------------------------------------------------------------
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: '${resourcePrefix}-cosmos'
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: accountProperties
}

// ---------------------------------------------------------------------------
// Database
// ---------------------------------------------------------------------------
resource trainingDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: 'TrainingSkillDevelopmentDB'
  properties: {
    resource: {
      id: 'TrainingSkillDevelopmentDB'
    }
  }
}

// ---------------------------------------------------------------------------
// Containers — partition keys aligned with appsettings.json container names
// ---------------------------------------------------------------------------
var containers = [
  { name: 'TrainingPlans',          partitionKey: '/CompanyId'  }
  { name: 'TrainingActions',        partitionKey: '/CompanyId'  }
  { name: 'TrainingSessions',       partitionKey: '/ActionId'   }
  { name: 'Enrollments',            partitionKey: '/SessionId'  }
  { name: 'Competencies',           partitionKey: '/CompanyId'  }
  { name: 'CompetencyAssessments',  partitionKey: '/EmployeeId' }
  { name: 'ProfessionalInterviews', partitionKey: '/EmployeeId' }
  { name: 'CpfAccounts',            partitionKey: '/EmployeeId' }
  { name: 'Certifications',         partitionKey: '/CompanyId'  }
  { name: 'TrainingProviders',      partitionKey: '/CompanyId'  }
  { name: 'Opcos',                  partitionKey: '/CompanyId'  }
  { name: 'AlternanceContracts',    partitionKey: '/EmployeeId' }
  { name: 'TrainingObligations',    partitionKey: '/CompanyId'  }
  { name: 'TrainingEvaluations',    partitionKey: '/SessionId'  }
  { name: 'BilanDeCompetences',     partitionKey: '/EmployeeId' }
  { name: 'TutoringPrograms',       partitionKey: '/CompanyId'  }
  { name: 'FundingApplications',    partitionKey: '/CompanyId'  }
  { name: 'VaeProjects',            partitionKey: '/EmployeeId' }
]

resource cosmosContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [
  for container in containers: {
    parent: trainingDatabase
    name: container.name
    properties: {
      resource: {
        id: container.name
        partitionKey: {
          paths: [ container.partitionKey ]
          kind: 'Hash'
          version: 2
        }
        indexingPolicy: {
          indexingMode: 'consistent'
          automatic: true
          includedPaths: [ { path: '/*' } ]
          excludedPaths: [ { path: '/"_etag"/?' } ]
        }
      }
    }
  }
]

// ---------------------------------------------------------------------------
// Write connection string directly into Key Vault (never exposed as output)
// ---------------------------------------------------------------------------
resource existingKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource cosmosDbSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: existingKeyVault
  name: 'cosmosdb-connection-string'
  properties: {
    value: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
    attributes: {
      enabled: true
    }
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------
@description('CosmosDB account endpoint URL (non-secret).')
output endpoint string = cosmosAccount.properties.documentEndpoint

@description('Key Vault secret URI for the CosmosDB connection string.')
output cosmosDbSecretUri string = cosmosDbSecret.properties.secretUri

@description('CosmosDB account resource ID.')
output accountId string = cosmosAccount.id
