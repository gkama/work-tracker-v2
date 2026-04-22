using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var databaseBuilder = builder.AddPostgres("worktracker-server")
    .WithImageTag("alpine");

var database = databaseBuilder.AddDatabase("worktracker");

var cache = builder.AddRedis("worktracker-cache")
    .WithImageTag("alpine");

var queue = builder.AddRabbitMQ("worktracker-queue")
    .WithImageTag("3-alpine");

var hostEnvironment = builder.Services.BuildServiceProvider().GetRequiredService<IHostEnvironment>();

if (hostEnvironment.IsDevelopment())
{
    databaseBuilder.WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5052));
    cache.WithRedisInsight();
}

var workTrackerApi = builder.AddProject<Projects.WorkTracker_ApiService>("worktracker-api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(queue)
    .WaitFor(queue);

builder.AddProject<Projects.WorkTracker_Web>("worktracker-web")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(workTrackerApi)
    .WaitFor(workTrackerApi);

builder.Build().Run();
