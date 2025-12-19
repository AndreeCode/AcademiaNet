# ? CULQI CONFIGURADO POR DEFECTO Y CONTROL DE PASARELAS

## ?? CAMBIOS IMPLEMENTADOS

### 1. Culqi como Pasarela por Defecto ?

**Antes:** Sin Pasarela (matrícula manual)  
**Ahora:** Culqi (pago automático)

#### Cambios en `DbInitializer.cs`:
- ? Al crear la tabla `ConfiguracionPasarelas`, se inserta con `PasarelaActiva = 2` (Culqi)
- ? Si ya existe la configuración, se actualiza automáticamente a Culqi
- ? Registro de auditoría: "SYSTEM - Default Culqi"

---

### 2. Página de Matrícula Actualizada ??

**Archivo:** `AcademiaNet/Pages/Public/Matriculate.cshtml.cs`

#### Nuevas Funcionalidades:
- ? Lee la configuración de pasarela activa desde la BD
- ? Procesa matrícula según la pasarela configurada:
  - **Culqi:** Muestra formulario con Culqi Checkout
  - **MercadoPago:** Redirige a MercadoPago
  - **Sin Pasarela:** Registro manual (pendiente de aprobación)

#### Métodos Agregados:
```csharp
- ProcessCulqiMatriculaAsync()
- ProcessMercadoPagoMatriculaAsync()
- ProcessManualMatriculaAsync()
```

---

### 3. Callback de Culqi Implementado ??

**Archivos:**
- `AcademiaNet/Pages/Public/CulqiCallback.cshtml.cs`
- `AcademiaNet/Pages/Public/CulqiCallback.cshtml`

#### Flujo de Pago con Culqi:
1. Usuario completa formulario de matrícula
2. Datos se guardan en TempData
3. Se muestra Culqi Checkout en el frontend
4. Usuario paga con tarjeta/Yape/PagoEfectivo
5. Culqi genera token
6. Token enviado a `/Public/CulqiCallback` (POST)
7. Backend crea cargo en Culqi
8. Si pago exitoso:
   - Crear alumno
   - Crear matrícula con `EstadoPago = Pagado`
   - Crear usuario Identity
   - Asignar rol "Alumno"
9. Redirigir a login

#### Validaciones:
- ? Solo se registra si Culqi confirma el pago
- ? No hay acceso hasta que el pago sea confirmado
- ? Rollback automático si falla la creación de usuario

---

### 4. Panel de Admin - Cambio de Pasarela ??

**Página Existente:** `/Admin/ConfigurarPasarela`

El Admin puede cambiar entre:
- ? Sin Pasarela (manual)
- ?? MercadoPago
- ?? **Culqi** (por defecto)

**Cambios se aplican inmediatamente** a nuevas matrículas.

---

## ?? TABLA: ConfiguracionPasarelas

```sql
CREATE TABLE [ConfiguracionPasarelas](
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [PasarelaActiva] INT NOT NULL DEFAULT(2),  -- Culqi por defecto
    [UltimaModificacion] DATETIME2 NOT NULL,
    [ModificadoPor] NVARCHAR(256) NULL
);
```

### Valores de PasarelaActiva:
- `0` = Sin Pasarela
- `1` = MercadoPago
- `2` = **Culqi** (por defecto)

---

## ?? CÓMO USAR

### Paso 1: Actualizar BD
```sh
sqlcmd -S localhost -d academic -E -i SQL_CONFIG_CULQI_DEFAULT.sql
```

O desde la aplicación, al ejecutar `dotnet run` se actualizará automáticamente.

### Paso 2: Verificar Configuración
```sql
SELECT * FROM ConfiguracionPasarelas;
-- Debe mostrar: PasarelaActiva = 2 (Culqi)
```

### Paso 3: Probar Matrícula
1. Ir a: `https://localhost:5001/Public/Matriculate`
2. Completar formulario
3. Se mostrará **Culqi Checkout**
4. Pagar con tarjeta de prueba:
   ```
   Número: 4111 1111 1111 1111
   CVV: 123
   Fecha: 09/25
   ```
5. Después del pago exitoso:
   - Alumno creado
   - Matrícula con `EstadoPago = Pagado`
   - Usuario creado con rol "Alumno"
   - Acceso inmediato al sistema

---

## ?? CONTROL DE ACCESO

### Alumnos con Estado Pendiente
Si la pasarela está en "Sin Pasarela" o si el pago falla:

**? Permitido:**
- Login
- Dashboard básico
- Ver perfil

**? Bloqueado:**
- Materiales
- Horarios
- Notas
- Descarga de archivos

### Cambio Automático de Estado
Solo cuando:
- **Culqi/MercadoPago:** Confirman el pago
- **Sin Pasarela:** Admin/Tutor/Coordinador aprueban manualmente

---

