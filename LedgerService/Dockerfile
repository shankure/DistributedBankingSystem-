# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only project files for restore
COPY Shared.Messages/Shared.Messages.csproj ./Shared.Messages/
COPY LedgerService/LedgerService.csproj ./LedgerService/
COPY LedgerService/NuGet.Config ./LedgerService/

# Restore dependencies
RUN dotnet restore LedgerService/LedgerService.csproj

# Copy source code
COPY Shared.Messages/ ./Shared.Messages/
COPY LedgerService/ ./LedgerService/

WORKDIR /src/LedgerService
RUN dotnet publish -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# ✅ Install netcat for wait-for-rabbit.sh
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

# Copy published app and wait script
COPY --from=build /app/out .
COPY LedgerService/wait-for-rabbit.sh /app/wait-for-rabbit.sh

# Make script executable
RUN chmod +x /app/wait-for-rabbit.sh

# Run via wait-for-rabbit
ENTRYPOINT ["/app/wait-for-rabbit.sh", "rabbitmq", "5672", "dotnet", "LedgerService.dll"]
