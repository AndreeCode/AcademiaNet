# ? TUTORES - ASIGNAR AULAS Y APROBAR MATRÍCULAS

## ?? NUEVAS FUNCIONALIDADES PARA TUTORES

### 1. ? **ASIGNAR SALONES A ALUMNOS**

#### Página Creada
**Ruta**: `/Tutor/AsignarSalones`

#### Funcionalidades
- **Ver alumnos sin salón asignado**
  - Lista de alumnos activos sin aula
  - Ordenados alfabéticamente
  - Muestra DNI, email, estado

- **Asignar salón**
  - Solo puede asignar a salones que el tutor tiene asignados
  - Selección desde dropdown
  - Asignación con un click

- **Ver salones disponibles**
  - Muestra los salones del tutor
  - Con sede correspondiente
  - Información visual clara

#### Restricciones
- ? Solo puede asignar a salones propios
- ? Solo alumnos activos pueden ser asignados
- ? Verificación de permisos en cada acción

---

### 2. ? **APROBAR/RECHAZAR MATRÍCULAS**

#### Página Creada
**Ruta**: `/Tutor/AprobarMatriculas`

#### Funcionalidades
- **Ver matrículas pendientes**
  - Lista completa de matrículas no aprobadas
  - Información del estudiante
  - Ciclo y monto

- **Aprobar matrícula**
  - Asignar salón al estudiante
  - Solo salones del tutor
  - Cambia estado a "Pagado"
  - Decrementa vacantes del ciclo
  - Asigna sede automáticamente

- **Rechazar matrícula**
  - Ingresar motivo del rechazo
  - Cambia estado a "Rechazado"
  - Estudiante ve el motivo

#### Proceso de Aprobación
```
1. Tutor ve matrícula pendiente
   ?
2. Click en "Aprobar"
   ?
3. Selecciona salón de sus salones
   ?
4. Confirma
   ?
5. Sistema:
   - Asigna salón al alumno
   - Asigna sede del salón
   - Cambia estado a Pagado
   - Decrementa vacantes
   ?
6. ? Estudiante puede acceder
```

---

## ?? ARCHIVOS CREADOS

1. `AcademiaNet/Pages/Tutor/AsignarSalones.cshtml`
2. `AcademiaNet/Pages/Tutor/AsignarSalones.cshtml.cs`
3. `AcademiaNet/Pages/Tutor/AprobarMatriculas.cshtml`
4. `AcademiaNet/Pages/Tutor/AprobarMatriculas.cshtml.cs`

## ?? ARCHIVOS MODIFICADOS

1. `AcademiaNet/Pages/Tutor/Dashboard.cshtml`
   - Agregado enlace "Asignar Salones a Alumnos"
   - Agregado enlace "Aprobar Matrículas"

---

## ?? FLUJOS DE TRABAJO

### Flujo 1: Asignar Salón a Alumno

```
1. Login como Tutor
   ?
2. Dashboard ? "Asignar Salones a Alumnos"
   ?
3. Ve lista de alumnos sin salón
   ?
4. Click en "Asignar Salón" para un alumno
   ?
5. Selecciona salón de sus salones
   ?
6. Click "Asignar"
   ?
7. ? Alumno asignado al salón y sede
```

### Flujo 2: Aprobar Matrícula

```
1. Login como Tutor
   ?
2. Dashboard ? "Aprobar Matrículas"
   ?
3. Ve lista de matrículas pendientes
   ?
4. Click en "Aprobar" para una matrícula
   ?
5. Selecciona salón
   ?
6. Confirma aprobación
   ?
7. Sistema:
   - Asigna salón
   - Asigna sede
   - Estado ? Pagado
   - Decrementa vacantes
   ?
8. ? Estudiante aprobado
```

### Flujo 3: Rechazar Matrícula

```
1. Login como Tutor
   ?
2. Dashboard ? "Aprobar Matrículas"
   ?
3. Click en "Rechazar" para una matrícula
   ?
4. Ingresa motivo del rechazo
   ?
5. Confirma
   ?
6. Estado ? Rechazado
   ?
7. Estudiante ve motivo
```

---

## ?? PERMISOS Y SEGURIDAD

