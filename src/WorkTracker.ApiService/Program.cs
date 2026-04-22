var builder = WebApplication.CreateBuilder(args);

builder.AddApiServiceDefaults();

builder.Services.AddProblemDetails()
    .AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();
app.MapControllers();

await app.UseTestMigrationAsync(builder);

app.Run();
