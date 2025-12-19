# ? SISTEMA FINAL - ASIGNAR APODERADOS Y DESPLIEGUE DOCKER

## ?? CARACTERÍSTICAS IMPLEMENTADAS

### 1. ? **ASIGNAR APODERADO A ALUMNO (ADMIN)**

#### Página Creada
**Ruta**: `/Admin/AsignarApoderado`

#### Funcionalidades
- **Formulario de registro de apoderado**
  - Asignar a alumno específico
  - Datos completos: nombre, apellido, DNI, email, teléfono
  - Parentesco seleccionable (Padre, Madre, Tutor, Abuelo, Tío, Otro)
  - Opción de recibir notificaciones

- **Usuario Identity automático**
  - Se crea usuario al asignar apoderado
  - Email como username
  - Password por defecto: `Apoderado123!`
  - Rol "Apoderado" asignado automáticamente

- **Lista de apoderados**
  - Vista de apoderados registrados
  - Información del alumno asociado
  - Opción de eliminar

#### Restricciones
- ? Email único por apoderado
- ? Un alumno puede tener múltiples apoderados
- ? Solo Admin puede asignar apoderados

---

### 2. ? **DATOS DE EJEMPLO (SEEDERS)**

#### Apoderados de Ejemplo
```
1. Roberto Sanchez (Padre de Carlos)
   - Email: roberto.sanchez@example.com
   - Password: Apoderado123!
   - DNI: 40123456

2. María Sanchez (Madre de Carlos)
   - Email: maria.sanchez@example.com
   - Password: Apoderado123!
   - DNI: 40123457

3. Carmen Lopez (Madre de Pedro)
   - Email: carmen.lopez@example.com
   - Password: Apoderado123!
   - DNI: 40234567
```

**Nota**: Carlos tiene 2 apoderados (padre y madre)

---

## ?? ARCHIVOS CREADOS/MODIFICADOS

### Archivos Nuevos
1. `AcademiaNet/Pages/Admin/AsignarApoderado.cshtml`
2. `AcademiaNet/Pages/Admin/AsignarApoderado.cshtml.cs`

### Archivos Modificados
1. `AcademiaNet/Pages/Admin/Dashboard.cshtml`
   - Agregado botón "Asignar Apoderado"
   
2. `AcademiaNet/Data/DbInitializer.cs`
   - Agregados 3 apoderados de ejemplo
   - Usuarios Identity para apoderados

---

## ?? DESPLIEGUE CON DOCKER COMPOSE

### Archivos Docker Existentes
Ya tienes configurados:
- ? `Dockerfile`
- ? `docker-compose.yml`
- ? `docker-entrypoint.sh`
- ? `.dockerignore`
- ? `start-docker.bat` (Windows)
- ? `start-docker.sh` (Linux/Mac)

### Comandos para Despliegue

#### Windows
```cmd
# Opción 1: Usar script batch
.\start-docker.bat

# Opción 2: Comando manual
docker-compose up --build
```

#### Linux/Mac
```bash
# Dar permisos de ejecución
chmod +x start-docker.sh

# Opción 1: Usar script
./start-docker.sh

# Opción 2: Comando manual
docker-compose up --build
```

### Servicios Docker

El `docker-compose.yml` incluye:

1. **SQL Server**
   - Puerto: 1433
   - Usuario: SA
   - Password: (configurado en docker-compose.yml)

2. **AcademiaNet App**
   - Puerto: 5000 (HTTP)
   - Puerto: 5001 (HTTPS)
   - Conectado a SQL Server

---

## ?? CONFIGURACIÓN PARA DESPLIEGUE

### 1. Verificar `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=AcademicDb;User Id=SA;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  }
}
```

### 2. Verificar `docker-compose.yml`

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

  academianet:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=AcademicDb;User Id=SA;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
    depends_on:
      - sqlserver

volumes:
  sqldata:
```

### 3. Verificar `Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AcademiaNet/AcademiaNet.csproj", "AcademiaNet/"]
RUN dotnet restore "AcademiaNet/AcademiaNet.csproj"
COPY . .
WORKDIR "/src/AcademiaNet"
RUN dotnet build "AcademiaNet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AcademiaNet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AcademiaNet.dll"]
```

---

## ?? PASOS PARA DESPLEGAR

### 1. Preparar el Entorno

```bash
# 1. Asegurarse de que Docker esté corriendo
docker --version
docker-compose --version

# 2. Detener contenedores existentes (si los hay)
docker-compose down

# 3. Limpiar imágenes antiguas (opcional)
docker system prune -a
```

### 2. Construir y Desplegar

```bash
# Construir y levantar servicios
docker-compose up --build

# O en segundo plano
docker-compose up --build -d
```

### 3. Verificar Despliegue

```bash
# Ver logs
docker-compose logs -f

# Ver contenedores corriendo
docker ps

# Verificar SQL Server
docker exec -it <sqlserver_container_id> /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'YourStrong!Passw0rd'
```

### 4. Acceder a la Aplicación

```
URL: http://localhost:5000
HTTPS: https://localhost:5001
```

---

## ? VERIFICACIÓN POST-DESPLIEGUE

### 1. Verificar Base de Datos

```sql
-- Conectarse a SQL Server
docker exec -it <sqlserver_container_id> bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'YourStrong!Passw0rd'

-- Verificar tablas
USE AcademicDb;
GO

