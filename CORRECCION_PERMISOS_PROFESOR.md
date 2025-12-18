# ?? CORRECCIÓN: PERMISOS DE MATERIALES Y SEMANAS PARA PROFESORES

## ? **Problema Detectado**

Los profesores no podían acceder a las páginas de **Materiales** y **Semanas** porque les faltaban permisos.

### **Error que Aparecía:**
```
HTTP 403 - Forbidden
o
Página de error de Google (acceso denegado)
```

---

## ? **Solución Aplicada**

He actualizado los atributos `[Authorize]` en **3 páginas** para incluir el rol **Profesor**.

### **Archivos Modificados:**

#### **1. `/Materiales/Index.cshtml.cs`**

**? Antes:**
```csharp
[Authorize(Roles = "Admin,Tutor,Coordinador")]
```

**? Ahora:**
```csharp
[Authorize(Roles = "Admin,Tutor,Coordinador,Profesor")]
```

---

#### **2. `/Materiales/Subir.cshtml.cs`**

**? Antes:**
```csharp
[Authorize(Roles = "Admin,Tutor,Coordinador")]
```

**? Ahora:**
```csharp
[Authorize(Roles = "Admin,Tutor,Coordinador,Profesor")]
```

---

#### **3. `/Semanas/Gestionar.cshtml.cs`**

**? Antes:**
```csharp
[Authorize(Roles = "Admin,Coordinador")]
```

**? Ahora:**
```csharp
[Authorize(Roles = "Admin,Coordinador,Profesor,Tutor")]
```

---

## ?? **Matriz de Permisos Actualizada**

| Página | Admin | Coordinador | Profesor | Tutor | Alumno |
|--------|-------|-------------|----------|-------|--------|
| **Materiales/Index** | ? | ? | ? | ? | ? |
| **Materiales/Subir** | ? | ? | ? | ? | ? |
| **Semanas/Gestionar** | ? | ? | ? | ? | ? |

---

## ?? **Cómo Probar**

### **1. Reiniciar la Aplicación**
```sh
Ctrl+C (detener)
dotnet run
```

### **2. Login como Profesor**
```
URL: http://localhost:5042/Profesor/Dashboard
Email: profesor@academia.local
Password: Prof123!
```

### **3. Probar Enlaces del Sidebar**

#### **A. Materiales**
```
Click en: Materiales
URL esperada: /Materiales/Index
Resultado: ? Debe mostrar listado de materiales
```

#### **B. Semanas**
```
Click en: Semanas
URL esperada: /Semanas/Gestionar
Resultado: ? Debe mostrar gestión de semanas
```

---

## ?? **Funcionalidades Disponibles para Profesor**

### **A. Materiales (/Materiales/Index)**

**Puede:**
- ? Ver todos los materiales
- ? Buscar materiales por título
- ? Filtrar por ciclo
- ? Eliminar materiales
- ? Acceder a "Subir Material"

**Interfaz:**
```
??????????????????????????????????????????
? ?? Materiales                          ?
??????????????????????????????????????????
? [Buscar: _______] [Filtro Ciclo: ?]   ?
? [+ Subir Material]                     ?
??????????????????????????????????????????
? Semana 1 - Matemáticas                ?
? Semana 2 - Física                     ?
? Semana 3 - Química                    ?
??????????????????????????????????????????
```

---

### **B. Subir Material (/Materiales/Subir)**

**Puede:**
- ? Subir archivos (PDF, Word, Excel, PowerPoint)
- ? Agregar enlaces externos
- ? Asignar a ciclo y semana
- ? Categorizar por tipo (Documento, Enlace, etc.)

**Formulario:**
```
Título: [_______________________]
Descripción: [___________________]
Ciclo: [Ciclo 2025-II ?]
Semana: [Semana 3 ?]
Tipo: [Documento ?]
Archivo: [Seleccionar archivo...]
[Subir Material]
```

---

### **C. Gestionar Semanas (/Semanas/Gestionar)**

**Puede:**
- ? Ver semanas del ciclo
- ? Crear nuevas semanas
- ? Generar semanas automáticamente
- ? Editar temas y descripción
- ? Ver materiales asociados

**Interfaz:**
```
??????????????????????????????????????????
? ?? Gestionar Semanas                   ?
??????????????????????????????????????????
? Ciclo: [Ciclo 2025-II ?]              ?
? [Generar 12 semanas automáticamente]   ?
??????????????????????????????????????????
? Semana 1: 01/02 - 07/02               ?
? Semana 2: 08/02 - 14/02               ?
? Semana 3: 15/02 - 21/02               ?
? [+ Crear Nueva Semana]                 ?
??????????????????????????????????????????
```

---

## ?? **Flujo Completo de Trabajo del Profesor**

### **1. Crear Semanas**
```
Dashboard ? Semanas ?
Seleccionar Ciclo ?
Click "Generar semanas" ?
? 12 semanas creadas
```

