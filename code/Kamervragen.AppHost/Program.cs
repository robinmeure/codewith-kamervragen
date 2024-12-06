var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject<Projects.Kamervragen_WebApi>("backend")
    .WithExternalHttpEndpoints();

//var frontend = builder.AddNpmApp("frontend", "../Kamervragen.Frontend", "dev")
//    .WithReference(backend)
//    .WithHttpEndpoint(env: "PORT")
//    .WithExternalHttpEndpoints();

builder.Build().Run();
