# ?? MONTO DE MATRÍCULA CONFIGURABLE - ACADEMIA ZOE

## ?? Resumen

Se ha implementado la funcionalidad para que el **administrador pueda definir el monto de matrícula** para cada ciclo académico.

### Características:
- ? Monto configurable por el administrador
- ? Valor por defecto: **S/ 1.00**
- ? Rango permitido: **S/ 1.00 a S/ 10,000.00**
- ? Se muestra en la página de matrícula
- ? Se usa automáticamente en Mercado Pago

---

## ?? Cambios Implementados

### 1. **Modelo Ciclo Actualizado**

#### Archivo: `Models/Ciclo.cs`

```csharp
public sealed class Ciclo
{
    // ...campos existentes...
    
    // Monto de matrícula para este ciclo (por defecto 1.00 PEN)
    public decimal MontoMatricula { get; set; } = 1.00m;
    
    // ...resto del modelo...
}
```

### 2. **Base de Datos Actualizada**

#### Migración Automática en `DbInitializer.cs`:

```csharp
var hasMontoMatricula = await ColumnExistsAsync(context, "Ciclos", "MontoMatricula");
if (!hasMontoMatricula)
{
    addCommands.Add("ALTER TABLE [Ciclos] ADD [MontoMatricula] decimal(18,2) NOT NULL CONSTRAINT DF_Ciclos_MontoMatricula DEFAULT(1.00);");
}
```

**La columna se agrega automáticamente** al iniciar la aplicación.

### 3. **Formulario de Crear Ciclo**

#### Vista: `Pages/Admin/CreateCycle.cshtml`

```html
<div class="mb-3">
    <label>Monto de Matrícula (PEN) *</label>
    <div class="input-group">
        <span class="input-group-text">S/</span>
        <input type="number" min="1.00" step="0.01" value="1.00" />
    </div>
    <small>Monto que pagará cada estudiante (por defecto S/ 1.00)</small>
</div>
```

#### Código: `Pages/Admin/CreateCycle.cshtml.cs`

```csharp
public class InputModel
{
    // ...campos existentes...
    
    [Required(ErrorMessage = "El monto de matrícula es requerido")]
    [Range(1.00, 10000.00, ErrorMessage = "El monto debe estar entre S/ 1.00 y S/ 10,000.00")]
    public decimal MontoMatricula { get; set; } = 1.00m;
}
```

### 4. **Formulario de Editar Ciclo**

#### Vista: `Pages/Admin/EditCycle.cshtml`

```html
<div class="mb-3">
    <label>Monto de Matrícula (PEN) *</label>
    <div class="input-group">
        <span class="input-group-text">S/</span>
        <input type="number" min="1.00" step="0.01" />
    </div>
    <small>Monto que pagará cada estudiante al matricularse</small>
</div>
```

### 5. **Matrícula Pública**

#### Vista: `Pages/Public/Matriculate.cshtml`

Ahora muestra el monto en el hero section:

```html
<div class="badge bg-success fs-6 mt-2 ms-2">
    <i class="bi bi-cash"></i> S/ @Model.CurrentCiclo.MontoMatricula.ToString("N2")
</div>
```

#### Código: `Pages/Public/Matriculate.cshtml.cs`

```csharp
var matricula = new Matricula
{
    AlumnoId = alumno.Id,
    CicloId = CurrentCiclo.Id,
    Monto = CurrentCiclo.MontoMatricula, // ? Usa el monto del ciclo
    Moneda = "PEN",
    EstadoPago = EstadoPago.Pendiente,
    CreatedAt = DateTime.UtcNow
};
```

---

## ?? Cómo Funciona

### Flujo Completo:

```
1. Admin crea/edita ciclo
   ?
2. Define MontoMatricula (ej: S/ 150.00)
   ?
3. Ciclo se guarda en BD
   ?
4. Usuario accede a /Public/Matriculate
   ?
5. Ve el monto en pantalla: "S/ 150.00"
   ?
6. Completa formulario y envía
   ?
7. Sistema crea matrícula con Monto = 150.00
   ?
8. Redirige a Mercado Pago con monto S/ 150.00
   ?
9. Usuario paga S/ 150.00
   ?
10. Sistema confirma pago y matrícula
```

---

## ?? Casos de Uso

### Caso 1: Crear Ciclo con Monto Personalizado

```
Admin ? Crear Ciclo
?
Nombre: Ciclo 2025-III
Fechas: ...
Monto Matrícula: S/ 250.00 ? Personalizado
?
Guardar
?
? Ciclo creado con monto S/ 250.00
```

### Caso 2: Crear Ciclo con Monto Por Defecto

```
Admin ? Crear Ciclo
?
Nombre: Ciclo Gratuito 2025
Fechas: ...
Monto Matrícula: S/ 1.00 ? Valor por defecto
?
Guardar
?
? Ciclo creado con monto S/ 1.00
```

### Caso 3: Editar Monto de Ciclo Existente

```
Admin ? Editar Ciclo
?
Ciclo actual: S/ 1.00
Cambiar a: S/ 200.00
?
Guardar
?
? Nuevas matrículas pagarán S/ 200.00
?? Matrículas existentes mantienen su monto original
```

---

## ?? Validaciones Implementadas

| Campo | Validación | Mensaje de Error |
|-------|-----------|------------------|
| **MontoMatricula** | Requerido | "El monto de matrícula es requerido" |
| **MontoMatricula** | Mínimo: 1.00 | "El monto debe estar entre S/ 1.00 y S/ 10,000.00" |
| **MontoMatricula** | Máximo: 10,000.00 | "El monto debe estar entre S/ 1.00 y S/ 10,000.00" |
| **MontoMatricula** | Formato: Decimal | Automático por tipo de dato |