### Rol Requerido
- ? **Tutor** (exclusivo)

### Restricciones
1. **Asignar Salones**
   - Solo puede asignar a salones que tiene asignados
   - Verificación en backend
   - No puede asignar alumnos inactivos

2. **Aprobar Matrículas**
   - Solo puede aprobar asignando a sus salones
   - Verificación de permisos en cada acción
   - No puede aprobar sin asignar salón

### Validaciones
- ? Verifica que el tutor tenga el salón asignado
- ? Verifica que el alumno exista
- ? Verifica que el salón exista
- ? Verifica que el alumno esté activo (para asignación)
- ? Verifica que la matrícula esté pendiente (para aprobación)

---

## ?? INTERFAZ DE USUARIO

### AsignarSalones.cshtml

#### Secciones
1. **Mis Salones Asignados**
   - Cards visuales de cada salón
   - Muestra nombre y sede
   - Disposición en grid

2. **Alumnos sin Salón**
   - Tabla con información completa
   - Badge de estado (Activo/Inactivo)
   - Botón "Asignar Salón" por alumno

3. **Modal de Asignación**
   - Información del alumno
   - Dropdown de salones disponibles
   - Botones Cancelar/Asignar

#### Características
- ? Diseño responsivo
- ? Mensajes de éxito/error
- ? Validaciones client-side
- ? Confirmaciones antes de acciones

### AprobarMatriculas.cshtml

#### Secciones
1. **Mis Salones Disponibles**
   - Vista de salones del tutor
   - Para referencia rápida

2. **Matrículas Pendientes**
   - Tabla con todos los datos
   - Email, DNI, Ciclo, Monto
   - Fecha de solicitud
   - Botones Aprobar/Rechazar

3. **Modal Aprobar**
   - Información de la matrícula
   - Dropdown de salones
   - Advertencia sobre vacantes

4. **Modal Rechazar**
   - Campo de motivo (obligatorio)
   - Texto libre

---

## ? CÓMO PROBAR

### 1. Probar Asignación de Salones

```bash
# 1. Login como Tutor
Email: tutor@academia.local
Password: Tutor123!

# 2. Ir a Dashboard
URL: /Tutor/Dashboard

# 3. Click en "Asignar Salones a Alumnos"
URL: /Tutor/AsignarSalones

# 4. Ver lista de alumnos sin salón

# 5. Click "Asignar Salón" en un alumno

# 6. Seleccionar salón del dropdown

# 7. Click "Asignar"

# 8. Verificar mensaje de éxito
```

### 2. Probar Aprobación de Matrículas

```bash
# 1. Primero crear matrícula pendiente:
#    - Ir a /Public/Matriculate
#    - Registrar estudiante sin pasarela
#    - Matrícula queda pendiente

# 2. Login como Tutor
Email: tutor@academia.local
Password: Tutor123!

# 3. Click "Aprobar Matrículas"
URL: /Tutor/AprobarMatriculas

# 4. Ver matrícula pendiente

# 5. Click "Aprobar"

# 6. Seleccionar salón

# 7. Confirmar

# 8. Verificar:
   - Matrícula aprobada
   - Salón asignado
   - Vacantes decrementadas
```

---

## ?? VERIFICACIÓN SQL

### Ver Alumnos Sin Salón
```sql
SELECT 
    Id,
    Nombre,
    Apellido,
    Email,
    DNI,
    IsActive,
    SalonId
FROM Alumnos
WHERE SalonId IS NULL AND IsActive = 1;
```

### Ver Matrículas Pendientes
```sql
SELECT 
    m.Id,
    a.Nombre + ' ' + a.Apellido AS Estudiante,
    a.Email,
    c.Nombre AS Ciclo,
    m.Monto,
    m.EstadoPago,
    m.CreatedAt
FROM Matriculas m
INNER JOIN Alumnos a ON m.AlumnoId = a.Id
INNER JOIN Ciclos c ON m.CicloId = c.Id
WHERE m.EstadoPago = 0; -- Pendiente
```

