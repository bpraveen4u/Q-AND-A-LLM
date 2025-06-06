var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AssessmentsApp_WebApi>("WebApi")
       .WithExternalHttpEndpoints();

builder.Build().Run();
