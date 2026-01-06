var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Sannr_Demo_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Sannr_Demo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService, "apiservice")
    .WaitFor(apiService);

builder.Build().Run();
