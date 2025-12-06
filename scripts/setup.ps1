param(
    [string] $ProjectPath = ".\AcademiaNet",
    [string] $Environment = "Development",
    [string] $MpSandboxToken = "",
    [string] $MpProductionToken = ""
)

# Ir al path del proyecto
Set-Location $ProjectPath

Write-Host "1) Restaurando paquetes..."
dotnet restore

Write-Host "2) Instalando paquetes necesarios (si faltan)..."
$packages = @(
    "Npgsql.EntityFrameworkCore.PostgreSQL",
    "Microsoft.EntityFrameworkCore.Design",
    "Microsoft.EntityFrameworkCore.Tools",
    "Microsoft.AspNetCore.Identity.EntityFrameworkCore"
)

foreach ($pkg in $packages) {
    Write-Host " - dotnet add package $pkg"
    dotnet add package $pkg
}

# Instalar dotnet-ef si falta
if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) {
    Write-Host "Instalando dotnet-ef global tool..."
    dotnet tool install --global dotnet-ef
    $env:PATH += ";" + "$env:USERPROFILE\.dotnet\tools"
} else {
    Write-Host "dotnet-ef ya está instalado."
}

# Establecer entorno para ejecución
$env:ASPNETCORE_ENVIRONMENT = $Environment

# Configurar tokens MercadoPago en variables de entorno (opcionales)
if ($MpSandboxToken -ne "") {
    Write-Host "Guardando token sandbox en variable de entorno (sesión y persistente)..."
    setx MERCADOPAGO__ACCESSTOKENSANDBOX $MpSandboxToken | Out-Null
    $env:MERCADOPAGO__ACCESSTOKENSANDBOX = $MpSandboxToken
}
if ($MpProductionToken -ne "") {
    Write-Host "Guardando token production en variable de entorno (sesión y persistente)..."
    setx MERCADOPAGO__ACCESSTOKENPRODUCTION $MpProductionToken | Out-Null
    $env:MERCADOPAGO__ACCESSTOKENPRODUCTION = $MpProductionToken
}

# Crear migración inicial si no existe (idempotente)
if (-not (Test-Path ".\Migrations")) {
    Write-Host "Creando migración inicial..."
    dotnet ef migrations add InitialCreate --context AcademicContext
} else {
    Write-Host "Migrations folder existe — omitiendo dotnet ef migrations add."
}

Write-Host "Aplicando migraciones a la base de datos..."
dotnet ef database update --context AcademicContext

Write-Host "Fin de setup. Ejecutando la aplicación (usa Ctrl+C para detener)..."
dotnet run
