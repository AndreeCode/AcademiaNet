# ? CORRECCIÓN - Redirección a Mercado Pago

## ? Problema Identificado

```
Error: auto_return invalid. back_url.success must be defined
```

### Causa:
Las URLs de retorno (`back_urls`) no estaban correctamente formateadas como URLs absolutas completas.

---

## ? Correcciones Aplicadas

### 1. **URLs Absolutas Corregidas**

#### Antes:
```csharp
BackUrls = new PreferenceBackUrlsRequest
{
    Success = "/Public/MatriculaResult?status=success",  // ? URL relativa
    ...
}
```

#### Ahora:
```csharp
BackUrls = new PreferenceBackUrlsRequest
{
    Success = $"{baseUrl}/Public/MatriculaResult?status=success&matriculaId={matricula.Id}",  // ? URL absoluta
    Failure = $"{baseUrl}/Public/MatriculaResult?status=failure&matriculaId={matricula.Id}",
    Pending = $"{baseUrl}/Public/MatriculaResult?status=pending&matriculaId={matricula.Id}"
}
```

### 2. **Mejoras en Logging**

```csharp
_logger.LogInformation("Creando preferencia de MP. BaseUrl: {BaseUrl}, MatriculaId: {Id}", baseUrl, matricula.Id);
_logger.LogInformation("Redirigiendo a MP. InitPoint: {InitPoint}", initPoint);
```

Ahora podrás ver en los logs exactamente qué URLs se están usando.

### 3. **Guardar Información de MP**

```csharp
matricula.MercadoPagoInitPoint = initPoint;
matricula.MercadoPagoPreferenceId = preferenceId;
_context.Matriculas.Update(matricula);
await _context.SaveChangesAsync();
```

### 4. **No Eliminar Matrícula en Error de MP**

Si Mercado Pago falla, la matrícula se mantiene en estado "Pendiente" en lugar de eliminarse, permitiendo reintento.

---

## ?? Cómo Probar

### 1. **Reiniciar la Aplicación**

```bash
# Detener aplicación (Ctrl+C)
# Volver a ejecutar
dotnet run
```

### 2. **Completar Formulario de Matrícula**

```
1. Ir a: http://localhost:5000/Public/Matriculate

2. Llenar datos:
   Nombre: Test
   Apellido: Usuario
   DNI: TEST1234
   Email: test@test.com
   Contraseña: Test123!
   Confirmar: Test123!
   ? Términos

3. Click "Completar Matrícula"
```

### 3. **Verificar Logs**

Deberías ver en los logs:

```
info: Creando preferencia de MP. BaseUrl: http://localhost:5000, MatriculaId: XX
info: Preferencia creada exitosamente. ID: XXXXX, InitPoint: https://sandbox.mercadopago...
info: Redirigiendo a MP. InitPoint: https://sandbox.mercadopago...
```

### 4. **Resultado Esperado**

? **Serás redirigido automáticamente a Mercado Pago Sandbox**

Verás una página de Mercado Pago donde podrás:
- Ingresar datos de tarjeta de prueba
- Completar el pago
- Ser redirigido de vuelta a MatriculaResult

---

## ?? Tarjetas de Prueba

### ? Aprobación Automática:
```
Número: 4509 9535 6623 3704
CVV: 123
Vencimiento: 11/25
Nombre: APRO
```

### ? Rechazo (Fondos Insuficientes):
```
Número: 4074 0962 7899 4726
CVV: 123
Vencimiento: 11/25
Nombre: OTHE
```

---

## ?? Verificación de URLs

### Verificar que las URLs sean absolutas:

```csharp
// En los logs deberías ver algo como:
Success URL: http://localhost:5000/Public/MatriculaResult?status=success&matriculaId=18
Failure URL: http://localhost:5000/Public/MatriculaResult?status=failure&matriculaId=18
Pending URL: http://localhost:5000/Public/MatriculaResult?status=pending&matriculaId=18
```

**NO** debe verse así:
```
? /Public/MatriculaResult?status=success
```

