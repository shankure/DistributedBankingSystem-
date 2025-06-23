# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Shared.Messages/Shared.Messages.csproj ./Shared.Messages/
COPY TransactionAbstractions/TransactionAbstractions.csproj ./TransactionAbstractions/
COPY FraudDetectionService/FraudDetectionService.csproj ./FraudDetectionService/
COPY FraudDetectionService/NuGet.Config ./FraudDetectionService/

RUN dotnet restore FraudDetectionService/FraudDetectionService.csproj

COPY Shared.Messages/ ./Shared.Messages/
COPY TransactionAbstractions/ ./TransactionAbstractions/
COPY FraudDetectionService/ ./FraudDetectionService/

WORKDIR /src/FraudDetectionService
RUN dotnet publish -c Release -o /app/out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# ✅ Install netcat (for wait-for-rabbit.sh)
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

# Copy built app and wait script
COPY --from=build /app/out .
COPY FraudDetectionService/wait-for-rabbit.sh /app/wait-for-rabbit.sh

# Make script executable
RUN chmod +x /app/wait-for-rabbit.sh

# Use the wait script as entry point
ENTRYPOINT ["/app/wait-for-rabbit.sh", "rabbitmq", "5672", "dotnet", "FraudDetectionService.dll"]
