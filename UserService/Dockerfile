# Use ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ./UserService.csproj ./
RUN dotnet restore

# Copy the full source and build
COPY . .
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

# Final container
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]
