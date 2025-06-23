# Use ASP.NET runtime image (final container)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project files and NuGet.Config for restoring packages
COPY AccountService/AccountService.csproj AccountService/
COPY Shared.Messages/Shared.Messages.csproj Shared.Messages/
COPY AccountService/NuGet.Config AccountService/

# Restore dependencies
RUN dotnet restore AccountService/AccountService.csproj

# Copy source files
COPY AccountService/ AccountService/
COPY Shared.Messages/ Shared.Messages/

# Publish the project
RUN dotnet publish AccountService/AccountService.csproj -c Release -r linux-x64 --self-contained false -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AccountService.dll"]