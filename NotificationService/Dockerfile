# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY NotificationService/NotificationService.csproj NotificationService/
COPY Shared.Messages/Shared.Messages.csproj Shared.Messages/
COPY NotificationService/NuGet.Config NotificationService/

# Restore dependencies
RUN dotnet restore NotificationService/NotificationService.csproj

# Copy all source
COPY NotificationService/ NotificationService/
COPY Shared.Messages/ Shared.Messages/

# Build the app
WORKDIR /src/NotificationService
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "NotificationService.dll"]