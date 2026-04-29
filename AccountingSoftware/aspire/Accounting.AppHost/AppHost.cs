var builder = DistributedApplication.CreateBuilder(args);

var accountingDb = builder.AddSqlServer("accounting-server")
    .WithDataVolume()
    .AddDatabase("accountingdb");

// Add the AccountingApi project with database dependency
// Configure for Azure deployment with proper health checks and observability
var accountingApi = builder.AddProject<Projects.AccountingApi>("accountingapi")
    .WithReference(accountingDb)
    .WithExternalHttpEndpoints()
    .WaitFor(accountingDb); // Enable external access for frontend

// Add the React frontend as a JavaScript project
// Configure for development and production scenarios
var frontend = builder.AddJavaScriptApp("frontend", "../../frontend/accounting-frontend", "start")
    .WithReference(accountingApi)
    .WithExternalHttpEndpoints()
    .WaitFor(accountingApi);

builder.Build().Run();
