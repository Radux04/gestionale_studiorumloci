# Immagine dell'API (StudiorumLoci.Api) per Render o qualsiasi host Docker.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Prima i soli progetti, così il restore resta in cache se cambia solo il codice.
COPY src/StudiorumLoci.Shared/StudiorumLoci.Shared.csproj src/StudiorumLoci.Shared/
COPY src/StudiorumLoci.Api/StudiorumLoci.Api.csproj src/StudiorumLoci.Api/
RUN dotnet restore src/StudiorumLoci.Api/StudiorumLoci.Api.csproj

COPY src/StudiorumLoci.Shared/ src/StudiorumLoci.Shared/
COPY src/StudiorumLoci.Api/ src/StudiorumLoci.Api/
RUN dotnet publish src/StudiorumLoci.Api/StudiorumLoci.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Render comunica la porta da usare nella variabile PORT.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet StudiorumLoci.Api.dll"]
