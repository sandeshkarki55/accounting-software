using Aspire.Hosting;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server database for accounting data
// Using SQL Server for robust ACID compliance needed for financial data
var accountingDb = builder.AddSqlServer("sqlserver")
    .WithDataVolume()
    .AddDatabase("accountingdb");

// Add the AccountingApi project with database dependency
// Configure for Azure deployment with proper health checks and observability
var accountingApi = builder.AddProject<Projects.AccountingApi>("accountingapi")
    .WithReference(accountingDb)
    .WithExternalHttpEndpoints(); // Enable external access for frontend

// Add the React frontend as an npm project
// Configure for development and production scenarios
var frontend = builder.AddNpmApp("frontend", "../AccountingSoftware/frontend/accounting-frontend")
    .WithReference(accountingApi)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// Configure environment-specific settings for Azure deployment
if (builder.Environment.IsProduction())
{
    // Production configuration for Azure deployment
    accountingApi.WithReplicas(2); // High availability for API only
    // Note: WithReplicas() is not available for npm apps in Aspire
}

builder.Build().Run();