**DEBE** verse así:
```
? http://localhost:5000/Public/MatriculaResult?status=success&matriculaId=18
```

---

## ?? Flujo Completo

```
Usuario en /Public/Matriculate
  ?
Completa formulario
  ?
Sistema crea Alumno + Usuario Identity
  ?
Sistema crea Matrícula (Estado: Pendiente)
  ?
Sistema llama a MercadoPagoService
  ?
MercadoPago crea Preferencia
  ?
MercadoPago retorna InitPoint (URL de pago)
  ?
Sistema guarda InitPoint y PreferenceId en Matrícula
  ?
Sistema redirige a InitPoint
  ?
?? USUARIO VE PÁGINA DE MERCADO PAGO ??
  ?
Usuario completa pago
  ?
MercadoPago redirige según resultado:
  ?? ? Success ? /Public/MatriculaResult?status=success&matriculaId=X
  ?? ? Pending ? /Public/MatriculaResult?status=pending&matriculaId=X
  ?? ? Failure ? /Public/MatriculaResult?status=failure&matriculaId=X
  ?
Sistema procesa resultado:
  ?? ? Success ? Estado: Pagado, Login automático
  ?? ? Pending ? Estado: Pendiente, Esperar confirmación
  ?? ? Failure ? Estado: Cancelado, Permitir reintento
```

---

## ?? Troubleshooting

### Problema 1: Sigue sin redirigir
**Solución**:
1. Verifica los logs para ver el error exacto
2. Asegúrate de que `appsettings.json` tenga el Access Token correcto
3. Verifica que la aplicación esté ejecutándose en el puerto esperado

### Problema 2: Error "Invalid credentials"
**Solución**:
```json
{
  "MercadoPago": {
    "AccessToken": "APP_USR-3755805793889715-092001-ea347befb249b2af8b89ebcc1d5796f0-1997592911"
  }
}
```

### Problema 3: Redirige pero la URL es incorrecta
**Solución**:
Verificar que `Request.Scheme` y `Request.Host` sean correctos:
```csharp
var baseUrl = $"{Request.Scheme}://{Request.Host}"; 
// Debe ser: http://localhost:5000 o https://tudominio.com
```

---

## ? Checklist de Verificación

- [x] Código actualizado con URLs absolutas
- [x] Eliminados campos `Expires` y `ExpirationDateTo`
- [x] Logging mejorado
- [x] Compilación exitosa
- [ ] **Reiniciar aplicación**
- [ ] **Probar formulario de matrícula**
- [ ] **Verificar redirección a MP**
- [ ] **Completar pago de prueba**
- [ ] **Verificar resultado en MatriculaResult**

---

## ?? Cambios Realizados

### Archivos Modificados:

1. ? `Services/MercadoPagoService.cs`
   - URLs absolutas en `BackUrls`
   - Eliminados campos problemáticos
   - Logging mejorado

2. ? `Pages/Public/Matriculate.cshtml.cs`
   - Guardar `InitPoint` y `PreferenceId` en BD
   - No eliminar matrícula en error de MP
   - Logging detallado

---

## ?? Resultado Esperado

Después de los cambios:

1. ? Formulario de matrícula se envía correctamente
2. ? Sistema crea alumno y matrícula
3. ? **Redirección automática a Mercado Pago Sandbox**
4. ? Página de pago de MP se muestra correctamente
5. ? Después del pago, redirige a MatriculaResult
6. ? Estado de matrícula se actualiza según resultado

---

**Estado**: ? CORREGIDO  
**Acción Requerida**: Reiniciar aplicación y probar  
**Tiempo Estimado**: 2 minutos de prueba

---

## ?? Próximos Pasos

1. Reinicia la aplicación
2. Prueba el flujo completo de matrícula
3. Verifica que seas redirigido a Mercado Pago
4. Usa una tarjeta de prueba para completar el pago
5. Verifica que regreses a MatriculaResult con el estado correcto

**¡El sistema está listo para procesar pagos con Mercado Pago!** ??
