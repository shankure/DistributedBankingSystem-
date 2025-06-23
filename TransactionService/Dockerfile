# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only .csproj files first for restore
COPY Shared.Messages/Shared.Messages.csproj ./Shared.Messages/
COPY TransactionAbstractions/TransactionAbstractions.csproj ./TransactionAbstractions/
COPY TransactionService/TransactionService.csproj ./TransactionService/
COPY NuGet.Config ./

# Restore dependencies
RUN dotnet restore ./TransactionService/TransactionService.csproj

# Copy the rest of the source files
COPY Shared.Messages/ ./Shared.Messages/
COPY TransactionAbstractions/ ./TransactionAbstractions/
COPY TransactionService/ ./TransactionService/

# Publish the service
WORKDIR /app/TransactionService
RUN dotnet publish -c Release -o /app/publish --verbosity minimal

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TransactionService.dll"]