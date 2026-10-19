# syntax=docker/dockerfile:1

# ---- build ------------------------------------------------------------------
# Build on the host's native platform and cross-compile for the target one
# (e.g. build on Apple Silicon, run on linux/amd64 in Azure Container Apps).
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src

# Restore first, using only the files that affect it, so this layer stays cached
# until a dependency changes.
COPY Directory.Build.props Directory.Packages.props .editorconfig ./
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY API/API.csproj API/
RUN dotnet restore API/API.csproj -a $TARGETARCH

COPY Domain/ Domain/
COPY Application/ Application/
COPY Infrastructure/ Infrastructure/
COPY API/ API/
RUN dotnet publish API/API.csproj -c Release -a $TARGETARCH --no-restore -o /app/publish

# ---- runtime ----------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Non-root user that ships with the .NET images; Kestrel listens on 8080.
USER $APP_UID
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]

