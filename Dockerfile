# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Build mode set to Debug for breakpoints
ARG BUILD_MODE=Debug
WORKDIR /src

# Copy entire solution
COPY . .

# Restore all projects
RUN dotnet restore

# Build all projects in Debug mode
RUN dotnet build -c $BUILD_MODE --no-restore

# --- Stage 2: Publish API + Client ---
FROM build AS publish
ARG BUILD_MODE=Debug

# Publish API in Debug mode
WORKDIR /src/DataSeeker.Api
RUN dotnet publish DataSeeker.Api.csproj -c $BUILD_MODE -o /app/publish/api /p:UseAppHost=false

# Publish Blazor client in Debug mode
WORKDIR /src/DataSeeker.Web
RUN dotnet publish DataSeeker.Web.csproj -c $BUILD_MODE -o /app/publish/web /p:UseAppHost=false

# --- Stage 3: Runtime (Debug) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Copy published API + client
COPY --from=publish /app/publish/api .
COPY --from=publish /app/publish/web ./Client
# Copy Blazor publish into API wwwroot
COPY --from=publish /app/publish/web/wwwroot ./wwwroot


# Expose API ports
EXPOSE 8080
EXPOSE 8081

# Optional: port for remote debugger
EXPOSE 5005

# Needed for Rider / VS Code remote debugging
ENV DOTNET_USE_POLLING_FILE_WATCHER 1
ENV DOTNET_HOST_PATH /usr/share/dotnet/dotnet

# Run the API (with Blazor client served as static files)
ENTRYPOINT ["dotnet", "DataSeeker.Api.dll"]
