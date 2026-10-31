# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Nachytej/Nachytej.csproj src/Nachytej/
RUN dotnet restore src/Nachytej/Nachytej.csproj
COPY src/Nachytej/ src/Nachytej/
RUN dotnet publish src/Nachytej/Nachytej.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Složka pro anonymní statistiky musí být zapisovatelná pro uživatele "app"
RUN mkdir -p /app/data && chown app:app /app/data
USER app
ENV ASPNETCORE_ENVIRONMENT=Production
# Render předává port v proměnné PORT (lokálně 8080)
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet Nachytej.dll"]