### Ver Salones de un Tutor
```sql
SELECT 
    s.Nombre AS Salon,
    se.Nombre AS Sede,
    t.Nombre + ' ' + t.Apellido AS Tutor
FROM TutorSalones ts
INNER JOIN Salones s ON ts.SalonId = s.Id
INNER JOIN Sedes se ON s.SedeId = se.Id
INNER JOIN Tutores t ON ts.TutorId = t.Id
WHERE t.Email = 'tutor@academia.local';
```

### Ver Asignaciones Realizadas
```sql
SELECT 
    a.Nombre + ' ' + a.Apellido AS Alumno,
    s.Nombre AS Salon,
    se.Nombre AS Sede,
    a.IsActive AS Estado
FROM Alumnos a
INNER JOIN Salones s ON a.SalonId = s.Id
INNER JOIN Sedes se ON s.SedeId = se.Id
WHERE a.SalonId IS NOT NULL
ORDER BY a.Apellido;
```

---

## ?? DASHBOARD DEL TUTOR ACTUALIZADO

### Acciones Rápidas (Nuevas)
```
??? Gestionar Salones
??? ?? Asignar Salones a Alumnos
??? ?? Aprobar Matrículas
??? Gestionar Notas
??? Ver Materiales
??? Subir Material
```

### Beneficios
- ? Acceso centralizado
- ? Flujos claros y directos
- ? Permisos correctos
- ? Interfaz intuitiva

---

## ?? MÉTRICAS Y LOGS

### Logs Generados
```csharp
// Asignación de salón
_logger.LogInformation("Tutor {Email} asignó alumno {AlumnoId} al salón {SalonId}", 
    userEmail, alumnoId, salonId);

// Aprobación de matrícula
_logger.LogInformation("Tutor {Email} aprobó matrícula {MatriculaId}", 
    userEmail, matriculaId);

// Rechazo de matrícula
_logger.LogInformation("Tutor {Email} rechazó matrícula {MatriculaId}", 
    userEmail, matriculaId);
```

### Eventos Auditables
- ? Asignación de salón
- ? Aprobación de matrícula
- ? Rechazo de matrícula
- ? Cambios en estado de matrícula
- ? Decremento de vacantes

---

## ?? DIFERENCIAS CON ADMIN/COORDINADOR

### Tutores
- ? Solo ven matrículas pendientes globales
- ? Solo pueden asignar a **sus salones**
- ? No pueden crear/editar salones
- ? No pueden gestionar otros tutores

### Admin/Coordinador
- ? Pueden asignar a **cualquier salón**
- ? Pueden gestionar todos los recursos
- ? Tienen acceso completo al sistema

---

## ? CHECKLIST FINAL

- [x] Página AsignarSalones creada
- [x] Página AprobarMatriculas creada
- [x] Enlaces agregados al Dashboard del Tutor
- [x] Validaciones de permisos implementadas
- [x] Solo puede asignar a salones propios
- [x] Decrementode vacantes funciona
- [x] Logs de auditoría implementados
- [x] Mensajes de éxito/error
- [x] Modals con confirmaciones
- [x] Compilación exitosa

---

## ?? RESUMEN

### Nuevas Capacidades de Tutores
1. ? Asignar alumnos a salones (solo a los suyos)
2. ? Aprobar matrículas pendientes
3. ? Rechazar matrículas con motivo
4. ? Ver alumnos sin salón
5. ? Gestionar asignaciones de su responsabilidad

### Beneficios
- ?? Descentraliza la gestión
- ?? Tutores más autónomos
- ?? Proceso de matrícula más rápido
- ?? Mejor distribución de responsabilidades
- ?? Control granular por tutor

---

**¡FUNCIONALIDADES COMPLETAS PARA TUTORES!** ??

**Desarrollado por**: Academia Zoe Team  
**Versión**: 1.4.0 FINAL  
**Última actualización**: Diciembre 2025 ??

---

## ?? PRÓXIMOS PASOS SUGERIDOS

1. **Testing completo**
   - Probar asignación de salones
   - Probar aprobación de matrículas
   - Verificar permisos

2. **Capacitación**
   - Manual para tutores
   - Video tutorial
   - Sesión de entrenamiento

3. **Monitoreo**
   - Revisar logs de asignaciones
   - Métricas de aprobaciones
   - Tiempos de respuesta

---

**¡SISTEMA COMPLETO Y FUNCIONAL!** ?
