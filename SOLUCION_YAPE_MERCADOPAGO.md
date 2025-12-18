# ?? SOLUCIÓN: Problema con Yape en Mercado Pago

## ? Problema Actual

```
Error: "Yape rechazó tu pago"
```

### Causa:
1. **Yape NO está disponible en ambiente de pruebas (sandbox)**
2. En sandbox solo se pueden usar **tarjetas de crédito/débito de prueba**
3. Tus credenciales parecen ser de producción pero están configuradas como sandbox

---

## ? Solución Inmediata: Usar Tarjetas de Prueba

### ?? **Tarjetas de Prueba para Perú:**

#### **Tarjeta Aprobada - Visa:**
```
Número:     4009 1753 3280 7657
CVV:        123
Vence:      11/25
Titular:    APRO
DNI:        12345678
Email:      test_user_12345678@testuser.com
```

#### **Tarjeta Aprobada - Mastercard:**
```
Número:     5031 7557 3453 0604
CVV:        123
Vence:      11/25
Titular:    APRO
DNI:        12345678
Email:      test_user_12345678@testuser.com
```

#### **Tarjeta Rechazada (Fondos Insuficientes):**
```
Número:     5031 4332 1540 6351
CVV:        123
Vence:      11/25
Titular:    OTHE
DNI:        12345678
```

#### **Más Tarjetas de Prueba:**
- https://www.mercadopago.com.pe/developers/es/docs/checkout-pro/additional-content/test-cards

---

## ?? **Verificar Configuración**

### Tu configuración actual en `appsettings.json`:

```json
{
  "MercadoPago": {
    "Environment": "sandbox",
    "AccessToken": "APP_USR-4603867478943523-100402-...",
    "PublicKey": "APP_USR-ea4722ed-f4af-440d-81f8-..."
  }
}
```

### ?? **Problema Detectado:**

Las credenciales que tienes:
- Comienzan con `APP_USR-` (formato de **producción**)
- Están configuradas como `"Environment": "sandbox"`

**Esto es INCONSISTENTE y puede causar problemas.**

---

## ? **Opciones para Solucionar**

### **Opción 1: Usar Credenciales de Prueba Correctas (Recomendado)**

1. **Ve al Panel de Mercado Pago**:
   - https://www.mercadopago.com.pe/developers/panel

2. **Selecciona "Credenciales de prueba"**

3. **Las credenciales de prueba deben comenzar con `TEST-`**:
   ```
   Public Key:   TEST-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
   Access Token: TEST-1234567890123456-123456-...
   ```

4. **Actualiza `appsettings.json`**:
   ```json
   {
     "MercadoPago": {
       "Environment": "sandbox",
       "AccessToken": "TEST-tu-access-token-de-prueba",
       "PublicKey": "TEST-tu-public-key-de-prueba"
     }
   }
   ```

5. **Reinicia la aplicación**

6. **Usa tarjetas de prueba** (NO Yape)

---

### **Opción 2: Usar Credenciales de Producción (Solo si estás listo para PAGOS REALES)**

?? **SOLO si quieres recibir pagos reales con dinero real**

1. **Verifica que las credenciales sean correctas**:
   - Accede a https://www.mercadopago.com.pe/developers/panel
   - Ve a "Credenciales de producción"
   - Verifica que coincidan con las de tu `appsettings.json`

2. **Cambia el environment a producción**:
   ```json
   {
     "MercadoPago": {
       "Environment": "production",
       "AccessToken": "APP_USR-4603867478943523-100402-...",
       "PublicKey": "APP_USR-ea4722ed-f4af-440d-81f8-..."
     }
   }
   ```

3. **Reinicia la aplicación**

4. **Ahora SÍ podrás usar Yape con dinero real**

---

## ?? **Entendiendo Sandbox vs Producción**

| Aspecto | Sandbox (Pruebas) | Producción (Real) |
|---------|-------------------|-------------------|
| **Credenciales** | Comienzan con `TEST-` | Comienzan con `APP_USR-` |
| **Pagos** | ? NO son reales | ? Pagos reales |
| **Yape** | ? NO disponible | ? Disponible |
| **Tarjetas** | ? Solo de prueba | ? Reales |
| **Dinero** | $0 (simulado) | ?? Real |

---

## ?? **Cómo Probar en Sandbox**

### **Paso 1: Verificar Credenciales de Prueba**

