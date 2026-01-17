using Beyond8.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.AddCommonExtensions();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/test", () => "Hello World");

app.UseHttpsRedirection();
app.UseCommonService();

app.Run();