---

## ?? Ejemplos de Configuración

### 1. **Ciclo Regular (S/ 150.00)**
```
Ciclo 2025-III
MontoMatricula: 150.00
? Matrículas cobran S/ 150.00
```

### 2. **Ciclo de Prueba (S/ 1.00)**
```
Ciclo Piloto 2025
MontoMatricula: 1.00
? Matrículas cobran S/ 1.00 (simbólico)
```

### 3. **Ciclo Premium (S/ 500.00)**
```
Ciclo Intensivo 2025
MontoMatricula: 500.00
? Matrículas cobran S/ 500.00
```

### 4. **Ciclo con Descuento (S/ 75.00)**
```
Ciclo Promoción 2025
MontoMatricula: 75.00
? Matrículas cobran S/ 75.00
```

---

## ?? Vista Previa

### Página de Matrícula:

```
??????????????????????????????????????????
? [LOGO ZOE]                             ?
? Matricúlate en Academia Zoe            ?
? Sueña, Decide e Ingresa                ?
?                                        ?
? [Ciclo 2025-III] [S/ 150.00]         ?
??????????????????????????????????????????
```

### Formulario de Crear Ciclo:

```
??????????????????????????????????????????
? Crear Nuevo Ciclo Académico            ?
??????????????????????????????????????????
? Nombre: [Ciclo 2025-III]              ?
? Fechas: [...] [....]                   ?
? Vacantes: [100]                        ?
? Modalidad: [Híbrido]                   ?
?                                        ?
? Monto de Matrícula (PEN):             ?
? [S/] [150.00]                         ?
? ??  Monto que pagará cada estudiante  ?
?                                        ?
? [Cancelar] [Crear Ciclo]              ?
??????????????????????????????????????????
```

---

## ?? SQL Manual (Opcional)

Si prefieres agregar la columna manualmente en SQL:

```sql
USE [academic];
GO

-- Agregar columna MontoMatricula si no existe
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Ciclos' 
    AND COLUMN_NAME = 'MontoMatricula'
)
BEGIN
    ALTER TABLE [Ciclos] 
    ADD [MontoMatricula] decimal(18,2) NOT NULL 
    CONSTRAINT DF_Ciclos_MontoMatricula DEFAULT(1.00);
    
    PRINT '? Columna MontoMatricula agregada';
END
ELSE
BEGIN
    PRINT '- MontoMatricula ya existe';
END
GO

-- Actualizar ciclos existentes con valor por defecto si es necesario
UPDATE [Ciclos]
SET [MontoMatricula] = 1.00
WHERE [MontoMatricula] IS NULL OR [MontoMatricula] = 0;
GO

-- Verificar
SELECT Id, Nombre, MontoMatricula
FROM [Ciclos]
ORDER BY Id DESC;
GO
```

---

## ? Checklist de Implementación

- [x] Modelo `Ciclo` actualizado con `MontoMatricula`
- [x] Migración automática en `DbInitializer`
- [x] Formulario de crear ciclo actualizado
- [x] Formulario de editar ciclo actualizado
- [x] Página de matrícula muestra el monto
- [x] Matrícula usa monto del ciclo
- [x] Mercado Pago recibe monto correcto
- [x] Validaciones implementadas
- [x] Valor por defecto S/ 1.00
- [x] Compilación exitosa

---

## ?? Notas Importantes

### 1. **Matrículas Existentes**
- Las matrículas ya creadas **mantienen** su monto original
- Solo las **nuevas matrículas** usan el monto actualizado del ciclo

### 2. **Cambio de Monto**
- Si cambias el monto de un ciclo, solo afecta a **futuras matrículas**
- Las matrículas ya procesadas **no se modifican**

### 3. **Monto Mínimo**
- El monto mínimo es **S/ 1.00** (simbólico)
- No se permiten matrículas gratuitas (S/ 0.00)
- Esto es para mantener compatibilidad con Mercado Pago

### 4. **Formato de Moneda**
- Se usa formato decimal `(18,2)`
- Permite hasta **2 decimales**
- Ejemplos válidos: `1.00`, `150.50`, `9999.99`

---

## ?? Próximos Pasos

### Mejoras Sugeridas:

1. **Descuentos**:
   - Agregar campo `MontoDescuento` para promociones
   - Calcular monto final = `MontoMatricula - MontoDescuento`

2. **Monedas Múltiples**:
   - Permitir USD, EUR, etc.
   - Agregar campo `MonedaMatricula` al ciclo

3. **Historial de Precios**:
   - Tabla `PreciosHistorico` para auditoría
   - Registrar cambios de monto con fecha

4. **Pagos Parciales**:
   - Permitir pago en cuotas
   - Campo `NumeroCuotas` en ciclo

---

## ?? Resultado Final

### Admin puede:
- ? Definir monto de matrícula al crear ciclo
- ? Modificar monto de ciclo existente
- ? Usar monto por defecto (S/ 1.00)
- ? Ver monto en el dashboard admin

### Estudiante ve:
- ? Monto de matrícula en página de inscripción
- ? Monto exacto que pagará
- ? Proceso de pago con monto correcto

### Sistema garantiza:
- ? Monto mínimo de S/ 1.00
- ? Monto máximo de S/ 10,000.00
- ? Validación de formato decimal
- ? Integración correcta con Mercado Pago

---

**Estado**: ? IMPLEMENTADO Y FUNCIONAL  
**Compilación**: ? Exitosa  
**Base de Datos**: ? Actualizada automáticamente  
**Listo para**: Usar inmediatamente

**¡El administrador ya puede definir el monto de matrícula para cada ciclo!** ??
