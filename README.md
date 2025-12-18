# AcademiaNet ??

Sistema de gestión académica completo desarrollado en **ASP.NET Core 10.0** con **Razor Pages**, diseñado para administrar ciclos académicos, matrículas, materiales de estudio y múltiples roles de usuario.

---

## ?? Tabla de Contenidos

- [Descripción General](#-descripción-general)
- [Características Principales](#-características-principales)
- [Arquitectura del Sistema](#-arquitectura-del-sistema)
- [Tecnologías Utilizadas](#-tecnologías-utilizadas)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación y Configuración](#-instalación-y-configuración)
- [Estructura de la Base de Datos](#-estructura-de-la-base-de-datos)
- [Roles y Permisos](#-roles-y-permisos)
- [Integración con MercadoPago](#-integración-con-mercadopago)
- [Gestión de Archivos](#-gestión-de-archivos)
- [Usuarios de Prueba](#-usuarios-de-prueba)
- [Configuración de Entorno](#-configuración-de-entorno)
- [API y Servicios](#-api-y-servicios)
- [Despliegue](#-despliegue)
- [Solución de Problemas](#-solución-de-problemas)

---

## ?? Descripción General

**AcademiaNet** es una plataforma integral para la gestión de instituciones educativas que permite:

- ?? Administrar ciclos académicos con fechas de inicio/fin y períodos de matrícula
- ?? Gestionar múltiples roles: Administradores, Coordinadores, Profesores, Tutores y Alumnos
- ?? Procesar pagos de matrícula mediante integración con MercadoPago
- ?? Organizar materiales de estudio por ciclo, semana y tipo
- ?? Administrar sedes, salones, horarios y asignaciones
- ?? Dashboards personalizados por rol con métricas relevantes

---

## ? Características Principales

### Gestión de Ciclos Académicos
- Creación y edición de ciclos con fechas personalizables
- Control de vacantes y períodos de matrícula
- Modalidades: Presencial, Virtual e Híbrido
- Organización por semanas (12 semanas por ciclo)
- Monto de matrícula configurable por ciclo

### Sistema de Matrículas
- Proceso de inscripción con validación de datos
- Integración con MercadoPago para pagos en línea
- Estados de pago: Pendiente, Pagado, Cancelado, Rechazado
- Validación de DNI y datos del alumno
- Registro de apoderados para menores de edad
- Auto-return desde MercadoPago tras completar el pago

### Gestión de Materiales
- Organización por ciclo, semana y tipo de material
- Tipos soportados: PDF, Video, Documento, Enlace, Otros
- Almacenamiento estructurado en FileStorage
- Control de acceso por rol y asignación
- Información de tamaño y fecha de creación
- Vinculación a salones, tutores y semanas específicas

### Sistema de Usuarios y Permisos
- 5 roles principales: Admin, Coordinador, Profesor, Tutor, Alumno
- Autenticación mediante ASP.NET Core Identity
- Permisos granulares mediante Claims
- Dashboards personalizados por rol
- Gestión de usuarios activos/inactivos

### Gestión Académica
- Administración de sedes y salones
- Asignación de tutores a salones (relación N:N)
- Programación de horarios con días y horas específicas
- Gestión de cursos y asignación de profesores
- Seguimiento de alumnos por salón

---

## ??? Arquitectura del Sistema

```
AcademiaNet/
??? Data/
?   ??? AcademicContext.cs          # DbContext principal
?   ??? DbInitializer.cs            # Inicialización y seeding
??? Models/
?   ??? Alumno.cs                   # Entidad de estudiantes
?   ??? Ciclo.cs                    # Ciclos académicos
?   ??? Curso.cs                    # Cursos
?   ??? Horario.cs                  # Horarios de clases
?   ??? Material.cs                 # Materiales de estudio
?   ??? Matricula.cs                # Registro de matrículas
?   ??? Profesor.cs                 # Profesores
?   ??? Salon.cs                    # Salones de clase
?   ??? Sede.cs                     # Sedes físicas
?   ??? Semana.cs                   # Semanas del ciclo
?   ??? Tutor.cs                    # Tutores
?   ??? TutorSalon.cs               # Relación N:N Tutor-Salon
??? Pages/
?   ??? Admin/                      # Páginas del administrador
?   ??? Alumno/                     # Páginas del estudiante
?   ??? Coordinador/                # Páginas del coordinador
?   ??? Profesor/                   # Páginas del profesor
?   ??? Tutor/                      # Páginas del tutor
?   ??? Public/                     # Páginas públicas (matrícula)
?   ??? Materiales/                 # Gestión de materiales
?   ??? Semanas/                    # Gestión de semanas
?   ??? Account/                    # Login/Logout
?   ??? Shared/                     # Layout compartido
??? Services/
?   ??? FileStorageService.cs       # Gestión de archivos
?   ??? MercadoPagoService.cs       # Integración de pagos
??? wwwroot/
?   ??? css/                        # Estilos CSS
?   ??? images/                     # Imágenes y logos
?   ??? lib/                        # Librerías de terceros
??? FileStorage/                    # Almacenamiento de archivos
??? Program.cs                      # Punto de entrada
??? appsettings.json               # Configuración principal
??? appsettings.Development.json   # Configuración desarrollo
```

---

## ??? Tecnologías Utilizadas

### Backend
- **.NET 10.0** - Framework principal
- **ASP.NET Core Razor Pages** - Patrón de UI
- **Entity Framework Core 10.0** - ORM
- **SQL Server** - Base de datos
- **ASP.NET Core Identity** - Autenticación y autorización

### Frontend
- **HTML5 / CSS3**
- **Bootstrap 5** - Framework CSS
- **JavaScript** - Interactividad del cliente

### Integraciones
- **MercadoPago SDK 2.11.0** - Procesamiento de pagos
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** - Sistema de identidad

### Herramientas de Desarrollo
- **Visual Studio 2022** o superior
- **SQL Server Management Studio (SSMS)**
- **Git** - Control de versiones

---

## ?? Requisitos Previos

### Software Necesario
1. **.NET 10.0 SDK** o superior
   - Descargar de: https://dotnet.microsoft.com/download

2. **SQL Server 2019** o superior (o SQL Server Express)
   - Descargar de: https://www.microsoft.com/sql-server/sql-server-downloads

3. **Visual Studio 2022** (recomendado) o VS Code
   - Con workload: "ASP.NET and web development"

### Conocimientos Requeridos
- C# básico/intermedio
- ASP.NET Core Razor Pages
- Entity Framework Core
- SQL Server básico
- HTML/CSS/Bootstrap

---

## ?? Instalación y Configuración

### 1. Clonar el Repositorio

```bash
git clone https://github.com/AndreeCode/AcademiaNet.git
cd AcademiaNet
```

### 2. Configurar la Base de Datos

#### Opción A: SQL Server con Autenticación de Windows (Recomendado para desarrollo local)

Editar `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=academic;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

#### Opción B: SQL Server con Autenticación SQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=academic;User Id=sa;Password=TuPasswordSeguro123!;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

#### Opción C: Azure SQL Database

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:tuservidor.database.windows.net,1433;Database=academic;User ID=tuusuario;Password=tupassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 3. Configurar MercadoPago (Opcional)

Para habilitar pagos en línea, obtener credenciales desde:
https://www.mercadopago.com.pe/developers/panel

Editar `appsettings.json`:

```json
{
  "MercadoPago": {
    "Enabled": true,
    "Environment": "sandbox",
    "AccessToken": "TU_ACCESS_TOKEN_AQUI",
    "PublicKey": "TU_PUBLIC_KEY_AQUI"
  }
}
```

**Modos disponibles:**
- `sandbox`: Ambiente de pruebas (recomendado para desarrollo)
- `production`: Ambiente de producción (pagos reales)

**Para deshabilitar MercadoPago** (solo permitir matrícula sin pago):
```json
"MercadoPago": {
  "Enabled": false
}
```

### 4. Restaurar Paquetes NuGet

```bash
cd AcademiaNet
dotnet restore
```

### 5. Compilar el Proyecto

```bash
dotnet build
```

### 6. Ejecutar la Aplicación

```bash
dotnet run
```

La aplicación estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 7. Inicialización Automática

Al iniciar la aplicación por primera vez:

1. **Se verifica la conexión** a la base de datos
2. **Se crea la base de datos** si no existe
3. **Se aplican migraciones** automáticas si hay archivos de migración
4. **Se ejecuta EnsureCreated()** si no hay migraciones
5. **Se agregan columnas faltantes** mediante SQL dinámico (desarrollo)
6. **Se ejecuta el seeding** de datos iniciales:
   - Roles: Admin, Coordinador, Profesor, Tutor, Alumno
   - Usuarios predefinidos con contraseñas
   - Ciclo de ejemplo: "Ciclo 2025-II"
   - 12 semanas por ciclo
   - 2 Sedes de ejemplo
   - 2 Profesores, 2 Tutores
   - 4 Salones (2 por sede)
   - 8 Alumnos distribuidos en salones
   - Horarios de clase
   - Materiales de ejemplo por semana y salón

---

## ??? Estructura de la Base de Datos

### Diagrama Entidad-Relación Simplificado

```
Sede ???? Salon ???? Alumno
      ?          ??? Horario
      ?          ??? Material
      ?          ??? TutorSalon ?? Tutor
      ?
      ?? Profesor ?? Curso

Ciclo ???? Semana ?? Material
       ??? Matricula ?? Alumno

AspNetUsers (Identity) ?? AspNetUserRoles ?? AspNetRoles
```

### Tablas Principales

#### **Ciclos**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Nombre | nvarchar | Nombre del ciclo |
| FechaInicio | datetime2 | Fecha de inicio de clases |
| FechaFin | datetime2 | Fecha de fin de clases |
| MatriculaInicio | datetime2 | Inicio del período de matrícula |
| MatriculaFin | datetime2 | Fin del período de matrícula |
| Vacantes | int | Número de vacantes (0 = ilimitado) |
| MontoMatricula | decimal(18,2) | Costo de matrícula |
| Modalidad | int | 0=Presencial, 1=Virtual, 2=Híbrido |

#### **Semanas**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| NumeroSemana | int | Número de semana (1-12) |
| CicloId | int | FK a Ciclos |
| FechaInicio | datetime2 | Fecha de inicio de la semana |
| FechaFin | datetime2 | Fecha de fin de la semana |
| Tema | nvarchar(200) | Tema de la semana |
| Descripcion | nvarchar(MAX) | Descripción detallada |
| IsActive | bit | Estado activo/inactivo |

#### **Matriculas**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| AlumnoId | int | FK a Alumnos |
| CicloId | int | FK a Ciclos |
| Monto | decimal(18,2) | Monto a pagar |
| Moneda | nvarchar | Código de moneda (PEN, USD) |
| EstadoPago | int | 0=Pendiente, 1=Pagado, 2=Cancelado, 3=Rechazado |
| MercadoPagoInitPoint | nvarchar(500) | URL de pago MP |
| MercadoPagoPreferenceId | nvarchar(200) | ID de preferencia MP |
| MercadoPagoPaymentId | nvarchar(200) | ID de pago MP |
| FechaPago | datetime2 | Fecha de pago confirmado |
| PaidAmount | decimal(18,2) | Monto pagado |
| CreatedAt | datetime2 | Fecha de creación |

#### **Materiales**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Title | nvarchar | Título del material |
| Description | nvarchar(MAX) | Descripción |
| Week | int | Número de semana (1-12) |
| SemanaId | int | FK a Semanas |
| CicloId | int | FK a Ciclos |
| CursoId | int | FK a Cursos |
| SalonId | int | FK a Salones |
| TutorId | int | FK a Tutores |
| CreatedById | int | ID del usuario creador |
| FileUrl | nvarchar(MAX) | Ruta del archivo |
| FileName | nvarchar(500) | Nombre del archivo |
| FileSize | bigint | Tamaño en bytes |
| TipoMaterial | int | 0=PDF, 1=Video, 2=Documento, 3=Enlace, 4=Otro |
| CreatedAt | datetime2 | Fecha de creación |

#### **Alumnos**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Nombre | nvarchar | Nombre |
| Apellido | nvarchar | Apellido |
| Email | nvarchar | Email (único) |
| DNI | nvarchar(20) | Documento de identidad |
| Telefono | nvarchar(20) | Teléfono |
| Direccion | nvarchar(200) | Dirección |
| DateOfBirth | datetime2 | Fecha de nacimiento |
| NombreApoderado | nvarchar(150) | Nombre del apoderado |
| TelefonoApoderado | nvarchar(20) | Teléfono del apoderado |
| SalonId | int | FK a Salones |
| IsActive | bit | Estado activo/inactivo |

#### **Tutores**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Nombre | nvarchar | Nombre |
| Apellido | nvarchar | Apellido |
| Email | nvarchar | Email (único) |
| IsActive | bit | Estado activo/inactivo |

#### **Profesores**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Nombre | nvarchar | Nombre |
| Apellido | nvarchar | Apellido |
| Email | nvarchar | Email (único) |

#### **Sedes**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Nombre | nvarchar | Nombre de la sede |
| Direccion | nvarchar | Dirección física |

#### **Salones**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| Nombre | nvarchar | Nombre del salón |
| SedeId | int | FK a Sedes |
| ProfesorId | int | FK a Profesores |

#### **Horarios**
| Columna | Tipo | Descripción |
|---------|------|-------------|
| Id | int | Identificador único |
| SalonId | int | FK a Salones |
| Dia | int | Día de la semana (0=Sunday...6=Saturday) |
| HoraInicio | time | Hora de inicio |
| HoraFin | time | Hora de fin |
| LinkMeet | nvarchar | Enlace de videollamada |

#### **TutorSalones** (Tabla de Relación N:N)
| Columna | Tipo | Descripción |
|---------|------|-------------|
| TutorId | int | FK a Tutores |
| SalonId | int | FK a Salones |

### Tablas de ASP.NET Identity

El sistema utiliza las tablas estándar de Identity:
- `AspNetUsers` - Usuarios del sistema
- `AspNetRoles` - Roles (Admin, Coordinador, Profesor, Tutor, Alumno)
- `AspNetUserRoles` - Relación Usuario-Rol
- `AspNetUserClaims` - Claims adicionales
- `AspNetRoleClaims` - Claims por rol

---

## ?? Roles y Permisos

### Roles del Sistema

| Rol | Descripción | Permisos Principales |
|-----|-------------|---------------------|
| **Admin** | Administrador del sistema | Acceso total: gestión de usuarios, ciclos, sedes, configuración global |
| **Coordinador** | Coordinador académico | Gestión de matrículas, visualización de reportes, asignación de salones |
| **Profesor** | Docente | Gestión de cursos asignados, subida de materiales, visualización de alumnos |
| **Tutor** | Tutor de salón | Gestión de materiales por salón, seguimiento de alumnos, visualización de horarios |
| **Alumno** | Estudiante | Acceso a materiales, visualización de horarios, datos personales |

### Claims por Rol

```csharp
Admin       ? permission: all
Coordinador ? permission: manage_enrollment
Profesor    ? permission: manage_courses
Tutor       ? permission: manage_tutors
Alumno      ? permission: student_access
```

### Jerarquía de Acceso

```
Admin
  ??? Todos los módulos
  ??? Gestión de usuarios y configuración

Coordinador
  ??? Dashboard con métricas
  ??? Gestión de matrículas
  ??? Reportes académicos

Profesor
  ??? Dashboard con cursos
  ??? Gestión de materiales
  ??? Visualización de alumnos

Tutor
  ??? Dashboard con salones
  ??? Gestión de materiales por salón
  ??? Seguimiento de alumnos
  ??? Gestión de semanas

Alumno
  ??? Dashboard personalizado
  ??? Materiales del ciclo actual
  ??? Horarios de clase
  ??? Configuración de perfil
```

---

## ?? Integración con MercadoPago

### Configuración

#### 1. Obtener Credenciales

1. Crear cuenta en https://www.mercadopago.com.pe
2. Ir a: **Developers ? Tus integraciones ? Crear aplicación**
3. Obtener:
   - **Access Token** (para el backend)
   - **Public Key** (para el frontend)

#### 2. Configurar en appsettings.json

**Ambiente de Pruebas (Sandbox):**
```json
{
  "MercadoPago": {
    "Enabled": true,
    "Environment": "sandbox",
    "AccessToken": "TEST-4603867478943523-100402-8580797f4481b5ad0530e1a34bbdb563-1997608301",
    "PublicKey": "TEST-fd47cf15-378c-4b11-8294-898f95e3c736"
  }
}
```

**Ambiente de Producción:**
```json
{
  "MercadoPago": {
    "Enabled": true,
    "Environment": "production",
    "AccessToken": "APP_USR-tu-access-token-real",
    "PublicKey": "APP_USR-tu-public-key-real"
  }
}
```

### Flujo de Pago

1. **Alumno inicia matrícula** ? `/Public/Matriculate`
2. **Sistema valida datos** (DNI, email, fechas)
3. **Se crea registro Matricula** con EstadoPago=Pendiente
4. **MercadoPagoService crea Preference**
   - Items: Matrícula + Ciclo
   - BackUrls: Success, Failure, Pending
   - ExternalReference: MATRICULA-{Id}
5. **Usuario redirigido a MercadoPago**
6. **Completa el pago** (tarjeta de crédito/débito, efectivo, etc.)
7. **MercadoPago redirige** a `/Public/MatriculaResult?status=success`
8. **Sistema actualiza** EstadoPago=Pagado
9. **Se crea cuenta de usuario** (IdentityUser + Rol Alumno)
10. **Se asigna a salón** según disponibilidad

### URLs de Retorno

```csharp
BackUrls = new PreferenceBackUrlsRequest
{
    Success = "{baseUrl}/Public/MatriculaResult?status=success",
    Failure = "{baseUrl}/Public/MatriculaResult?status=failure",
    Pending = "{baseUrl}/Public/MatriculaResult?status=pending"
}
```

### Datos Almacenados

En la tabla `Matriculas`:
- `MercadoPagoInitPoint`: URL de pago generada
- `MercadoPagoPreferenceId`: ID de preferencia
- `MercadoPagoPaymentId`: ID del pago (si se completa)
- `FechaPago`: Timestamp del pago
- `PaidAmount`: Monto pagado confirmado

### Tarjetas de Prueba (Sandbox)

| Tarjeta | Número | CVV | Fecha | Resultado |
|---------|--------|-----|-------|-----------|
| Mastercard | 5031 7557 3453 0604 | 123 | 11/25 | Aprobado |
| Visa | 4509 9535 6623 3704 | 123 | 11/25 | Aprobado |
| Rechazada | 5031 4332 1540 6351 | 123 | 11/25 | Rechazado |

Más tarjetas de prueba: https://www.mercadopago.com.pe/developers/es/docs/checkout-api/integration-test/test-cards

---

## ?? Gestión de Archivos

### Estructura de FileStorage

```
FileStorage/
??? Ciclo_2025-II/
?   ??? Semana_01/
?   ?   ??? PDFs/
?   ?   ??? Videos/
?   ?   ??? Documentos/
?   ?   ??? Otros/
?   ??? Semana_02/
?   ?   ??? PDFs/
?   ?   ??? Videos/
?   ?   ??? Documentos/
?   ?   ??? Otros/
?   ??? ...
??? Otro_Ciclo/
    ??? ...
```

### FileStorageService

Servicio encargado de:
- Crear estructura de directorios automáticamente
- Validar tipos y tamaños de archivo (max 100MB)
- Sanitizar nombres de archivo
- Generar nombres únicos (GUID + nombre original)
- Eliminar archivos asociados a materiales
- Retornar rutas relativas para almacenamiento en BD

### Tipos de Material Soportados

```csharp
public enum TipoMaterial
{
    PDF = 0,         // Documentos PDF
    Video = 1,       // Videos (mp4, avi, etc.)
    Documento = 2,   // Word, Excel, PowerPoint
    Enlace = 3,      // URLs externas
    Otro = 4         // Otros formatos
}
```

### Acceso a Archivos

Los archivos se sirven en la ruta: `/files/{relativePath}`

**Seguridad:**
- Solo usuarios autenticados pueden acceder
- Middleware valida `User.Identity.IsAuthenticated`
- Retorna 401 Unauthorized para usuarios no autenticados

---

## ?? Usuarios de Prueba

### Usuarios Predefinidos

Al ejecutar el seeding, se crean los siguientes usuarios:

| Email | Contraseña | Rol | Descripción |
|-------|-----------|-----|-------------|
| admin@academia.local | Admin123! | Admin | Administrador del sistema |
| coordinador@academia.local | Coord123! | Coordinador | Coordinador académico |
| profesor@academia.local | Prof123! | Profesor | Profesor (Juan Perez) |
| luis@academia.local | Prof123! | Profesor | Profesor (Luis Gomez) |
| tutor@academia.local | Tutor123! | Tutor | Tutor (Ana Lopez) |
| marcos@academia.local | Tutor123! | Tutor | Tutor inactivo (Marcos Rojas) |
| carlos@academia.local | Alumno123! | Alumno | Alumno activo - Salón A1 |
| maria@academia.local | Alumno123! | Alumno | Alumno inactivo - Salón A1 |
| pedro@academia.local | Alumno123! | Alumno | Alumno activo - Salón A2 |
| lucia@academia.local | Alumno123! | Alumno | Alumno activo - Salón A2 |
| jose@academia.local | Alumno123! | Alumno | Alumno activo - Salón B1 |
| analt@academia.local | Alumno123! | Alumno | Alumno inactivo - Salón B1 |
| miguel@academia.local | Alumno123! | Alumno | Alumno activo - Salón B2 |
| sofia@academia.local | Alumno123! | Alumno | Alumno activo - Salón B2 |

### Datos de Ejemplo Creados

**Ciclo:**
- Nombre: "Ciclo 2025-II"
- Inicia en 30 días desde hoy
- Termina en 150 días (aprox 5 meses)
- Matrícula abierta (inició hace 5 días, cierra en 25 días)
- Modalidad: Híbrido
- Monto: S/. 1.00 (editable desde Admin)

**Sedes:**
- Zoe - Ayacucho (Calle Ficticia 123)
- Zoe - Huamanga (Av. Principal 456)

**Salones:**
- A1, A2 (Sede Ayacucho)
- B1, B2 (Sede Huamanga)

**Semanas:**
- 12 semanas configuradas por ciclo
- Con fechas auto-calculadas desde el inicio del ciclo

**Materiales:**
- 48 materiales de ejemplo (12 semanas × 4 salones)
- Distribuidos por tutor y salón
- Vinculados a semanas específicas

---

## ?? Configuración de Entorno

### Archivo appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=academic;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "MercadoPago": {
    "Enabled": false,
    "Environment": "sandbox",
    "AccessToken": "TEST-xxx",
    "PublicKey": "TEST-xxx"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Variables de Entorno

El sistema acepta variables de entorno que sobrescriben appsettings.json:

```bash
# Connection String
ConnectionStrings__DefaultConnection="Server=..."

# MercadoPago
MercadoPago__Enabled=true
MercadoPago__Environment=production
MercadoPago__AccessToken="APP_USR-xxx"
MercadoPago__PublicKey="APP_USR-xxx"

# Logging
Logging__LogLevel__Default=Debug
```

### Archivo .env (Desarrollo Local)

Crear archivo `.env` en la raíz (no incluido en Git):

```env
# Base de Datos
DB_SERVER=localhost
DB_NAME=academic
DB_USER=sa
DB_PASSWORD=MiPasswordSeguro123!

# MercadoPago Sandbox
MP_ENABLED=true
MP_ENVIRONMENT=sandbox
MP_ACCESS_TOKEN=TEST-4603867478943523-100402-8580797f4481b5ad0530e1a34bbdb563-1997608301
MP_PUBLIC_KEY=TEST-fd47cf15-378c-4b11-8294-898f95e3c736

# MercadoPago Production (comentado por defecto)
# MP_ENVIRONMENT=production
# MP_ACCESS_TOKEN=APP_USR-xxx
# MP_PUBLIC_KEY=APP_USR-xxx
```

**Nota:** Agregar `.env` a `.gitignore` para no subir credenciales.

### Configuración por Entorno

**Development:**
- Mensajes de error detallados
- Logging extendido
- MercadoPago en modo Sandbox
- TrustServerCertificate=True

**Production:**
- Mensajes de error genéricos
- Logging reducido
- MercadoPago en modo Production
- HTTPS obligatorio
- Certificados SSL válidos

---

## ?? API y Servicios

### MercadoPagoService

**Métodos principales:**

```csharp
Task<(string? initPoint, string? preferenceId)> CreatePreferenceAsync(
    Matricula matricula, 
    Alumno alumno, 
    Ciclo ciclo, 
    string baseUrl
)
```

**Parámetros:**
- `matricula`: Entidad de matrícula (puede tener Id = 0 si aún no está guardada)
- `alumno`: Datos del estudiante (nombre, email)
- `ciclo`: Ciclo académico al que se matricula
- `baseUrl`: URL base de la aplicación (para BackUrls)

**Retorno:**
- `initPoint`: URL de MercadoPago para redirigir al usuario
- `preferenceId`: ID de preferencia creado

### FileStorageService

**Métodos principales:**

```csharp
// Guardar archivo
Task<(bool Success, string? FilePath, string? Error)> SaveFileAsync(
    IFormFile file, 
    string cicloNombre, 
    int numeroSemana, 
    TipoMaterial tipoMaterial
)

// Eliminar archivo
bool DeleteFile(string filePath)

// Obtener rutas
string GetRelativePath(string fullPath)
string GetFullPath(string relativePath)
long GetFileSize(string filePath)
```

### DbInitializer

**Métodos principales:**

```csharp
// Aplicar migraciones y esquemas
Task ApplyMigrationsAsync(AcademicContext context, ILogger? logger)

// Seed de datos iniciales
Task SeedAsync(
    AcademicContext context, 
    UserManager<IdentityUser> userManager, 
    RoleManager<IdentityRole> roleManager, 
    ILogger? logger
)
```

**Funcionalidades:**
- Verifica conectividad a BD
- Aplica migraciones de EF Core
- Fallback a EnsureCreated() si no hay migraciones
- Agrega columnas faltantes dinámicamente (solo desarrollo)
- Crea roles y usuarios predefinidos
- Seed de datos de ejemplo (ciclos, sedes, salones, etc.)

---

## ?? Despliegue

### Publicación en IIS (Windows Server)

1. **Publicar la aplicación:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Configurar IIS:**
   - Instalar ASP.NET Core Hosting Bundle
   - Crear Application Pool (.NET CLR: No Managed Code)
   - Crear sitio web apuntando a carpeta `publish`
   - Configurar permisos de escritura en `FileStorage`

3. **Actualizar appsettings.json:**
   - Connection string de producción
   - MercadoPago en modo production
   - Logging reducido

### Publicación en Azure App Service

1. **Crear recursos en Azure:**
   - App Service (Plan: B1 o superior)
   - Azure SQL Database

2. **Configurar Connection String:**
   - En App Service ? Configuration ? Connection Strings
   - Nombre: `DefaultConnection`
   - Valor: Connection string de Azure SQL
   - Tipo: SQLServer

3. **Configurar variables de entorno:**
   - `MercadoPago__Enabled`: true
   - `MercadoPago__Environment`: production
   - `MercadoPago__AccessToken`: tu access token
   - `MercadoPago__PublicKey`: tu public key

4. **Deploy desde Visual Studio:**
   - Click derecho en proyecto ? Publish
   - Seleccionar Azure ? App Service
   - Publicar

5. **Deploy desde GitHub Actions:**
   ```yaml
   name: Deploy to Azure
   on:
     push:
       branches: [ main ]
   jobs:
     build-and-deploy:
       runs-on: windows-latest
       steps:
         - uses: actions/checkout@v2
         - name: Setup .NET
           uses: actions/setup-dotnet@v1
           with:
             dotnet-version: '10.0.x'
         - name: Build
           run: dotnet build --configuration Release
         - name: Publish
           run: dotnet publish -c Release -o ./publish
         - name: Deploy to Azure
           uses: azure/webapps-deploy@v2
           with:
             app-name: tu-app-service-name
             publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
             package: ./publish
   ```

### Publicación en Docker

**Dockerfile:**

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

**docker-compose.yml:**

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "8080:80"
      - "8081:443"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=academic;User=sa;Password=YourPassword123!
      - MercadoPago__Enabled=true
      - MercadoPago__Environment=production
    depends_on:
      - sqlserver
    volumes:
      - ./FileStorage:/app/FileStorage

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

**Ejecutar:**
```bash
docker-compose up -d
```

---

## ?? Solución de Problemas

### Error: No se puede conectar a la base de datos

**Síntoma:**
```
Unable to connect to the database.
```

**Soluciones:**
1. Verificar que SQL Server esté ejecutándose
2. Comprobar connection string en `appsettings.json`
3. Verificar firewall y puertos (1433)
4. Probar autenticación SQL vs Windows Authentication
5. Revisar permisos del usuario de BD

### Error: MercadoPago no funciona

**Síntoma:**
```
Error al crear preferencia. Access Token no configurado
```

**Soluciones:**
1. Verificar que `MercadoPago__AccessToken` esté configurado
2. Comprobar que el token sea válido (TEST- para sandbox)
3. Verificar que `Enabled` esté en `true`
4. Revisar logs de la aplicación
5. Verificar conectividad a API de MercadoPago

### Error: Archivos no se suben

**Síntoma:**
```
Error al guardar el archivo: Access denied
```

**Soluciones:**
1. Verificar permisos de escritura en carpeta `FileStorage`
2. Comprobar que el directorio existe
3. Validar tamaño del archivo (max 100MB)
4. Revisar logs del FileStorageService

### Error: Migraciones no se aplican

**Síntoma:**
```
An error occurred while applying database migrations.
```

**Soluciones:**
1. Eliminar carpeta `Migrations` si está corrupta
2. Dejar que EnsureCreated() maneje el esquema
3. Crear migraciones manualmente:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Verificar que el modelo no tenga errores

### Error 401 al acceder a archivos

**Síntoma:**
```
401 Unauthorized al intentar ver un archivo
```

**Soluciones:**
1. Verificar que el usuario esté autenticado
2. Comprobar que la ruta `/files/` esté configurada en Program.cs
3. Revisar middleware de autenticación
4. Limpiar cookies y volver a iniciar sesión

### Performance lento al cargar materiales

**Síntoma:**
Demora excesiva al listar materiales

**Soluciones:**
1. Agregar índices a la base de datos:
   ```sql
   CREATE INDEX IX_Materiales_CicloId ON Materiales(CicloId);
   CREATE INDEX IX_Materiales_SalonId ON Materiales(SalonId);
   CREATE INDEX IX_Materiales_Week ON Materiales(Week);
   ```
2. Implementar paginación en las consultas
3. Usar `Include()` para cargar relaciones necesarias
4. Considerar caching para datos estáticos

---

## ?? Recursos Adicionales

### Documentación Oficial

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [MercadoPago Developers](https://www.mercadopago.com.pe/developers)

### Tutoriales Relacionados

- [Razor Pages Tutorial](https://docs.microsoft.com/aspnet/core/tutorials/razor-pages/)
- [EF Core con SQL Server](https://docs.microsoft.com/ef/core/get-started/overview/first-app)
- [Integración de MercadoPago](https://www.mercadopago.com.pe/developers/es/docs/checkout-api/integration-configuration)

### Comunidad

- Stack Overflow: `asp.net-core`, `entity-framework-core`
- GitHub Issues: https://github.com/AndreeCode/AcademiaNet/issues

---

## ?? Licencia

Este proyecto es de código abierto bajo la licencia MIT.

---

## ????? Autor

**AndreeCode**
- GitHub: [@AndreeCode](https://github.com/AndreeCode)
- Repositorio: [AcademiaNet](https://github.com/AndreeCode/AcademiaNet)

---

## ?? Roadmap Futuro

### Funcionalidades Planificadas

- [ ] Sistema de calificaciones y evaluaciones
- [ ] Chat en tiempo real (SignalR)
- [ ] Notificaciones push
- [ ] Generación de certificados PDF
- [ ] Exportación de reportes a Excel
- [ ] Dashboard con gráficos interactivos (Chart.js)
- [ ] API REST para integración con apps móviles
- [ ] Sistema de asistencia con QR
- [ ] Videoconferencia integrada
- [ ] Módulo de tareas y entregables

### Mejoras Técnicas

- [ ] Implementar Repository Pattern
- [ ] Agregar Unit Tests
- [ ] Implementar CQRS
- [ ] Caching con Redis
- [ ] CDN para archivos estáticos
- [ ] Monitoreo con Application Insights
- [ ] CI/CD completo con GitHub Actions
- [ ] Migración a Blazor Server para componentes interactivos

---

## ?? Soporte

Para reportar bugs o solicitar nuevas funcionalidades:
1. Abrir un issue en GitHub
2. Proporcionar logs relevantes
3. Describir pasos para reproducir el problema

---

**¡Gracias por usar AcademiaNet! ??**
