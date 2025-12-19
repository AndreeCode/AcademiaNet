# ?? Integración de Culqi en AcademiaNet

## ? Implementación Completada

Se ha integrado exitosamente **Culqi** como pasarela de pago para matrículas en AcademiaNet.

---

## ?? Cambios Realizados

### 1. **Modelos Actualizados**

#### `Matricula.cs`
- ? Agregado enum `TipoPasarela` (SinPasarela, MercadoPago, Culqi)
- ? Agregada propiedad `TipoPasarela`
- ? Agregada propiedad `CulqiChargeId`
- ? Agregada propiedad `CulqiTokenId`
- ? Agregada propiedad `CulqiOrderId`

#### `ConfiguracionPasarela.cs` (Nuevo)
- ? Modelo para almacenar la configuración global de pasarela activa
- ? Permite cambiar entre las 3 modalidades

---

### 2. **Servicio de Culqi**

#### `CulqiService.cs` (Nuevo)
Servicio completo para interactuar con la API de Culqi:

**Métodos implementados:**
- `CreateChargeAsync()` - Crear cargo con tarjeta
- `GetChargeAsync()` - Obtener información de un cargo
- `CreateOrderAsync()` - Crear orden de pago (PagoEfectivo, Yape, etc.)

**Características:**
- ? Autenticación con Secret Key
- ? Manejo de errores robusto
- ? Logging detallado
- ? Conversión automática de montos (PEN a centavos)
- ? Soporte para metadata personalizada

---

### 3. **Configuración**

#### `appsettings.json`
```json
{
  "Culqi": {
    "Enabled": false,
    "Environment": "sandbox",
    "PublicKey": "pk_test_xZpBFhfnkH5w9WZL",
    "SecretKey": "sk_test_RptFw7eon6AhkW8L",
    "RsaId": "9944c2af-b394-4cf2-abaa-5b2ebdefaa3e",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n..."
  }
}
```

#### `Program.cs`
- ? Registrado `CulqiService` como scoped service
- ? Configurado `HttpClient` para Culqi
- ? Inyección de `CulqiOptions` desde configuración

---

### 4. **Base de Datos**

#### Tabla `Matriculas` (Columnas Agregadas)
```sql
ALTER TABLE [Matriculas] ADD [TipoPasarela] int NOT NULL DEFAULT(0);
ALTER TABLE [Matriculas] ADD [CulqiChargeId] nvarchar(200) NULL;
ALTER TABLE [Matriculas] ADD [CulqiTokenId] nvarchar(200) NULL;
ALTER TABLE [Matriculas] ADD [CulqiOrderId] nvarchar(200) NULL;
```

#### Tabla `ConfiguracionPasarelas` (Nueva)
```sql
CREATE TABLE [ConfiguracionPasarelas](
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PasarelaActiva] INT NOT NULL DEFAULT(0),
    [UltimaModificacion] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
    [ModificadoPor] NVARCHAR(256) NULL
);
```

#### `DbInitializer.cs`
- ? Método `EnsureConfiguracionPasarelaTableAsync()` - Crea tabla si no existe
- ? Actualizado `EnsureMatriculaColumnsAsync()` - Agrega columnas de Culqi
- ? Inserta configuración por defecto (SinPasarela)

---

### 5. **Panel de Administración**

#### Página: `/Admin/ConfigurarPasarela`

**Funcionalidades:**
- ? Visualización de configuración actual
- ? Cambio entre 3 modalidades:
  - Sin Pasarela (Matrícula Manual)
  - MercadoPago
  - Culqi
- ? Información detallada de cada modalidad
- ? Auditoría de cambios (quién y cuándo)
- ? Alertas y validaciones

**Archivos:**
- `ConfigurarPasarela.cshtml.cs`
- `ConfigurarPasarela.cshtml`

---

## ?? Funcionalidades del Sistema

### Modalidades de Pago

#### 1. **Sin Pasarela (Manual)**
- Alumno completa formulario
- Estado: `Pendiente`
- **Acceso limitado** hasta aprobación manual
- Admin/Tutor aprueba el pago manualmente

#### 2. **MercadoPago**
- Pago automático con redirección
- Acepta: Tarjetas, efectivo, etc.
- **Acceso inmediato** tras pago exitoso
- Registro automático del alumno

#### 3. **Culqi**
- Pago automático con Culqi Checkout
- Acepta: Tarjetas, Yape, PagoEfectivo, Cuotéalo
- **Acceso inmediato** tras pago exitoso
- Registro automático del alumno

---

### Control de Acceso

