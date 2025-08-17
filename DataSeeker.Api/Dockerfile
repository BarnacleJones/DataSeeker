# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy everything from the solution root (context) to /src
COPY . .

# Restore dependencies for the API project
WORKDIR /src/DataSeeker.Api
RUN dotnet restore "DataSeeker.Api.csproj"

# Build the API project (and all referenced projects)
RUN dotnet build "DataSeeker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/DataSeeker.Api
RUN dotnet publish "DataSeeker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "DataSeeker.Api.dll"]
