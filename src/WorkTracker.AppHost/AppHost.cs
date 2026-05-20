using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var hostEnvironment = builder.Services.BuildServiceProvider().GetRequiredService<IHostEnvironment>();
var isDevelopment = hostEnvironment.IsDevelopment();

var databaseBuilder = builder.AddPostgres("worktracker-server")
    .WithImageTag("alpine");

var database = databaseBuilder.AddDatabase("worktracker");

var cache = builder.AddRedis("worktracker-cache")
    .WithImageTag("alpine");

var rabbitMqUsername = builder.AddParameter("rabbitmq-username", value: "guest", secret: false);
var rabbitMqPassword = builder.AddParameter("rabbitmq-password", value: "guest", secret: true);

var queue = builder.AddRabbitMQ("worktracker-queue", userName: rabbitMqUsername, password: rabbitMqPassword)
    .WithImageTag(isDevelopment ? "management-alpine" : "alpine");

if (isDevelopment)
{
    databaseBuilder.WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5052));
    cache.WithRedisInsight();
    queue.WithManagementPlugin();
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
