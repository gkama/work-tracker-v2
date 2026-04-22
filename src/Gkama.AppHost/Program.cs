var builder = DistributedApplication.CreateBuilder(args);

const string jwtIssuer = "gkama-auth";
const string jwtAudience = "gkama-clients";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ??
                    throw new InvalidOperationException("Jwt:SigningKey must be provided as configuration.");
var authUsername = builder.Configuration["Auth:Username"] ??
                   throw new InvalidOperationException("Auth:Username must be provided as configuration.");
var authPassword = builder.Configuration["Auth:Password"] ??
                   throw new InvalidOperationException("Auth:Password must be provided as configuration.");

var postgres = builder.AddPostgres("postgres");
var database = postgres.AddDatabase("gkamadb");
var redis = builder.AddRedis("redis");
var rabbitMq = builder.AddRabbitMQ("rabbitmq");

var authApi = builder.AddProject("auth-api", "../Gkama.AuthApi/Gkama.AuthApi.csproj")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Auth__Username", authUsername)
    .WithEnvironment("Auth__Password", authPassword);

builder.AddProject("business-api", "../Gkama.BusinessApi/Gkama.BusinessApi.csproj")
    .WithReference(database)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WaitFor(authApi);

builder.Build().Run();