### **2. Subir Material**
```
Dashboard ? Materiales ?
Click "Subir Material" ?
Completar formulario:
  - Título: "Ecuaciones Diferenciales"
  - Ciclo: Ciclo 2025-II
  - Semana: Semana 3
  - Archivo: ecuaciones.pdf
? Click "Subir Material"
? Material subido
```

### **3. Ver Materiales**
```
Dashboard ? Materiales ?
Ver listado de todos los materiales ?
Buscar: "Ecuaciones" ?
? Material encontrado
```

### **4. Eliminar Material**
```
Materiales ? Index ?
Click [Eliminar] en un material ?
Confirmar ?
? Material eliminado
```

---

## ?? **Casos de Uso**

### **Caso 1: Profesor Planifica Ciclo**

```
1. Login como profesor@academia.local
   ?
2. Dashboard ? Click "Semanas"
   ?
3. Seleccionar "Ciclo 2025-II"
   ?
4. Click "Generar 12 semanas"
   ?
5. ? Semanas creadas automáticamente
   ?
6. Editar temas de cada semana
```

### **Caso 2: Profesor Sube Material**

```
1. Dashboard ? Click "Materiales"
   ?
2. Click "Subir Material"
   ?
3. Formulario:
   - Título: "Guía de Laboratorio"
   - Ciclo: 2025-II
   - Semana: 4
   - Archivo: lab_guia.pdf
   ?
4. Click "Subir"
   ?
5. ? Material disponible para alumnos
```

### **Caso 3: Profesor Comparte Enlace**

```
1. Dashboard ? Materiales ? Subir
   ?
2. Formulario:
   - Título: "Video Explicativo"
   - Tipo: Enlace
   - URL: https://youtube.com/watch?v=...
   - Semana: 5
   ?
3. Click "Subir"
   ?
4. ? Enlace compartido con alumnos
```

---

## ?? **Solución de Problemas**

### **Problema: Sigue dando error 403**

**Causas posibles:**
1. ? No reiniciaste la aplicación
2. ? Sesión cacheada

**Solución:**
```
1. Cerrar sesión
2. Ctrl+C (detener app)
3. dotnet run
4. Login nuevamente
5. Intentar acceder a Materiales/Semanas
```

---

### **Problema: No ve el sidebar con Materiales/Semanas**

**Causa:**
- La vista del dashboard no se actualizó

**Solución:**
1. Verificar que `Dashboard.cshtml` del profesor tenga:
```html
<a asp-page="/Materiales/Index">Materiales</a>
<a asp-page="/Semanas/Gestionar">Semanas</a>
```

2. Forzar recarga: `Ctrl + Shift + R`

---

### **Problema: Página en blanco o error de servidor**

**Posibles causas:**
1. Error en la base de datos
2. Servicio FileStorage no configurado

**Verificar logs:**
```
Revisar la consola donde ejecutas dotnet run
Buscar errores en rojo
```

**Solución común:**
- Verificar que existe la carpeta `wwwroot/uploads`
- Verificar permisos de escritura

---

## ?? **Estructura de Archivos de Materiales**

```
wwwroot/
??? uploads/
    ??? Ciclo 2025-II/
    ?   ??? Semana 1/
    ?   ?   ??? Documentos/
    ?   ?   ?   ??? apuntes.pdf
    ?   ?   ??? Videos/
    ?   ??? Semana 2/
    ?   ?   ??? Documentos/
    ?   ?       ??? ejercicios.docx
    ?   ??? ...
    ??? ...
```

---

## ? **Checklist de Verificación**

Después de reiniciar, verifica:

- [ ] Login como `profesor@academia.local` funciona
- [ ] Dashboard del profesor se muestra
- [ ] Sidebar tiene enlace "Materiales"
- [ ] Sidebar tiene enlace "Semanas"
- [ ] Click en "Materiales" ? muestra `/Materiales/Index`
- [ ] Click en "Semanas" ? muestra `/Semanas/Gestionar`
- [ ] Puede ver listado de materiales
- [ ] Puede hacer click en "Subir Material"
- [ ] Puede seleccionar ciclo en Semanas
- [ ] Puede generar semanas automáticamente

---

## ?? **Resumen de Cambios**

| Aspecto | Antes | Ahora |
|---------|-------|-------|
| **Materiales/Index** | ? Prohibido para Profesor | ? Permitido |
| **Materiales/Subir** | ? Prohibido para Profesor | ? Permitido |
| **Semanas/Gestionar** | ? Prohibido para Profesor y Tutor | ? Permitido |

---

## ?? **Próximos Pasos**

1. **Reiniciar aplicación**: `dotnet run`
2. **Login como profesor**: `profesor@academia.local / Prof123!`
3. **Probar enlaces**: Materiales y Semanas
4. **Crear semanas**: Generar 12 semanas para el ciclo actual
5. **Subir material**: Probar subida de un PDF de prueba

---

**Estado**: ? **CORREGIDO**  
**Compilación**: ? **Exitosa**  
**Listo para**: **Usar inmediatamente**

**¡Los profesores ahora pueden acceder a Materiales y Semanas sin errores!** ??
