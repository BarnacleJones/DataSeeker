FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy all csproj files first to leverage layer caching
COPY ["../DataAccess/DataAccess.csproj", "DataAccess/"]
COPY ["DataSeeker.Api/DataSeeker.Api.csproj", "DataSeeker.Api/"]
COPY ["../Entities/Entities.csproj", "Entities/"]
COPY ["../Service/Service.csproj", "Service/"]
COPY ["../Worker/Worker.csproj", "Worker/"]

# Restore dependencies
WORKDIR /src/DataSeeker.Api
RUN dotnet restore "DataSeeker.Api.csproj"

# Copy all source code
COPY . .
COPY ../DataAccess ../DataAccess
COPY ../Entities ../Entities
COPY ../Service ../Service
COPY ../Worker ../Worker


# Ensure build output folder is writable
RUN mkdir -p /app/build && chmod -R 777 /app/build

# Build
RUN dotnet build "DataSeeker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DataSeeker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataSeeker.Api.dll"]
