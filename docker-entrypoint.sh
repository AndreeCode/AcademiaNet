#!/bin/bash
set -e

echo "?? Esperando a que SQL Server esté listo..."

# Esperar a que SQL Server acepte conexiones
until nc -z sqlserver 1433; do
  echo "? SQL Server no está listo aún - esperando..."
  sleep 2
done

echo "? SQL Server está listo!"

echo "?? Iniciando aplicación AcademiaNet..."

# Iniciar la aplicación
exec dotnet AcademiaNet.dll
