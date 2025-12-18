# ?? CÓMO OBTENER CREDENCIALES DE MERCADO PAGO

## ? Problema Actual:

```
Error: Status code: 403 | At least one policy returned UNAUTHORIZED
```

**Causa**: Las credenciales en `appsettings.json` no son válidas.

---

## ? SOLUCIÓN: Obtener Credenciales Reales

### **Opción 1: Credenciales de Prueba (Recomendado para Desarrollo)**

#### **Paso 1: Acceder al Panel de Mercado Pago**

1. Ve a: https://www.mercadopago.com.pe/developers/panel
2. Inicia sesión con tu cuenta de Mercado Pago
3. Si no tienes cuenta, créala en: https://www.mercadopago.com.pe/

#### **Paso 2: Crear una Aplicación**

1. En el panel, haz clic en **"Tus aplicaciones"**
2. Click en **"Crear aplicación"**
3. Selecciona:
   - **Nombre**: AcademiaZoe (o el que prefieras)
   - **Producto**: Pagos online
   - **Propósito**: Recibir pagos
4. Click **"Crear aplicación"**

#### **Paso 3: Obtener Credenciales de Prueba**

1. Dentro de tu aplicación, ve a **"Credenciales"**
2. Selecciona el tab **"Credenciales de prueba"**
3. Verás algo como:

```
Public Key (Prueba):
TEST-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

Access Token (Prueba):
TEST-1234567890123456-123456-xxxxxxxxxxxxxxxxxxxxxxxxxxxx-123456789
```

#### **Paso 4: Actualizar `appsettings.json`**

```json
{
  "MercadoPago": {
    "Environment": "sandbox",
    "AccessToken": "TEST-1234567890123456-123456-xxxxxxxxxxxxxxxxxxxxxxxxxxxx-123456789",
    "PublicKey": "TEST-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

?? **IMPORTANTE**: 
- Las credenciales de prueba **comienzan con `TEST-`**
- Si no comienzan con `TEST-`, estás usando credenciales de producción (no recomendado para desarrollo)

---

### **Opción 2: Credenciales de Producción (Para Ir en Vivo)**

?? **Solo usar cuando estés listo para recibir pagos reales**

1. En el panel, ve a **"Credenciales de producción"**
2. Copia:
   - **Public Key** (comienza con `APP_USR-...`)
   - **Access Token** (comienza con `APP_USR-...`)

3. Actualizar `appsettings.json`:

```json
{
  "MercadoPago": {
    "Environment": "production",
    "AccessToken": "APP_USR-1234567890123456-123456-xxxxxxxxxxxxxxxxxxxxxxxxxxxx-123456789",
    "PublicKey": "APP_USR-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
  }
}
```

---

## ?? Tarjetas de Prueba (Solo con Credenciales de Prueba)

### **Tarjeta Aprobada:**
```
Número: 5031 7557 3453 0604
CVV: 123
Vencimiento: 11/25
Nombre: APRO
DNI: 12345678
```

### **Tarjeta Rechazada (Fondos Insuficientes):**
```
Número: 5031 4332 1540 6351
CVV: 123
Vencimiento: 11/25
Nombre: OTHE
DNI: 12345678
```

### **Más Tarjetas de Prueba:**
https://www.mercadopago.com.pe/developers/es/docs/checkout-pro/additional-content/test-cards

---

## ?? Seguridad de Credenciales

### ?? **NUNCA** Hagas Esto:

1. ? No subas credenciales a GitHub público
2. ? No compartas credenciales en capturas de pantalla
3. ? No uses credenciales de producción en desarrollo

### ? **Buenas Prácticas:**

1. ? Usa **variables de entorno** para producción:

```csharp
// En Program.cs o Startup.cs
builder.Configuration["MercadoPago:AccessToken"] = Environment.GetEnvironmentVariable("MP_ACCESS_TOKEN");
```

2. ? Agrega `appsettings.json` a `.gitignore` (ya está)

3. ? Usa **Azure Key Vault** o **User Secrets** para almacenar credenciales:

```bash
dotnet user-secrets init
dotnet user-secrets set "MercadoPago:AccessToken" "TEST-your-token"
```

---

## ??? Verificación Rápida

### 1. **Verificar Formato de Credenciales**

```
? Credenciales de Prueba:
   Public Key:    TEST-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
   Access Token:  TEST-1234567890123456-123456-...

? Credenciales de Producción:
   Public Key:    APP_USR-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
   Access Token:  APP_USR-1234567890123456-123456-...

? Credenciales Inválidas:
   Public Key:    APP_USR-fd47cf15-378c-4b11-8294-898f95e3c736 (sin TEST- y corta)
   Access Token:  APP_USR-4603867478943523-100402-... (formato sospechoso)
```

### 2. **Probar Credenciales**

Después de actualizar las credenciales:

1. Reinicia la aplicación
2. Ve a `/Public/Matriculate`
3. Completa el formulario
4. Deberías ser redirigido a Mercado Pago

Si ves la página de pago de Mercado Pago, **¡tus credenciales funcionan!** ?

---

## ?? Errores Comunes

### Error 403 - Unauthorized
**Causa**: Credenciales inválidas o no pertenecen a tu cuenta

**Solución**: Obtén credenciales reales de tu cuenta de Mercado Pago

### Error 401 - Invalid credentials
**Causa**: Access Token incorrecto o expirado

**Solución**: Genera nuevas credenciales en el panel

### Error 400 - Bad request
**Causa**: Parámetros incorrectos en la solicitud

**Solución**: Verifica que el código esté usando correctamente las credenciales

---

## ?? Soporte de Mercado Pago

Si sigues teniendo problemas:

- **Documentación**: https://www.mercadopago.com.pe/developers/es/docs
- **Foros**: https://www.mercadopago.com.pe/developers/es/support
- **Chat de Soporte**: Disponible en el panel de desarrolladores

---

## ? Checklist

- [ ] Crear cuenta en Mercado Pago
- [ ] Acceder al panel de desarrolladores
- [ ] Crear aplicación
- [ ] Obtener credenciales de prueba (TEST-)
- [ ] Actualizar `appsettings.json`
- [ ] Reiniciar aplicación
- [ ] Probar flujo de matrícula
- [ ] Verificar redirección a MP
- [ ] Usar tarjeta de prueba
- [ ] Confirmar que funciona

---

## ?? Resultado Esperado

Después de configurar correctamente:

```
Usuario ? Formulario ? MP Service crea preferencia ?
? Redirige a Mercado Pago Sandbox ?
Usuario paga con tarjeta de prueba ?
MP redirige de vuelta ?
Usuario registrado exitosamente
```

---

**Estado Actual**: ? Credenciales Inválidas  
**Acción Requerida**: Obtener credenciales reales de Mercado Pago  
**Tiempo Estimado**: 5-10 minutos

¡Una vez que obtengas las credenciales reales, todo debería funcionar correctamente! ??