## ?? FLUJOS POR PASARELA

### 1?? Culqi (Defecto)
```
Usuario ? Formulario ? TempData guardado ? Culqi Checkout
    ?
Pago con tarjeta/Yape ? Token generado ? POST /CulqiCallback
    ?
Crear cargo en Culqi ? Si exitoso ? Crear alumno/matrícula/usuario
    ?
EstadoPago = Pagado ? Login ? Dashboard Alumno
```

### 2?? MercadoPago
```
Usuario ? Formulario ? TempData guardado ? Redirect a MercadoPago
    ?
Pago en MP ? Redirect a /MatriculaResult?status=success
    ?
Crear alumno/matrícula/usuario ? EstadoPago = Pagado
    ?
Login ? Dashboard Alumno
```

### 3?? Sin Pasarela
```
Usuario ? Formulario ? Crear alumno/matrícula/usuario
    ?
EstadoPago = Pendiente ? Login ? Dashboard (acceso limitado)
    ?
Admin/Tutor aprueba ? EstadoPago = Pagado ? Acceso completo
```

---

## ?? PRUEBAS

### Tarjetas de Culqi (Sandbox)
| Tarjeta | Número | CVV | Fecha | Resultado |
|---------|--------|-----|-------|-----------|
| Visa | 4111 1111 1111 1111 | 123 | 09/25 | ? Aprobado |
| Mastercard | 5111 1111 1111 1118 | 123 | 09/25 | ? Aprobado |
| Rechazada | 4222 2222 2222 2220 | 123 | 09/25 | ? Rechazado |

### Verificar Estado en BD
```sql
-- Ver matrículas
SELECT 
    a.Nombre + ' ' + a.Apellido AS Alumno,
    m.Monto,
    CASE m.TipoPasarela
        WHEN 0 THEN 'Sin Pasarela'
        WHEN 1 THEN 'MercadoPago'
        WHEN 2 THEN 'Culqi'
    END AS Pasarela,
    CASE m.EstadoPago
        WHEN 0 THEN 'Pendiente'
        WHEN 1 THEN 'Pagado'
        WHEN 2 THEN 'Cancelado'
        WHEN 3 THEN 'Rechazado'
    END AS Estado,
    m.FechaPago,
    m.CulqiChargeId
FROM Matriculas m
INNER JOIN Alumnos a ON m.AlumnoId = a.Id
ORDER BY m.CreatedAt DESC;
```

---

## ?? CONFIGURACIÓN

### appsettings.json
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

### Cambiar Pasarela Manualmente
```sql
UPDATE ConfiguracionPasarelas
SET 
    PasarelaActiva = 2,  -- 0=SinPasarela, 1=MercadoPago, 2=Culqi
    UltimaModificacion = GETUTCDATE(),
    ModificadoPor = 'Admin'
WHERE Id = 1;
```

O desde el panel:
1. Login como Admin
2. Dashboard ? **Configurar Pasarela de Pago**
3. Seleccionar opción deseada
4. Guardar

---

## ?? ARCHIVOS MODIFICADOS

1. ? `AcademiaNet/Data/DbInitializer.cs` - Culqi por defecto
2. ? `AcademiaNet/Pages/Public/Matriculate.cshtml.cs` - Multi-pasarela
3. ? `AcademiaNet/Pages/Public/CulqiCallback.cshtml.cs` - Nuevo callback
4. ? `AcademiaNet/Pages/Public/CulqiCallback.cshtml` - Vista callback
5. ? `SQL_CONFIG_CULQI_DEFAULT.sql` - Script de actualización

---

## ? CHECKLIST

- [x] Culqi configurado por defecto
- [x] Matrícula lee configuración activa
- [x] Procesa según pasarela configurada
- [x] Callback de Culqi implementado
- [x] Solo registra si pago confirmado
- [x] Control de acceso por estado de pago
- [x] Admin puede cambiar pasarela
- [x] Script SQL de configuración
- [x] Documentación completa
- [x] Proyecto compila sin errores

---

## ?? ESTADO FINAL

### ? COMPLETADO

**Configuración:**
- ? Culqi activado por defecto
- ? Admin puede cambiar entre 3 modalidades
- ? Solo se matricula después de pago confirmado

**Flujo:**
1. Usuario ingresa datos
2. Sistema detecta Culqi como pasarela activa
3. Muestra Culqi Checkout
4. Usuario paga
5. Culqi confirma pago
6. Sistema crea alumno/matrícula/usuario
7. Acceso inmediato al sistema

**Seguridad:**
- ? No hay registro sin pago (en Culqi/MercadoPago)
- ? Validación de email y DNI únicos
- ? Rollback automático si falla
- ? Auditoría completa

---

**Fecha:** 20 de Enero de 2025  
**Versión:** 2.0.0  
**Estado:** ? LISTO PARA PRODUCCIÓN
