using Beyond8.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddExternalServiceRegistration();

builder.Build().Run();
