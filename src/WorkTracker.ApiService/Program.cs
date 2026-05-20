using WorkTracker.ApiService.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServiceDefaults();

builder.Services.AddProblemDetails()
    .AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapControllers();

await app.UseTestMigrationAsync(builder);

app.Run();
