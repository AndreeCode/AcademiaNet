# ?? DOCKER DEPLOYMENT GUIDE - AcademiaNet

## ?? Requisitos Previos

- Docker Desktop instalado
- Docker Compose instalado
- Puertos 1433 (SQL Server) y 5000 (App) disponibles

## ?? Instrucciones de Despliegue

### 1. Clonar el Repositorio

```bash
git clone https://github.com/AndreeCode/AcademiaNet.git
cd AcademiaNet
```

### 2. Construir y Ejecutar con Docker Compose

```bash
# Construir y ejecutar ambos contenedores
docker-compose up -d --build

# Ver los logs en tiempo real
docker-compose logs -f
```

### 3. Verificar el Estado de los Contenedores

```bash
# Ver contenedores en ejecución
docker ps

# Deberías ver:
# - academianet-sqlserver (puerto 1433)
# - academianet-app (puerto 5000)
```

### 4. Acceder a la Aplicación

Abre tu navegador en:
```
http://localhost:5000
```

## ?? Credenciales por Defecto

### Administrador
- Email: `admin@academia.local`
- Password: `Admin123!`

### Coordinador
- Email: `coordinador@academia.local`
- Password: `Coord123!`

### Profesor
- Email: `profesor@academia.local`
- Password: `Prof123!`

### Tutor
- Email: `tutor@academia.local`
- Password: `Tutor123!`

### Alumno
- Email: `carlos@academia.local`
- Password: `Alumno123!`

## ??? Base de Datos

### Credenciales SQL Server
- Host: `localhost:1433`
- Usuario: `sa`
- Password: `Admin123!@#`
- Base de Datos: `AcademiaNetDB`

### Conectar desde SQL Server Management Studio (SSMS)
```
Server: localhost,1433
Authentication: SQL Server Authentication
Login: sa
Password: Admin123!@#
```

## ?? Datos de Ejemplo (Seeders)

El sistema crea automáticamente:

### ? Alumnos de Ejemplo
- Carlos Sanchez (Activo)
- María Diaz (Inactivo)
- Pedro Lopez (Activo)
- Lucía Vargas (Activa)
- José Martinez (Activo)
- Miguel Quispe (Activo)
- Sofía Reyna (Activa)

### ???????? Apoderados de Ejemplo
1. **Roberto Sanchez** (Padre de Carlos)
   - DNI: 40123456
   - Email: roberto.sanchez@example.com
   - Teléfono: 987654321

2. **Carmen Lopez** (Madre de Pedro)
   - DNI: 40234567
   - Email: carmen.lopez@example.com
   - Teléfono: 987654322

### ?? Ciclo Activo
- **Ciclo 2025-II**
- 100 vacantes
- Matrícula: S/ 1.00 (para pruebas)
- 12 semanas programadas

### ?? Sedes
- Zoe - Ayacucho
- Zoe - Huamanga

### ?? Salones
- A1, A2 (Sede Ayacucho)
- B1, B2 (Sede Huamanga)

## ??? Comandos Útiles

### Detener los Contenedores
```bash
docker-compose down
```

### Reiniciar los Contenedores
```bash
docker-compose restart
```

### Ver Logs de la Aplicación
```bash
docker-compose logs academianet
```

### Ver Logs de SQL Server
```bash
docker-compose logs sqlserver
```

### Eliminar Todo (Contenedores y Volúmenes)
```bash
docker-compose down -v
```

### Reconstruir la Aplicación
```bash
docker-compose up -d --build --force-recreate academianet
```

## ?? Configuración de Pasarelas de Pago

### MercadoPago (Configurado)
- Access Token: `APP_USR-6577725024922213-121915-f3ebe56e68ad0d50cb6de5a9aa0b28ae-702673890`
- Public Key: `APP_USR-2aed66c7-5aa5-4bec-a2a0-dcc5c8f97373`

### Culqi (Configurado - Por Defecto)
- Public Key: `pk_test_xZpBFhfnkH5w9WZL`
- Secret Key: `sk_test_s1z51fGdjFvzWVJO`

## ?? Volúmenes Persistentes

- `sqlserver_data`: Base de datos SQL Server
- `app_uploads`: Archivos subidos (materiales, etc.)

## ?? Puertos Expuestos

- **5000**: Aplicación Web (HTTP)
- **1433**: SQL Server

## ? Funcionalidades Nuevas

### Para Tutores:
1. **Gestionar Notas** (`/Tutor/GestionarNotas`)
   - Agregar notas a alumnos de sus salones
   - Ver historial de notas
   - Eliminar notas
   - Cálculo automático de promedios

2. **Gestionar Salones** (`/Tutor/GestionarSalones`)
   - Crear nuevos salones
   - Asignar salones a sedes
   - Desasignar salones

### Para Admin:
1. **Gestionar Apoderados** (`/Admin/GestionarApoderados`)
   - Registrar apoderados para alumnos
   - Ver lista de apoderados
   - Configurar notificaciones

2. **Generar Reportes PDF** (`/Admin/GenerarReportes`)
   - Reporte de Matriculados
   - Reporte de Estadísticas

### Para Alumnos:
- Ver información de apoderado en el dashboard
- Consultar notas y promedios

## ?? Solución de Problemas

### Error: "Port 1433 is already in use"
```bash
# Detener SQL Server local
net stop MSSQLSERVER

# O cambiar el puerto en docker-compose.yml
ports:
  - "1434:1433"
```

### Error: "Port 5000 is already in use"
```bash
# Cambiar el puerto en docker-compose.yml
ports:
  - "5001:80"
```

### La aplicación no inicia
```bash
# Ver logs detallados
docker-compose logs -f academianet

# Verificar que SQL Server esté saludable
docker-compose ps
```

### Resetear la Base de Datos
```bash
# Eliminar volumen de base de datos
docker-compose down -v

# Volver a crear todo
docker-compose up -d --build
```

## ?? Arquitectura

```
???????????????????????
?   Docker Network    ?
?  (academia-network) ?
???????????????????????
         ?
    ???????????
    ?         ?
????????? ?????????????
? SQL   ? ? Academia  ?
?Server ? ?   Net     ?
?:1433  ? ?   :80     ?
????????? ?????????????
    ?         ?
????????? ?????????????
? Data  ? ? Uploads   ?
?Volume ? ?  Volume   ?
????????? ?????????????
```

## ?? Seguridad en Producción

?? **IMPORTANTE**: Para producción, cambiar:

1. **Contraseña de SQL Server**
   ```yaml
   SA_PASSWORD=TuContraseñaSegura123!
   ```

2. **Credenciales de Pasarelas**
   - Usar credenciales de producción de MercadoPago
   - Usar credenciales de producción de Culqi

3. **Connection String**
   - Actualizar con la nueva contraseña de SA

4. **HTTPS**
   - Configurar certificados SSL
   - Cambiar ASPNETCORE_URLS a https

## ?? Notas

- El sistema crea automáticamente la base de datos al iniciar
- Los seeders se ejecutan solo la primera vez
- Los archivos subidos se almacenan en `/app/wwwroot/uploads`
- El sistema usa migraciones EF Core automáticas

## ?? Soporte

Para reportar problemas:
1. Abrir un issue en GitHub
2. Incluir logs: `docker-compose logs`
3. Incluir versión de Docker: `docker --version`

---

**Desarrollado por**: Academia Zoe Team  
**Versión**: 1.0.0  
**Última actualización**: Diciembre 2025
