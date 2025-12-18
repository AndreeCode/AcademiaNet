# ?? SOLUCIÓN: Error de Columnas Faltantes en Matriculas

## ? **Problema Identificado**

```
Error: El nombre de columna 'MercadoPagoPaymentId' no es válido.
```

### Causa:
La tabla `Matriculas` en la base de datos no tiene las columnas necesarias para el tracking de Mercado Pago.

---

## ? **Soluciones Disponibles**

### **Opción 1: Ejecutar SQL Script (Recomendado - Más Rápido)**

#### Pasos:
1. Abre **SQL Server Management Studio** o **Azure Data Studio**
2. Conéctate a tu base de datos `academic`
3. Abre el archivo `SQL_AGREGAR_COLUMNAS_MERCADOPAGO.sql`
4. Ejecuta el script (F5)
5. Verifica que aparezca "? Script completado exitosamente"
6. Reinicia tu aplicación

#### Columnas que se agregarán:
```sql
MercadoPagoInitPoint    nvarchar(500)   NULL
MercadoPagoPreferenceId nvarchar(200)   NULL
MercadoPagoPaymentId    nvarchar(200)   NULL
FechaPago              datetime2       NULL
PaidAmount             decimal(18,2)   NULL
```

---

### **Opción 2: Actualización Automática (Requiere Reinicio)**

El `DbInitializer` ya ha sido actualizado con el método `EnsureMatriculaColumnsAsync()` que verificará y agregará automáticamente las columnas faltantes al iniciar la aplicación.

#### Pasos:
1. **Detén** la aplicación (Ctrl+C o Stop en Visual Studio)
2. **Reinicia** la aplicación
3. El sistema automáticamente:
   - Verificará las columnas en `Matriculas`
   - Agregará las columnas faltantes
   - Registrará en logs las acciones realizadas
4. Listo para usar

#### Logs esperados:
```
info: Executing schema fix: ALTER TABLE [Matriculas] ADD [MercadoPagoPaymentId] nvarchar(200) NULL;
info: Executing schema fix: ALTER TABLE [Matriculas] ADD [FechaPago] datetime2 NULL;
...
```

---

## ?? **Verificación**

### Verificar Columnas en SQL:
```sql
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Matriculas'
ORDER BY COLUMN_NAME;
```

### Columnas Esperadas:
```
? AlumnoId
? CicloId
? CreatedAt
? EstadoPago
? FechaPago              ? NUEVA
? Id
? MercadoPagoInitPoint   ? NUEVA
? MercadoPagoPaymentId   ? NUEVA
? MercadoPagoPreferenceId ? NUEVA
? Moneda
? Monto
? PaidAmount             ? NUEVA
```

---

## ?? **Después de Aplicar la Solución**

### 1. Reiniciar la Aplicación
```bash
# Si estás en debug, detén y vuelve a ejecutar
# O usa Hot Reload (puede que no detecte cambios de BD)
```

### 2. Probar Matrícula
```
1. Ir a /Public/Matriculate
2. Completar formulario
3. El error debería desaparecer
4. Redirigirá a Mercado Pago correctamente
```

---

## ?? **Cambios Realizados en el Código**

### Archivo: `DbInitializer.cs`

#### Nuevo Método Agregado:
```csharp
private static async Task EnsureMatriculaColumnsAsync(AcademicContext context, ILogger? logger)
{
    var addCommands = new List<string>();

    // Verificar y agregar columnas faltantes
    var hasMercadoPagoPaymentId = await ColumnExistsAsync(context, "Matriculas", "MercadoPagoPaymentId");
    if (!hasMercadoPagoPaymentId)
    {
        addCommands.Add("ALTER TABLE [Matriculas] ADD [MercadoPagoPaymentId] nvarchar(200) NULL;");
    }
    
    // ... más columnas
    
    // Ejecutar comandos
    foreach (var cmd in addCommands)
    {
        await context.Database.ExecuteSqlRawAsync(cmd);
    }
}
```

#### Llamada Agregada en `ApplyMigrationsAsync`:
```csharp
try
{
    await EnsureCicloColumnsAsync(context, logger);
    await EnsureMatriculaColumnsAsync(context, logger); // ? NUEVO
    await EnsureTutorColumnsAsync(context, logger);
    // ...
}
```

---

## ?? **Seguridad de Datos**

### ¿Se perderán datos?
? **NO**. El script solo **agrega** columnas, no modifica ni elimina datos existentes.

### ¿Qué pasa con las matrículas existentes?
? Las matrículas existentes tendrán valores `NULL` en las nuevas columnas, lo cual es correcto.

---

## ?? **Troubleshooting**

### Error: "Cannot execute script"
**Solución**: 
- Asegúrate de estar conectado a la BD correcta
- Verifica permisos de ALTER TABLE

### Error: "Timeout"
**Solución**:
- Ejecuta el script en una ventana SQL separada
- Incrementa el timeout en las opciones

### Las columnas no aparecen
**Solución**:
1. Refresca el Object Explorer en SSMS
2. Verifica con el query de verificación
3. Reinicia la conexión a la BD

---

## ?? **Resumen de la Solución**

| Método | Tiempo | Complejidad | Recomendado |
|--------|--------|-------------|-------------|
| **SQL Script** | 10 segundos | Baja | ? Sí |
| **Reinicio App** | 1-2 minutos | Baja | ?? Alternativa |

---

## ? **Checklist de Corrección**

- [ ] **Opción 1**: Ejecutar `SQL_AGREGAR_COLUMNAS_MERCADOPAGO.sql`
  - [ ] Conectar a BD `academic`
  - [ ] Ejecutar script
  - [ ] Verificar mensaje de éxito
  
- [ ] **Opción 2**: Reiniciar aplicación
  - [ ] Detener aplicación
  - [ ] Iniciar aplicación
  - [ ] Revisar logs

- [ ] **Verificación Final**
  - [ ] Columnas visibles en SSMS/Azure Data Studio
  - [ ] Probar formulario de matrícula
  - [ ] Verificar redirección a Mercado Pago
  - [ ] Sin errores en consola

---

## ?? **Resultado Esperado**

Después de aplicar la solución:

```
? Formulario de matrícula funcional
? Redirección a Mercado Pago exitosa
? Tracking de pagos operativo
? Sin errores de columnas faltantes
```

---

## ?? **Soporte Adicional**

Si el problema persiste:

1. Verifica la cadena de conexión en `appsettings.json`
2. Asegúrate de estar usando la BD correcta
3. Revisa los logs completos de la aplicación
4. Comprueba permisos del usuario de BD

---

**Estado**: ?? Solución Lista  
**Acción Requerida**: Ejecutar SQL Script o Reiniciar App  
**Tiempo Estimado**: < 1 minuto  
**Impacto**: Sin pérdida de datos
