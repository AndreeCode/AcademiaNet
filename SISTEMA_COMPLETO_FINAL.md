# ?? ACADEMIA ZOE - SISTEMA COMPLETO DE GESTIÓN ACADÉMICA

## ? ESTADO FINAL DEL SISTEMA

**Fecha:** 20 de Enero de 2025  
**Versión:** 4.0.0 FINAL  
**Estado:** ? LISTO PARA PRODUCCIÓN

---

## ?? TABLA DE CONTENIDOS

1. [Características Principales](#características-principales)
2. [Pasarelas de Pago](#pasarelas-de-pago)
3. [Usuarios del Sistema](#usuarios-del-sistema)
4. [Estructura de la Base de Datos](#estructura-de-la-base-de-datos)
5. [Configuración Inicial](#configuración-inicial)
6. [Flujos de Matrícula](#flujos-de-matrícula)
7. [Dashboards por Rol](#dashboards-por-rol)
8. [Gestión de Notas](#gestión-de-notas)
9. [Sistema de Materiales](#sistema-de-materiales)
10. [Logo y Branding](#logo-y-branding)
11. [Solución de Problemas](#solución-de-problemas)

---

## ?? CARACTERÍSTICAS PRINCIPALES

### ? Sistema Multi-Rol
- **5 Roles:** Admin, Coordinador, Profesor, Tutor, Alumno
- **Permisos Granulares:** Cada rol tiene acceso específico a funcionalidades
- **Navegación Dinámica:** Menú se adapta según el rol del usuario

### ? Sistema de Pagos Multi-Pasarela
- **Culqi** (Por defecto): Tarjetas, Yape, PagoEfectivo
- **MercadoPago**: Tarjetas, efectivo, transferencias
- **Sin Pasarela**: Aprobación manual de matrículas

### ? Gestión Académica Completa
- Ciclos académicos con fechas configurables
- Semanas por ciclo (12 semanas por defecto)
- Materiales por semana/sala/tutor
- Sistema de notas y evaluaciones
- Horarios por salón

### ? Gestión de Matrículas
- Matrícula online con pago automático
- Validación de vacantes
- Períodos de matrícula configurables
- Seguimiento de estado de pago

---

## ?? PASARELAS DE PAGO

### 1. CULQI (Por Defecto) ??

**Características:**
- ? Pago con tarjetas Visa/Mastercard
- ? Yape integrado
- ? PagoEfectivo (banca móvil, agentes)
- ? Cuotéalo BCP
- ? Checkout embebido
- ? Confirmación instantánea

**Configuración en `appsettings.json`:**
```json
{
  "Culqi": {
    "Enabled": true,
    "Environment": "sandbox",
    "PublicKey": "pk_test_xZpBFhfnkH5w9WZL",
    "SecretKey": "sk_test_RptFw7eon6AhkW8L",
    "RsaId": "9944c2af-b394-4cf2-abaa-5b2ebdefaa3e",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n..."
  }
}
```

**Tarjetas de Prueba:**
| Tarjeta | Número | CVV | Fecha | Resultado |
|---------|--------|-----|-------|-----------|
| Visa | 4111 1111 1111 1111 | 123 | 09/25 | ? Aprobado |
| Mastercard | 5111 1111 1111 1118 | 123 | 09/25 | ? Aprobado |
| Rechazada | 4222 2222 2222 2220 | 123 | 09/25 | ? Rechazado |

### 2. MERCADOPAGO ??

**Características:**
- ? Pago con tarjetas
- ? Pago en efectivo
- ? Transferencias bancarias
- ? Redirección a MercadoPago
- ? Auto-return configurado

**Configuración en `appsettings.json`:**
```json
{
  "MercadoPago": {
    "Enabled": false,
    "Environment": "sandbox",
    "AccessToken": "APP_USR-4603867478943523-100402-...",
    "PublicKey": "APP_USR-fd47cf15-378c-4b11-8294-..."
  }
}
```

### 3. SIN PASARELA ?

**Características:**
- ? Registro inmediato del alumno
- ? Matrícula con estado "Pendiente"
- ?? Requiere aprobación manual
- ?? Alumno no tiene acceso hasta aprobación

**Uso:**
Ideal para pruebas o cuando se desea gestionar pagos manualmente.

---

## ?? USUARIOS DEL SISTEMA

### Usuarios Predefinidos

| Rol | Email | Password | Descripción |
|-----|-------|----------|-------------|
| **Admin** | admin@academia.local | Admin123! | Acceso completo al sistema |
| **Coordinador** | coordinador@academia.local | Coord123! | Gestión de matrículas y ciclos |
| **Profesor** | profesor@academia.local | Prof123! | Gestión de cursos y materiales |
| **Tutor** | tutor@academia.local | Tutor123! | Gestión de salones y alumnos |
| **Alumno** | alumno@academia.local | Alumno123! | Acceso a materiales y notas |

### Usuarios de Prueba (8 alumnos)

| Email | Password | Salón | Estado |
|-------|----------|-------|--------|
| carlos@academia.local | Alumno123! | A1 | Activo |
| maria@academia.local | Alumno123! | A1 | Inactivo |
| pedro@academia.local | Alumno123! | A2 | Activo |
| lucia@academia.local | Alumno123! | A2 | Activo |
| jose@academia.local | Alumno123! | B1 | Activo |
| analt@academia.local | Alumno123! | B1 | Inactivo |
| miguel@academia.local | Alumno123! | B2 | Activo |
| sofia@academia.local | Alumno123! | B2 | Activo |

---

## ??? ESTRUCTURA DE LA BASE DE DATOS

### Tablas Principales

1. **AspNetUsers** - Usuarios del sistema (Identity)
2. **AspNetRoles** - Roles (Admin, Coordinador, etc.)
3. **Ciclos** - Ciclos académicos
4. **Semanas** - Semanas por ciclo
5. **Sedes** - Sedes físicas
6. **Salones** - Salones por sede
7. **Tutores** - Tutores del sistema
8. **Profesores** - Profesores del sistema
9. **Alumnos** - Estudiantes matriculados
10. **Matriculas** - Matrículas con info de pago
11. **Materiales** - Recursos educativos
12. **Notas** - Sistema de evaluaciones
13. **Horarios** - Horarios por salón
14. **ConfiguracionPasarelas** - Configuración de pasarela activa

### Diagrama de Relaciones

```
Ciclo (1) -----> (N) Semanas
  |
  ?--> (N) Matriculas
          |
          ?--> (1) Alumno
                  |
                  ?--> (1) Salon
                          |
                          ?--> (1) Sede
                          ?--> (1) Profesor
                          ?--> (N) TutorSalones
                                      |
                                      ?--> (1) Tutor

Semana (1) -----> (N) Materiales
  |
  ?--> (1) Tutor
  ?--> (1) Salon
```

---

## ?? CONFIGURACIÓN INICIAL

### 1. Requisitos Previos

- ? .NET 10 SDK
- ? SQL Server 2019+
- ? Visual Studio 2022 o VS Code

### 2. Configurar Base de Datos

**Opción A: Usar la cadena de conexión por defecto**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=academic;Trusted_Connection=True;..."
  }
}
```

**Opción B: Usar SQL Server Express**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=academic;Trusted_Connection=True;..."
  }
}
```

### 3. Ejecutar la Aplicación

```bash
cd AcademiaNet
dotnet run
```

**Al ejecutar por primera vez:**
- ? Se crea automáticamente la base de datos
- ? Se crean todas las tablas necesarias
- ? Se insertan datos de prueba
- ? Se configura Culqi como pasarela por defecto
- ? Se crean usuarios predefinidos

### 4. Acceder al Sistema

```
URL: https://localhost:5001
Login: admin@academia.local
Password: Admin123!
```

---

## ?? FLUJOS DE MATRÍCULA

### FLUJO CON CULQI (Por Defecto)

```
1. Usuario completa formulario
   ?
2. Click en "Completar Matrícula"
   ?
3. Datos guardados en TempData
   ?
4. Culqi Checkout se abre (modal)
   ?
5. Usuario ingresa datos de tarjeta/Yape
   ?
6. Culqi valida y genera token
   ?
7. Token enviado a /Public/CulqiCallback
   ?
8. Backend crea cargo en Culqi
   ?
9. Si pago exitoso:
   - Crear alumno en BD
   - Crear matrícula (EstadoPago = Pagado)
   - Crear usuario Identity
   - Asignar rol "Alumno"
   ?
10. Redirect a /Account/Login
    ?
11. Alumno inicia sesión
    ?
12. Acceso completo a materiales y notas
```

### FLUJO CON MERCADOPAGO

```
1. Usuario completa formulario
   ?
2. Click en "Completar Matrícula"
   ?
3. Datos guardados en TempData
   ?
4. Redirect a MercadoPago (externa)
   ?
5. Usuario paga en MercadoPago
   ?
6. Auto-return a /Public/MatriculaResult
   ?
7. Backend verifica pago con MP API
   ?
8. Si pago exitoso:
   - Crear alumno/matrícula/usuario
   - EstadoPago = Pagado
   ?
9. Login automático
   ?
10. Acceso completo
```

### FLUJO SIN PASARELA

```
1. Usuario completa formulario
   ?
2. Click en "Completar Matrícula"
   ?
3. Crear alumno/matrícula/usuario
   EstadoPago = Pendiente
   ?
4. Login automático
   ?
5. Dashboard con acceso limitado
   ?? "Tu matrícula está pendiente de aprobación"
   ?
6. Admin/Tutor/Coordinador aprueba pago
   EstadoPago = Pagado
   ?
7. Acceso completo
```

---

## ??? DASHBOARDS POR ROL

### 1. ADMIN DASHBOARD

**Ruta:** `/Admin/Dashboard`

**Estadísticas:**
- Total de alumnos
- Total de matrículas
- Matrículas pendientes
- Vacantes disponibles

**Acciones Rápidas:**
- ? Crear Ciclo
- ? Crear Admin/Coordinador/Tutor
- ? Gestionar Materiales
- ? Gestionar Semanas
- ? Gestionar Notas
- ? **Configurar Pasarela de Pago** ??

**Opciones de Pasarela:**
- Sin Pasarela (Manual)
- MercadoPago
- Culqi

### 2. COORDINADOR DASHBOARD

**Ruta:** `/Coordinador/Dashboard`

**Funciones:**
- Ver estadísticas de matrículas
- Aprobar matrículas pendientes
- Gestionar alumnos
- Ver horarios

### 3. PROFESOR DASHBOARD

**Ruta:** `/Profesor/Dashboard`

**Funciones:**
- Ver mis cursos
- Subir materiales
- Ver alumnos inscritos
- Programar evaluaciones

### 4. TUTOR DASHBOARD

**Ruta:** `/Tutor/Dashboard`

**Funciones:**
- Ver mis salones
- Gestionar alumnos del salón
- Subir materiales por semana
- Aprobar matrículas pendientes

### 5. ALUMNO DASHBOARD

**Ruta:** `/Alumno/Dashboard`

**Funciones:**
- Ver materiales por semana
- Descargar recursos
- Ver horario de clases
- Consultar notas
- Editar perfil

**Estados:**
- ? **Pagado:** Acceso completo
- ?? **Pendiente:** Acceso limitado

---

## ?? GESTIÓN DE NOTAS

### Características

- ? Registro de notas por materia
- ? Diferentes tipos de evaluación
- ? Peso personalizable por evaluación
- ? Cálculo automático de promedio general
- ? Historial completo de evaluaciones

### Tipos de Evaluación

| Código | Tipo | Descripción |
|--------|------|-------------|
| 0 | Examen | Evaluación formal |
| 1 | Práctica | Ejercicios prácticos |
| 2 | Tarea | Trabajos asignados |
| 3 | Participación | Participación en clase |
| 4 | Proyecto | Proyectos integradores |

### Acceso por Rol

| Rol | Acción |
|-----|--------|
| **Admin** | Crear/Editar/Eliminar notas de todos |
| **Coordinador** | Crear/Editar notas de su ciclo |
| **Tutor** | Crear/Editar notas de sus salones |
| **Alumno** | Solo lectura de sus notas |

### Cálculo de Promedio

```csharp
PromedioGeneral = ?(Calificacion × Peso) / ?(Peso)
```

---

## ?? SISTEMA DE MATERIALES

### Características

- ? Subida de archivos (PDF, Word, Excel, PowerPoint)
- ? Organización por semana
- ? Filtrado por salón/tutor
- ? Almacenamiento en `wwwroot/uploads`
- ? Descarga directa para alumnos

### Tipos de Material

| Tipo | Extensiones | Tamaño Máximo |
|------|-------------|---------------|
| Documento | .pdf, .doc, .docx | 10 MB |
| Presentación | .ppt, .pptx | 20 MB |
| Hoja de Cálculo | .xls, .xlsx | 5 MB |
| Video | .mp4 (URL externa) | N/A |

### Flujo de Subida

```
1. Tutor/Profesor selecciona archivo
   ?
2. Selecciona semana y salón
   ?
3. Agrega título y descripción
   ?
4. Sistema guarda en wwwroot/uploads/
   ?
5. Registro en BD con metadata
   ?
6. Alumnos del salón pueden descargar
```

---

## ?? LOGO Y BRANDING

### Logo Actual

**Formatos Disponibles:**
- `logo-zoe.svg` (Principal)
- `logo-zoe.png` (Generado manualmente)

**Colores Corporativos:**
- **Principal:** #800020 (Burdeo)
- **Hover:** #660019 (Burdeo oscuro)
- **Texto:** #FFFFFF (Blanco)

### Generar Logo PNG

1. Abrir `AcademiaNet/wwwroot/images/generate-logo.html`
2. Click en "Descargar Logo PNG"
3. Guardar como `logo-zoe.png`
4. Copiar a `wwwroot/images/`

### Ubicaciones del Logo

| Página | Ubicación | Tamaño |
|--------|-----------|--------|
| Navbar | Header superior | 40x40px |
| Login | Centro del formulario | 150x150px |
| Matrícula | Hero section | 120x120px |
| Dashboards | Sidebar | 100x100px |

---

## ?? SOLUCIÓN DE PROBLEMAS

### Error: "logo-zoe.png 404 Not Found"

**Causa:** Logo no existe en `wwwroot/images/`

**Solución:**
1. Abrir `wwwroot/images/generate-logo.html`
2. Descargar logo PNG
3. Copiar a `wwwroot/images/logo-zoe.png`

O usar SVG:
```html
<img src="~/images/logo-zoe.svg" alt="Academia Zoe" />
```

### Error: "via.placeholder.com ERR_NAME_NOT_RESOLVED"

**Causa:** Sitio externo bloqueado

**Solución:**
Ya corregido. Ahora usa logos locales SVG/PNG con fallback HTML.

### Error: "No se puede conectar a la BD"

**Verificar:**
```sql
-- 1. Verificar SQL Server está ejecutándose
-- 2. Verificar cadena de conexión
-- 3. Verificar permisos de usuario
```

**Solución:**
```bash
# Recrear base de datos
dotnet run
# Al iniciar se creará automáticamente
```

### Error: "Culqi Checkout no se abre"

**Verificar:**
1. `appsettings.json`: `Culqi.Enabled = true`
2. PublicKey correcta
3. Consola del navegador para errores JavaScript

**Solución:**
```javascript
// Verificar en consola:
console.log('Culqi PublicKey:', culqiPublicKey);
console.log('Monto:', montoMatricula);
```

### Error: "Matrícula pendiente no se puede aprobar"

**Causa:** Estado de pago incorrecto

**Solución SQL:**
```sql
UPDATE Matriculas
SET EstadoPago = 1 -- 1 = Pagado
WHERE Id = [ID_MATRICULA];
```

---

## ?? ARCHIVOS IMPORTANTES

### Configuración
- `appsettings.json` - Configuración principal
- `Program.cs` - Punto de entrada

### Servicios
- `CulqiService.cs` - Integración con Culqi
- `MercadoPagoService.cs` - Integración con MP
- `FileStorageService.cs` - Gestión de archivos

### Modelos
- `Alumno.cs`, `Tutor.cs`, `Profesor.cs`
- `Ciclo.cs`, `Semana.cs`, `Matricula.cs`
- `Material.cs`, `Nota.cs`
- `ConfiguracionPasarela.cs`

### Páginas Principales
- `/Public/Matriculate` - Matrícula pública
- `/Admin/Dashboard` - Panel Admin
- `/Admin/ConfigurarPasarela` - Configurar pasarela
- `/Admin/GestionarNotas` - Gestión de notas

---

## ?? PRÓXIMOS PASOS

### Para Producción

1. ? Cambiar credenciales de Culqi a producción
2. ? Cambiar credenciales de MercadoPago a producción
3. ? Configurar certificado SSL
4. ? Configurar dominio personalizado
5. ? Habilitar logs de producción
6. ? Configurar backup automático de BD

### Credenciales de Producción

```json
{
  "Culqi": {
    "Environment": "production",
    "PublicKey": "pk_live_XXXXXXXXXXXXXXXX",
    "SecretKey": "sk_live_XXXXXXXXXXXXXXXX"
  },
  "MercadoPago": {
    "Environment": "production",
    "AccessToken": "APP_USR-PRODUCTION-TOKEN",
    "PublicKey": "APP_USR-PRODUCTION-PUBLIC-KEY"
  }
}
```

---

## ?? SOPORTE

### Contacto
- **Email:** soporte@academiazoe.edu.pe
- **Teléfono:** (066) 123-4567
- **Dirección:** Av. Principal 123, Ayacucho

### Documentación Adicional
- `CULQI_INTEGRATION_COMPLETE.md` - Guía completa de Culqi
- `MERCADOPAGO_INTEGRATION_README.md` - Guía de MercadoPago
- `SISTEMA_NOTAS_README.md` - Sistema de notas
- `LOGO_INTEGRADO_README.md` - Guía de logo

---

## ? CHECKLIST FINAL

### Sistema
- [x] Base de datos creada y poblada
- [x] Usuarios predefinidos funcionando
- [x] Roles y permisos configurados
- [x] Navegación dinámica por rol

### Pasarelas de Pago
- [x] Culqi integrado y funcional
- [x] MercadoPago integrado y funcional
- [x] Sin Pasarela funcionando
- [x] Admin puede cambiar pasarela
- [x] Configuración por defecto: Culqi

### Funcionalidades
- [x] Matrícula online
- [x] Gestión de ciclos
- [x] Gestión de semanas
- [x] Sistema de materiales
- [x] Sistema de notas
- [x] Dashboards por rol
- [x] Logo y branding

### Seguridad
- [x] Autenticación Identity
- [x] Autorización por roles
- [x] Validación de formularios
- [x] Protección CSRF
- [x] Sanitización de archivos

---

**¡EL SISTEMA ESTÁ LISTO PARA PRODUCCIÓN!** ??

Versión: 4.0.0 FINAL  
Fecha: 20 de Enero de 2025  
Estado: ? COMPLETADO Y FUNCIONAL