SELECT COUNT(*) FROM Apoderados;
GO

-- Ver apoderados
SELECT * FROM Apoderados;
GO

-- Ver usuarios Identity de apoderados
SELECT u.Email, r.Name AS Role
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Apoderado';
GO
```

### 2. Probar Login

```
1. Ir a http://localhost:5000/Account/Login

2. Probar apoderados:
   - Email: roberto.sanchez@example.com
   - Password: Apoderado123!

3. Debe redirigir a /Apoderado/Dashboard
```

### 3. Probar Asignación de Apoderados

```
1. Login como Admin
   - Email: admin@academia.local
   - Password: Admin123!

2. Ir a Dashboard > Asignar Apoderado

3. Completar formulario y crear nuevo apoderado

4. Verificar en la lista
```

---

## ?? COMANDOS ÚTILES DOCKER

### Administración de Contenedores

```bash
# Ver logs en tiempo real
docker-compose logs -f academianet

# Detener servicios
docker-compose down

# Reiniciar servicios
docker-compose restart

# Eliminar volúmenes (?? elimina datos)
docker-compose down -v
```

### Debugging

```bash
# Entrar al contenedor de la app
docker exec -it <academianet_container_id> bash

# Ver variables de entorno
docker exec <academianet_container_id> env

# Ver archivos
docker exec <academianet_container_id> ls -la /app
```

### SQL Server

```bash
# Conectar a SQL Server
docker exec -it <sqlserver_container_id> /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'YourStrong!Passw0rd'

# Backup de BD
docker exec <sqlserver_container_id> /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'YourStrong!Passw0rd' -Q "BACKUP DATABASE [AcademicDb] TO DISK = N'/var/opt/mssql/data/AcademicDb.bak'"

# Copiar backup fuera del contenedor
docker cp <sqlserver_container_id>:/var/opt/mssql/data/AcademicDb.bak ./backup/
```

---

## ?? SEGURIDAD

### Cambiar Password de SQL Server

1. **En `docker-compose.yml`**:
```yaml
environment:
  - SA_PASSWORD=TuPasswordSegura123!
```

2. **En `appsettings.json` o variables de entorno**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=sqlserver;Database=AcademicDb;User Id=SA;Password=TuPasswordSegura123!;TrustServerCertificate=True;"
}
```

### Variables de Entorno

Crear archivo `.env` en la raíz:

```env
SA_PASSWORD=YourStrong!Passw0rd
ASPNETCORE_ENVIRONMENT=Production
```

Actualizar `docker-compose.yml`:

```yaml
services:
  sqlserver:
    environment:
      - SA_PASSWORD=${SA_PASSWORD}
```

---

## ?? ESTRUCTURA FINAL DEL PROYECTO

```
AcademiaNet/
??? AcademiaNet/
?   ??? Pages/
?   ?   ??? Admin/
?   ?   ?   ??? Dashboard.cshtml
?   ?   ?   ??? AsignarApoderado.cshtml ? NUEVO
?   ?   ?   ??? AsignarApoderado.cshtml.cs ? NUEVO
?   ?   ??? Apoderado/
?   ?   ?   ??? Dashboard.cshtml
?   ?   ??? ...
?   ??? Data/
?   ?   ??? DbInitializer.cs (? Actualizado con apoderados)
?   ??? ...
??? Dockerfile ?
??? docker-compose.yml ?
??? docker-entrypoint.sh ?
??? .dockerignore ?
??? start-docker.bat ? (Windows)
??? start-docker.sh ? (Linux/Mac)
??? README.md
```

---

## ? CHECKLIST FINAL

- [x] Página AsignarApoderado creada
- [x] Dashboard Admin actualizado con botón
- [x] Seeders con 3 apoderados de ejemplo
- [x] Usuarios Identity para apoderados
- [x] Un alumno puede tener múltiples apoderados
- [x] Archivos Docker configurados
- [x] Scripts de inicio para Windows/Linux
- [x] Documentación completa

---

## ?? CREDENCIALES DE EJEMPLO

### Admin
```
Email: admin@academia.local
Password: Admin123!
```

### Apoderados
```
1. roberto.sanchez@example.com / Apoderado123!
2. maria.sanchez@example.com / Apoderado123!
3. carmen.lopez@example.com / Apoderado123!
```

### Alumnos
```
carlos@academia.local / Alumno123!
pedro@academia.local / Alumno123!
```

### Tutores
```
tutor@academia.local / Tutor123!
```

---

## ?? DESPLIEGUE RÁPIDO

```bash
# 1. Clonar/actualizar repositorio
git pull

# 2. Navegar al directorio
cd AcademiaNet

# 3. Ejecutar Docker Compose
docker-compose up --build -d

# 4. Ver logs
docker-compose logs -f

# 5. Acceder
# http://localhost:5000
```

---

**¡SISTEMA COMPLETO Y LISTO PARA DESPLIEGUE!** ??

**Desarrollado por**: Academia Zoe Team  
**Versión**: 2.0.0 FINAL  
**Última actualización**: Diciembre 2025 ??

---

## ?? SOPORTE

Si necesitas ayuda:
1. Revisa los logs: `docker-compose logs -f`
2. Verifica la conexión a BD
3. Comprueba las variables de entorno
4. Revisa la documentación Docker

**¡ÉXITO EN TU DESPLIEGUE!** ?
