# Sistema de Administración - Academia Zoe

## ? Funcionalidades de Admin/Coordinador Implementadas

### ?? Panel de Administración Completo

El sistema ahora cuenta con un panel de administración completo que permite a los Admin y Coordinadores gestionar todo el ciclo académico.

---

## ?? **1. Crear Ciclos Académicos** (`/Admin/CreateCycle`)

### Características:
- ? Definir nombre del ciclo (ej: "Ciclo 2025-III")
- ? Configurar fechas de inicio y fin del ciclo
- ? **Establecer periodo de matrícula** (fecha inicio y fin)
- ? Definir número de vacantes (0 = ilimitadas)
- ? Seleccionar modalidad (Presencial, Virtual, Híbrido)
- ? **Generación automática de semanas** basada en la duración del ciclo

### Validaciones:
- Fecha fin debe ser posterior a fecha inicio
- Fecha fin de matrícula debe ser posterior a inicio de matrícula
- Matrícula debe iniciar ANTES del inicio del ciclo
- Número de vacantes entre 0 y 10,000

### Flujo:
```
Admin ? /Admin/CreateCycle ? Completa formulario ? Crear Ciclo
  ?
Sistema crea ciclo + genera semanas automáticamente
  ?
Redirección a Dashboard con mensaje de éxito
```

---

## ?? **2. Crear Tutores** (`/Admin/CreateTutor`)

### Características:
- ? Crear cuenta de Identity automáticamente
- ? Asignar rol "Tutor" automáticamente
- ? Crear entrada en tabla Tutores
- ? Configurar estado activo/inactivo
- ? Validación de duplicados (email único)

### Datos Requeridos:
- Nombre
- Apellido
- Email (será el usuario)
- Contraseña (mínimo 6 caracteres)
- Confirmar contraseña
- Estado (Activo/Inactivo)

### Flujo:
```
Admin ? /Admin/CreateTutor ? Completa formulario ? Crear Tutor
  ?
Sistema crea:
  1. Usuario Identity
  2. Asigna rol "Tutor"
  3. Crea registro en tabla Tutores
  ?
Redirección a Dashboard con credenciales generadas
```

---

## ?? **3. Dashboard Mejorado** (`/Admin/Dashboard`)

### Estadísticas en Tiempo Real:
- ?? **Total de Alumnos**: Cuenta total de estudiantes registrados
- ?? **Total de Matrículas**: Matrículas del ciclo actual
- ? **Matrículas Pendientes**: Matrículas sin pagar
- ?? **Vacantes Disponibles**: Vacantes restantes del ciclo actual

### Información del Ciclo Actual:
- Nombre del ciclo
- Modalidad (Presencial/Virtual/Híbrido)
- Fechas de inicio y fin
- **Estado de matrícula**: ABIERTA/CERRADA (badge dinámico)
- Fechas del periodo de matrícula
- Indicador visual si no está configurada

### Listas Rápidas:
- **Últimos 5 ciclos** con estado activo/inactivo
- **Últimos 5 tutores** con estado activo/inactivo

### Acciones Rápidas:
- Gestionar Materiales
- Gestionar Semanas
- Ver Página de Matrícula

---

## ?? **4. Editar Ciclos** (`/Admin/EditCycle`)

### Características:
- ? Editar todos los datos del ciclo
- ? **Modificar fechas de matrícula** en cualquier momento
- ? Cambiar modalidad y vacantes
- ? Validaciones completas
- ?? Advertencia sobre impacto de cambios

### Acceso:
- Desde el dashboard (próximamente link directo)
- URL directa: `/Admin/EditCycle?id={cicloId}`
- Sin ID edita el ciclo más reciente

---

## ?? **Roles y Permisos**

| Funcionalidad | Admin | Coordinador | Tutor | Alumno |
|---------------|-------|-------------|-------|--------|
| Crear Ciclo | ? | ? | ? | ? |
| Editar Ciclo | ? | ? | ? | ? |
| Crear Tutor | ? | ? | ? | ? |
| Ver Dashboard Admin | ? | ? | ? | ? |
| Gestionar Semanas | ? | ? | ? | ? |
| Subir Materiales | ? | ? | ? | ? |

---

## ?? **Cómo Probar**

### 1. Crear un Nuevo Ciclo:
```
1. Login como Admin: admin@academia.local / Admin123!
2. Ir a Dashboard (/Admin/Dashboard)
3. Click en "Crear Ciclo"
4. Completar formulario:
   - Nombre: Ciclo 2025-III
   - Fecha Inicio Ciclo: [Seleccionar fecha futura]
   - Fecha Fin Ciclo: [+4 meses]
   - Inicio Matrícula: [HOY]
   - Fin Matrícula: [+30 días]
   - Vacantes: 150
   - Modalidad: Híbrido
5. Click "Crear Ciclo"
6. Verificar mensaje de éxito
7. Ver semanas creadas automáticamente en /Semanas/Gestionar
```

### 2. Crear un Nuevo Tutor:
```
1. Login como Admin: admin@academia.local / Admin123!
2. Ir a Dashboard
3. Click en "Crear Tutor"
4. Completar formulario:
   - Nombre: Pedro
   - Apellido: González
   - Email: pedro.gonzalez@academia.local
   - Contraseña: Tutor123!
   - Confirmar Contraseña: Tutor123!
   - ? Activo
5. Click "Crear Tutor"
6. Copiar credenciales del mensaje de éxito
7. Cerrar sesión e intentar login con nuevo tutor
```

