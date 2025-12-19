@echo off
echo.
echo ========================================
echo   ACADEMIA ZOE - DOCKER DEPLOYMENT
echo ========================================
echo.

REM Verificar si Docker está corriendo
docker info >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Docker no está corriendo
    echo Por favor, inicia Docker Desktop y vuelve a intentarlo.
    pause
    exit /b 1
)

echo [OK] Docker está corriendo
echo.

REM Detener contenedores anteriores
echo Deteniendo contenedores existentes...
docker-compose down
echo.

REM Construir y ejecutar
echo Construyendo e iniciando servicios...
docker-compose up -d --build

if errorlevel 1 (
    echo.
    echo [ERROR] Hubo un problema al iniciar los servicios
    echo Ver logs: docker-compose logs
    pause
    exit /b 1
)

echo.
echo ========================================
echo   SERVICIOS INICIADOS EXITOSAMENTE
echo ========================================
echo.
echo SQL Server: localhost:1433
echo   Usuario: sa
echo   Password: Admin123!@#
echo.
echo Aplicacion Web: http://localhost:5000
echo.
echo Credenciales de acceso:
echo   Admin:       admin@academia.local / Admin123!
echo   Coordinador: coordinador@academia.local / Coord123!
echo   Profesor:    profesor@academia.local / Prof123!
echo   Tutor:       tutor@academia.local / Tutor123!
echo   Alumno:      carlos@academia.local / Alumno123!
echo.
echo ========================================
echo.
echo Para ver logs: docker-compose logs -f
echo Para detener:  docker-compose down
echo.

REM Esperar a que los servicios estén listos
echo Esperando a que los servicios esten listos...
timeout /t 10 /nobreak >nul

REM Intentar abrir el navegador
start http://localhost:5000

echo.
echo Presiona cualquier tecla para salir...
pause >nul
