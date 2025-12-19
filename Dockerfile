# Imagen base para compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar proyecto y restaurar dependencias
COPY ["AcademiaNet/AcademiaNet.csproj", "AcademiaNet/"]
RUN dotnet restore "AcademiaNet/AcademiaNet.csproj"

# Copiar todo el código fuente
COPY . .
WORKDIR "/src/AcademiaNet"

# Compilar la aplicación
RUN dotnet build "AcademiaNet.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "AcademiaNet.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copiar la aplicación publicada
COPY --from=publish /app/publish .

# Instalar herramientas para wait-for-it
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

# Script de inicio que espera a SQL Server
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/docker-entrypoint.sh"]
