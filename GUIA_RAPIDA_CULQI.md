# ?? Guía Rápida: Configurar Culqi en 5 Minutos

## Paso 1: Actualizar Base de Datos ?

Ejecuta el script SQL en tu base de datos:

```bash
sqlcmd -S localhost -d academic -E -i SQL_CULQI_INTEGRATION.sql
```

O desde SSMS:
- Abrir `SQL_CULQI_INTEGRATION.sql`
- Ejecutar (F5)

---

## Paso 2: Verificar Credenciales ??

Abre `appsettings.json` y verifica que estén las credenciales de Culqi:

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

---

## Paso 3: Ejecutar la Aplicación ??

```bash
cd AcademiaNet
dotnet run
```

La app estará en: `https://localhost:5001`

---

## Paso 4: Configurar Pasarela desde Admin ??

1. Login como **Admin**:
   - Email: `admin@academia.local`
   - Password: `Admin123!`

2. En el Dashboard, click en **"Configurar Pasarela de Pago"**

3. Selecciona una opción:
   - ? **Sin Pasarela** (matrícula manual)
   - ?? **MercadoPago** (pago automático)
   - ?? **Culqi** (pago automático - NUEVO!)

4. Click en **"Guardar Configuración"**

---

## Paso 5: Probar una Matrícula ??

### Con Culqi Activo:

1. Ir a: `https://localhost:5001/Public/Matriculate`

2. Completar datos del alumno:
   - Nombre, Apellido, Email
   - DNI: `12345678`
   - Fecha de nacimiento
   - Datos del apoderado (si es menor)

3. Click en **"Continuar al Pago"**

4. Se abrirá **Culqi Checkout**

5. Usar tarjeta de prueba:
   ```
   Número: 4111 1111 1111 1111
   CVV: 123
   Fecha: 09/25
   Email: test@test.com
   ```

6. Click en **"Pagar"**

7. ? **Pago exitoso!**
   - Se crea automáticamente la cuenta del alumno
   - Estado: `Pagado`
   - Acceso inmediato a materiales

---

## ?? ¡Listo!

Tu sistema ahora acepta pagos con Culqi.

---

## ?? Cambiar a MercadoPago

1. Admin Dashboard ? **Configurar Pasarela de Pago**
2. Seleccionar **MercadoPago**
3. Guardar

---

## ?? Cambiar a Sin Pasarela (Manual)

1. Admin Dashboard ? **Configurar Pasarela de Pago**
2. Seleccionar **Sin Pasarela**
3. Guardar

**Con esta opción:**
- Los alumnos se registran pero quedan con `EstadoPago = Pendiente`
- **No tienen acceso** a materiales hasta que apruebes manualmente
- Para aprobar: Dashboard ? Matrículas Pendientes ? Aprobar Pago

---

## ?? Ver Matrículas

```sql
-- Ver todas las matrículas
SELECT * FROM Matriculas;

-- Ver matrículas por Culqi
SELECT * FROM Matriculas WHERE TipoPasarela = 2;

-- Ver matrículas pendientes
SELECT * FROM Matriculas WHERE EstadoPago = 0;

-- Ver configuración actual
SELECT * FROM ConfiguracionPasarelas;
```

---

## ?? Troubleshooting

### Error: "Culqi Secret Key no configurado"
? Verificar que `Culqi.SecretKey` esté en `appsettings.json`

### Error: "No se puede conectar a la API de Culqi"
? Verificar conexión a internet  
? Verificar que la URL sea `https://api.culqi.com`

### Alumno no puede acceder a materiales
? Verificar que `EstadoPago = 1` (Pagado)  
? Verificar que tenga rol "Alumno"  
? Verificar que `IsActive = true`

---

## ?? Pasar a Producción

1. Obtener credenciales de producción desde https://panel.culqi.com

2. Actualizar `appsettings.json`:
   ```json
   {
     "Culqi": {
       "Enabled": true,
       "Environment": "production",
       "PublicKey": "pk_live_xxx",
       "SecretKey": "sk_live_xxx"
     }
   }
   ```

3. ¡Listo para recibir pagos reales! ??

---

## ?? Más Información

- README.md - Documentación completa
- CULQI_INTEGRATION_COMPLETE.md - Detalles técnicos
- https://docs.culqi.com - Documentación oficial de Culqi

---

**¡Disfruta de tu nueva integración de Culqi!** ??
