var builder = DistributedApplication.CreateBuilder(args);

var accountingDb = builder.AddSqlServer("accounting-server")
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

builder.Build().Run();
