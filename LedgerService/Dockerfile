# Use ASP.NET runtime image (final container)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file to restore packages
COPY LedgerService.csproj ./
RUN dotnet restore

# Copy everything else and build the app
COPY . .
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

# Final image with just the published output
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LedgerService.dll"]