### 3. Editar Fechas de Matrícula:
```
1. Login como Admin o Coordinador
2. Ir a /Admin/EditCycle (edita último ciclo)
   O /Admin/EditCycle?id=1 (edita ciclo específico)
3. Modificar fechas de matrícula:
   - Inicio Matrícula: [Nueva fecha]
   - Fin Matrícula: [Nueva fecha]
4. Guardar cambios
5. Verificar en Dashboard que el estado se actualizó
6. Ir a /Public/Matriculate y verificar que funcione
```

---

## ?? **Datos de Prueba**

### Usuarios del Sistema:
```
Admin:
Email: admin@academia.local
Contraseña: Admin123!

Coordinador:
Email: coordinador@academia.local
Contraseña: Coord123!

Tutor:
Email: tutor@academia.local
Contraseña: Tutor123!

Alumno:
Email: alumno@academia.local
Contraseña: Alumno123!
```

---

## ?? **Mejoras de UI/UX**

### Dashboard:
- ? Cards estadísticas con iconos Bootstrap
- ? Badges dinámicos para estados
- ? Colores consistentes (#800020 brand color)
- ? Iconos descriptivos en toda la interfaz
- ? Diseño responsive
- ? Acciones rápidas destacadas

### Formularios:
- ? Validación en tiempo real
- ? Mensajes de error claros
- ? Hints informativos
- ? Alertas de advertencia
- ? Confirmación de acciones

---

## ?? **Flujo Completo del Sistema**

```
ADMIN CREA CICLO
  ?
Define fechas de matrícula (HOY ? +30 días)
  ?
Sistema genera semanas automáticamente
  ?
MATRÍCULA ABIERTA (verificar en Dashboard)
  ?
Estudiantes se matriculan en /Public/Matriculate
  ?
Admin ve estadísticas en tiempo real
  ?
Admin/Coordinador pueden editar fechas si es necesario
  ?
Tutores suben materiales por semana
  ?
Estudiantes acceden a materiales en su dashboard
```

---

## ?? **Características Automáticas**

### Al Crear un Ciclo:
1. ? Se validan todas las fechas
2. ? Se calculan las semanas basado en duración
3. ? Se crean semanas automáticamente (máx 20)
4. ? Cada semana tiene:
   - Número de semana
   - Fecha inicio/fin (7 días cada una)
   - Tema por defecto
   - Estado activo

### Al Crear un Tutor:
1. ? Se crea cuenta Identity
2. ? Se asigna rol "Tutor"
3. ? Se crea entrada en tabla Tutores
4. ? Se valida email único
5. ? Se muestra contraseña generada para compartir

---

## ?? **Validaciones Implementadas**

### Ciclos:
- ? Nombre requerido (máx 100 chars)
- ? Fechas válidas (fin > inicio)
- ? Matrícula antes del inicio del ciclo
- ? Vacantes entre 0-10000
- ? Modalidad requerida

### Tutores:
- ? Email único en sistema
- ? Email válido (formato)
- ? Contraseña mínimo 6 caracteres
- ? Confirmación de contraseña
- ? Nombre y apellido requeridos

---

## ?? **Próximas Mejoras Sugeridas**

1. **Ciclos**:
   - Lista completa de ciclos con filtros
   - Eliminar/Archivar ciclos
   - Duplicar ciclo para nuevo periodo

2. **Tutores**:
   - Lista completa de tutores
   - Editar tutores
   - Desactivar/Activar tutores
   - Asignar salones desde admin

3. **Reportes**:
   - Reporte de matrículas por ciclo
   - Reporte de pagos pendientes
   - Exportar a Excel/PDF

4. **Notificaciones**:
   - Email de bienvenida a tutores
   - Email cuando se crea ciclo
   - Recordatorios de matrícula

---

## ? **Resumen de Archivos Creados/Modificados**

### Nuevos Archivos:
1. `Pages/Admin/CreateCycle.cshtml.cs` - Lógica crear ciclo
2. `Pages/Admin/CreateCycle.cshtml` - Vista crear ciclo
3. `Pages/Admin/CreateTutor.cshtml.cs` - Lógica crear tutor
4. `Pages/Admin/CreateTutor.cshtml` - Vista crear tutor
5. `Pages/Admin/EditCycle.cshtml.cs` - Lógica editar ciclo
6. `Pages/Admin/EditCycle.cshtml` - Vista editar ciclo

### Modificados:
1. `Pages/Admin/Dashboard.cshtml.cs` - Más estadísticas
2. `Pages/Admin/Dashboard.cshtml` - Diseño completo nuevo

---

**¡El sistema de administración está completo y funcional!** ??

Los Admin y Coordinadores ahora pueden:
- ? Crear ciclos con fechas de matrícula personalizadas
- ? Crear tutores con cuentas completas
- ? Ver estadísticas en tiempo real
- ? Editar fechas de matrícula cuando sea necesario
- ? Gestionar todo el sistema académico

**Última actualización**: Enero 2025
