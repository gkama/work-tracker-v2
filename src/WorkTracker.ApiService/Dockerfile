FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first to maximize layer cache on restore
COPY ["WorkTracker.ApiService/WorkTracker.ApiService.csproj", "WorkTracker.ApiService/"]
COPY ["WorkTracker.ServiceDefaults/WorkTracker.ServiceDefaults.csproj", "WorkTracker.ServiceDefaults/"]
COPY ["WorkTracker.Common/WorkTracker.Common.csproj", "WorkTracker.Common/"]
RUN dotnet restore "WorkTracker.ApiService/WorkTracker.ApiService.csproj" --locked-mode

COPY . .
WORKDIR "/src/WorkTracker.ApiService"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WorkTracker.ApiService.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true \
    /p:TieredCompilation=true \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkTracker.ApiService.dll"]
