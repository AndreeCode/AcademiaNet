# INSTRUCCIONES PARA AGREGAR EL LOGO

## ?? Ubicación del Logo

La carpeta para imágenes ya ha sido creada en:
```
AcademiaNet\wwwroot\images\
```

## ?? Pasos para Agregar el Logo

### 1. Guardar la Imagen
- Guarda la imagen del logo como: `logo-zoe.png` o `logo-zoe.jpg`
- Ubicación: `AcademiaNet\wwwroot\images\logo-zoe.png`

### 2. Formato Recomendado
- **Formato**: PNG (con fondo transparente) o JPG
- **Tamaño**: 400x400px o similar (cuadrado)
- **Nombre**: `logo-zoe.png`

## ?? Lugares Donde se Agregará el Logo

El logo se agregará en los siguientes lugares:

### 1. ? **Navbar** (Todas las páginas)
- Ubicación: Esquina superior izquierda
- Tamaño: 40px de alto
- Enlace: Redirige a la página principal

### 2. ? **Página de Login**
- Ubicación: Encima del formulario
- Tamaño: 150px de ancho
- Centrado

### 3. ? **Página de Matrícula**
- Ubicación: Hero section
- Tamaño: 120px de ancho
- Junto al título

### 4. ? **Dashboard de Alumno**
- Ubicación: Header del dashboard
- Tamaño: 50px de alto

### 5. ? **Dashboard de Admin**
- Ubicación: Sidebar
- Tamaño: 100px de ancho
- Centrado

### 6. ? **Footer** (Todas las páginas)
- Ubicación: Centro del footer
- Tamaño: 60px de alto

## ?? Archivos que se Modificarán

1. `Pages/Shared/_Layout.cshtml` - Navbar y Footer
2. `Pages/Account/Login.cshtml` - Página de login
3. `Pages/Public/Matriculate.cshtml` - Página de matrícula
4. `Pages/Alumno/Dashboard.cshtml` - Dashboard del alumno
5. `Pages/Admin/Dashboard.cshtml` - Dashboard del admin

## ?? Código CSS para el Logo

```css
.logo-navbar {
    height: 40px;
    width: auto;
}

.logo-login {
    max-width: 150px;
    margin: 0 auto 2rem;
    display: block;
}

.logo-dashboard {
    max-width: 100px;
    margin: 1rem auto;
}

.logo-footer {
    height: 60px;
    width: auto;
}
```

## ? Características del Logo

- Fondo rojo (#E30613 o similar)
- Texto blanco
- Slogan: "SUEÑA, DECIDE E INGRESA"
- Estilo moderno y profesional

## ?? Una Vez Guardada la Imagen

Los archivos ya estarán actualizados para usar el logo en:
- `/images/logo-zoe.png`

Solo necesitas:
1. Guardar la imagen en `wwwroot\images\logo-zoe.png`
2. Refrescar el navegador
3. ¡El logo aparecerá automáticamente!

---

**Nota**: Si el logo no se muestra, verifica:
- Que el archivo esté en la ubicación correcta
- Que el nombre sea exactamente `logo-zoe.png`
- Que el servidor esté reiniciado (si usas Hot Reload puede que no detecte nuevas imágenes)
