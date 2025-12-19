#!/bin/bash

echo ""
echo "========================================"
echo "  ACADEMIA ZOE - DOCKER DEPLOYMENT"
echo "========================================"
echo ""

# Verificar si Docker está corriendo
if ! docker info > /dev/null 2>&1; then
    echo "[ERROR] Docker no está corriendo"
    echo "Por favor, inicia Docker y vuelve a intentarlo."
    exit 1
fi

echo "[OK] Docker está corriendo"
echo ""

# Detener contenedores anteriores
echo "Deteniendo contenedores existentes..."
docker-compose down
echo ""

# Construir y ejecutar
echo "Construyendo e iniciando servicios..."
docker-compose up -d --build

if [ $? -ne 0 ]; then
    echo ""
    echo "[ERROR] Hubo un problema al iniciar los servicios"
    echo "Ver logs: docker-compose logs"
    exit 1
fi

echo ""
echo "========================================"
echo "  SERVICIOS INICIADOS EXITOSAMENTE"
echo "========================================"
echo ""
echo "SQL Server: localhost:1433"
echo "  Usuario: sa"
echo "  Password: Admin123!@#"
echo ""
echo "Aplicación Web: http://localhost:5000"
echo ""
echo "Credenciales de acceso:"
echo "  Admin:       admin@academia.local / Admin123!"
echo "  Coordinador: coordinador@academia.local / Coord123!"
echo "  Profesor:    profesor@academia.local / Prof123!"
echo "  Tutor:       tutor@academia.local / Tutor123!"
echo "  Alumno:      carlos@academia.local / Alumno123!"
echo ""
echo "========================================"
echo ""
echo "Para ver logs: docker-compose logs -f"
echo "Para detener:  docker-compose down"
echo ""

# Esperar a que los servicios estén listos
echo "Esperando a que los servicios estén listos..."
sleep 10

# Intentar abrir el navegador (si estamos en un entorno gráfico)
if command -v xdg-open > /dev/null; then
    xdg-open http://localhost:5000
elif command -v open > /dev/null; then
    open http://localhost:5000
fi

echo ""
echo "¡Listo! Los servicios están corriendo."
