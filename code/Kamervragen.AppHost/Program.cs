var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject<Projects.Kamervragen_WebApi>("backend")
    .WithExternalHttpEndpoints();



//builder.AddAzureFunctionsProject<Projects.Kamervragen_IngestData>("kamervragen-ingestdata");



builder.Build().Run();