#### Alumnos con `EstadoPago = Pendiente`

**? Permitido:**
- Login al sistema
- Ver dashboard básico
- Editar perfil

**? Bloqueado:**
- Materiales de estudio
- Horarios de clase
- Archivos descargables
- Información detallada del ciclo

---

## ?? Uso del Sistema

### Como Administrador

1. **Configurar Pasarela:**
   - Login como Admin
   - Dashboard ? **Configurar Pasarela de Pago**
   - Seleccionar modalidad deseada
   - Guardar

2. **Aprobar Pagos Manuales (Si aplica):**
   - Dashboard ? Matrículas Pendientes
   - Aprobar pago del alumno
   - Estado cambia a `Pagado`

### Como Alumno

1. **Matrícula con Culqi:**
   - Ir a `/Public/Matriculate`
   - Completar datos personales
   - Si Culqi está activo:
     - Se abre Culqi Checkout
     - Seleccionar método de pago
     - Ingresar datos de tarjeta o código Yape
     - Confirmar pago
   - Acceso inmediato tras pago exitoso

2. **Matrícula Manual:**
   - Completar formulario
   - Esperar aprobación de Admin/Tutor
   - Acceso limitado hasta aprobación

---

## ?? Pruebas

### Tarjetas de Prueba (Sandbox Culqi)

| Tarjeta | Número | CVV | Fecha | Resultado |
|---------|--------|-----|-------|-----------|
| Visa | 4111 1111 1111 1111 | 123 | 09/25 | ? Aprobado |
| Mastercard | 5111 1111 1111 1118 | 123 | 09/25 | ? Aprobado |
| Rechazada | 4222 2222 2222 2220 | 123 | 09/25 | ? Rechazado |

### Variables de Entorno para Pruebas

```env
CULQI_ENABLED=true
CULQI_ENVIRONMENT=sandbox
CULQI_PUBLIC_KEY=pk_test_xZpBFhfnkH5w9WZL
CULQI_SECRET_KEY=sk_test_RptFw7eon6AhkW8L
```

---

## ?? Logs y Debugging

### Logs del CulqiService

El servicio genera logs detallados:

```
[INFO] Creando cargo Culqi. Monto: 100.00 PEN, Email: alumno@test.com
[INFO] ? Cargo Culqi creado exitosamente. ChargeId: chr_test_xxx
[ERROR] ? Error al crear cargo Culqi. Status: 400, Response: {...}
```

### Verificar en Base de Datos

```sql
-- Ver configuración actual
SELECT * FROM ConfiguracionPasarelas;

-- Ver matrículas con Culqi
SELECT * FROM Matriculas WHERE TipoPasarela = 2;

-- Ver matrículas pendientes
SELECT * FROM Matriculas WHERE EstadoPago = 0;
```

---

## ?? Despliegue a Producción

### 1. Obtener Credenciales de Producción

- Ingresar a https://panel.culqi.com
- Ir a **Producción ? API Keys**
- Copiar `pk_live_xxx` y `sk_live_xxx`

### 2. Actualizar `appsettings.json`

```json
{
  "Culqi": {
    "Enabled": true,
    "Environment": "production",
    "PublicKey": "pk_live_TU_PUBLIC_KEY",
    "SecretKey": "sk_live_TU_SECRET_KEY",
    "RsaId": "TU_RSA_ID_PROD",
    "RsaPublicKey": "TU_RSA_PUBLIC_KEY_PROD"
  }
}
```

### 3. Configurar Variables de Entorno (Recomendado)

```bash
CULQI_ENABLED=true
CULQI_ENVIRONMENT=production
CULQI_PUBLIC_KEY=pk_live_xxx
CULQI_SECRET_KEY=sk_live_xxx
```

---

## ?? Recursos

### Documentación Oficial
- [Culqi Docs](https://docs.culqi.com/)
- [API Reference](https://docs.culqi.com/#/api)
- [Tarjetas de Prueba](https://docs.culqi.com/#/desarrollo/tarjetas-de-prueba)
- [Custom Checkout](https://docs.culqi.com/#/pagos/culqi-checkout)

### Soporte Culqi
- Email: soporte@culqi.com
- Panel: https://panel.culqi.com

---

## ?? Resumen

? **Culqi completamente integrado**  
? **3 modalidades de pago disponibles**  
? **Control de acceso basado en estado de pago**  
? **Panel de Admin para configuración**  
? **Base de datos actualizada**  
? **Documentación completa**  

**¡El sistema está listo para aceptar pagos con Culqi!** ??
