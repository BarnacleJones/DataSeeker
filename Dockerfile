# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Default build mode: Release. Use Debug for local debug builds.
ARG BUILD_MODE=Debug
WORKDIR /src

# Copy the entire solution into the container
COPY . .

# Restore dependencies for the API project (and all referenced projects)
WORKDIR /src/DataSeeker.Api
RUN dotnet restore "DataSeeker.Api.csproj"

# Build API in the specified mode
RUN dotnet build "DataSeeker.Api.csproj" -c $BUILD_MODE -o /app/build

# --- Stage 2: Publish ---
FROM build AS publish
ARG BUILD_MODE=Release
WORKDIR /src/DataSeeker.Api
RUN dotnet publish "DataSeeker.Api.csproj" -c $BUILD_MODE -o /app/publish /p:UseAppHost=false

# --- Stage 3: Runtime ---
# Use SDK image for Debug (allows dotnet watch / remote debug)
# Use lightweight runtime image for Release
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG BUILD_MODE=Release
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Expose API ports
EXPOSE 8080
EXPOSE 8081

# Expose remote debug port (optional)
EXPOSE 5005

# Needed for Rider remote debugger
ENV DOTNET_USE_POLLING_FILE_WATCHER 1
ENV DOTNET_HOST_PATH /usr/share/dotnet/dotnet

ENTRYPOINT ["dotnet", "DataSeeker.Api.dll"]