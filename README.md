# gkama-aspire-backend

.NET Aspire backend scaffolding with:

- **Authentication API** (`src/Gkama.AuthApi`) for JWT token generation
- **Business API** (`src/Gkama.BusinessApi`) protected with JWT bearer authentication
- **PostgreSQL**, **Redis**, and **RabbitMQ** resources orchestrated by Aspire AppHost (`src/Gkama.AppHost`)

## Run

```bash
dotnet build gkama-aspire-backend.slnx
dotnet run --project src/Gkama.AppHost/Gkama.AppHost.csproj
```

## Auth flow

1. Request token:

```http
POST /api/authentication/token
Content-Type: application/json

{
  "username": "alice",
  "password": "pass"
}
```

2. Call protected business endpoint:

```http
GET /api/business/summary
Authorization: Bearer <accessToken>
```