Asegúrate de tener credenciales que comiencen con `TEST-`:

```json
{
  "MercadoPago": {
    "Environment": "sandbox",
    "AccessToken": "TEST-...",
    "PublicKey": "TEST-..."
  }
}
```

### **Paso 2: Matricularse con Tarjeta de Prueba**

1. Ve a: `http://localhost:5042/Public/Matriculate`

2. Completa el formulario:
   ```
   Nombre: Test
   Apellido: Alumno
   DNI: TEST12345
   Email: test@test.com
   Contraseña: Test123!
   ```

3. **Serás redirigido a Mercado Pago**

4. **En la página de pago:**
   - ? NO selecciones Yape (no está disponible en pruebas)
   - ? Selecciona "Tarjeta de crédito o débito"
   - Ingresa los datos de la tarjeta de prueba:
     ```
     4009 1753 3280 7657
     CVV: 123
     Vence: 11/25
     Nombre: APRO
     ```

5. **Completa el pago**

6. **Serás redirigido de vuelta** a MatriculaResult con éxito

---

## ?? **Para Usar Yape (Solo en Producción)**

Si realmente quieres probar con Yape:

1. **Configura credenciales de producción** (`APP_USR-...`)
2. **Cambia environment a `"production"`**
3. **Ten en cuenta que los pagos serán REALES**
4. **Necesitarás una cuenta de Yape con saldo real**

?? **NO recomendado para desarrollo/pruebas**

---

## ?? **Configuración Recomendada para Desarrollo**

### **`appsettings.json` (Desarrollo):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=academic;..."
  },
  "MercadoPago": {
    "Environment": "sandbox",
    "AccessToken": "TEST-tu-access-token-de-prueba-aqui",
    "PublicKey": "TEST-tu-public-key-de-prueba-aqui"
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

### **`appsettings.Production.json` (Para cuando vayas a producción):**

```json
{
  "MercadoPago": {
    "Environment": "production",
    "AccessToken": "APP_USR-...-produccion",
    "PublicKey": "APP_USR-...-produccion"
  }
}
```

---

## ?? **Debugging**

### **Verificar qué environment estás usando:**

En los logs deberías ver:

```
info: Preparando preferencia MP. BaseUrl: http://localhost:5042, Monto: 1.00
info: URLs configuradas - Success: http://localhost:5042/Public/MatriculaResult?status=success
```

Si ves errores como:
```
fail: Error al crear preferencia. MatriculaId: 0, BaseUrl: http://localhost:5042
      MercadoPago.Error.MercadoPagoApiException: Error response from API. | Status code: 403
```

**Significa que las credenciales NO son válidas para ese environment.**

---

## ?? **Checklist de Solución**

- [ ] Obtener credenciales de prueba correctas (`TEST-...`)
- [ ] Actualizar `appsettings.json` con credenciales `TEST-`
- [ ] Configurar `"Environment": "sandbox"`
- [ ] Reiniciar la aplicación
- [ ] Ir a `/Public/Matriculate`
- [ ] Completar formulario
- [ ] **Seleccionar "Tarjeta" en Mercado Pago** (NO Yape)
- [ ] Usar tarjeta de prueba `4009 1753 3280 7657`
- [ ] Completar pago
- [ ] Verificar que redirige correctamente
- [ ] ? Matrícula exitosa

---

## ?? **Resumen**

### **Para Pruebas (Desarrollo):**
```
? Usar credenciales TEST-
? Environment: "sandbox"
? Usar TARJETAS de prueba
? NO usar Yape (no disponible en sandbox)
```

### **Para Producción (Pagos Reales):**
```
? Usar credenciales APP_USR-
? Environment: "production"
? Aceptar tarjetas reales
? Aceptar Yape (disponible)
?? Pagos reales con dinero real
```

---

## ?? **Recursos Adicionales**

- **Documentación MP**: https://www.mercadopago.com.pe/developers/es/docs
- **Tarjetas de Prueba**: https://www.mercadopago.com.pe/developers/es/docs/checkout-pro/additional-content/test-cards
- **Panel de Desarrolladores**: https://www.mercadopago.com.pe/developers/panel

---

**Estado Actual**: ? Configuración inconsistente  
**Acción Requerida**: Obtener credenciales TEST- correctas  
**Tiempo Estimado**: 5 minutos  
**Resultado Esperado**: Poder probar con tarjetas de prueba ?
