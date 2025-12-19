-- =============================================
-- Script: Sistema de Notas para Alumnos
-- Fecha: 2025-01-20
-- Descripción: Crea tabla Notas y actualiza configuración a Culqi por defecto
-- =============================================

USE [academic];
GO

PRINT '=== Iniciando implementación de Sistema de Notas ===';
GO

-- =============================================
-- 1. Crear tabla Notas
-- =============================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Notas')
BEGIN
    PRINT '? Creando tabla Notas...';
    
    CREATE TABLE [Notas](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AlumnoId] INT NOT NULL,
        [CicloId] INT NOT NULL,
        [SalonId] INT NULL,
        [Materia] NVARCHAR(200) NOT NULL,
        [Descripcion] NVARCHAR(500) NULL,
        [Calificacion] DECIMAL(5,2) NOT NULL,
        [Peso] DECIMAL(5,2) NOT NULL DEFAULT(1.0),
        [TipoEvaluacion] INT NOT NULL DEFAULT(0),
        [FechaEvaluacion] DATETIME2 NOT NULL,
        [RegistradoPor] NVARCHAR(256) NULL,
        [FechaRegistro] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        [Observaciones] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL DEFAULT(1),
        CONSTRAINT FK_Notas_Alumnos FOREIGN KEY (AlumnoId) REFERENCES [Alumnos](Id) ON DELETE CASCADE,
        CONSTRAINT FK_Notas_Ciclos FOREIGN KEY (CicloId) REFERENCES [Ciclos](Id) ON DELETE CASCADE,
        CONSTRAINT FK_Notas_Salones FOREIGN KEY (SalonId) REFERENCES [Salones](Id) ON DELETE SET NULL
    );
    
    PRINT '? Tabla Notas creada';
    
    -- Crear índices para performance
    PRINT '? Creando índices...';
    CREATE INDEX IX_Notas_AlumnoId ON Notas(AlumnoId);
    CREATE INDEX IX_Notas_CicloId ON Notas(CicloId);
    CREATE INDEX IX_Notas_FechaEvaluacion ON Notas(FechaEvaluacion);
    PRINT '? Índices creados';
END
ELSE
    PRINT '? Tabla Notas ya existe';

GO

-- =============================================
-- 2. Agregar columna PromedioGeneral a Alumnos
-- =============================================

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Alumnos' AND COLUMN_NAME = 'PromedioGeneral')
BEGIN
    PRINT '? Agregando columna PromedioGeneral a Alumnos...';
    ALTER TABLE [Alumnos] ADD [PromedioGeneral] DECIMAL(5,2) NULL;
    PRINT '? Columna PromedioGeneral agregada';
END
ELSE
    PRINT '? Columna PromedioGeneral ya existe';

GO

-- =============================================
-- 3. Actualizar Configuración a Culqi por defecto
-- =============================================

PRINT '? Actualizando configuración de pasarela a Culqi por defecto...';

UPDATE [ConfiguracionPasarelas]
SET 
    [PasarelaActiva] = 2,  -- Culqi
    [UltimaModificacion] = GETUTCDATE(),
    [ModificadoPor] = 'SYSTEM - Auto-config Culqi'
WHERE [Id] = 1;

PRINT '? Pasarela de pago configurada a Culqi por defecto';

GO

-- =============================================
-- 4. Verificar configuración
-- =============================================

PRINT '';
PRINT '=== Verificación del Sistema ===';

-- Verificar tabla Notas
PRINT 'Estructura de tabla Notas:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Notas'
ORDER BY ORDINAL_POSITION;

-- Verificar configuración de pasarela
PRINT '';
PRINT 'Configuración actual de pasarela:';
SELECT 
    Id,
    CASE PasarelaActiva
        WHEN 0 THEN 'Sin Pasarela (Manual)'
        WHEN 1 THEN 'MercadoPago'
        WHEN 2 THEN 'Culqi'
        ELSE 'Desconocida'
    END AS PasarelaActiva,
    UltimaModificacion,
    ModificadoPor
FROM ConfiguracionPasarelas;

GO

-- =============================================
-- 5. Stored Procedure para calcular promedio
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CalcularPromedioAlumno')
    DROP PROCEDURE sp_CalcularPromedioAlumno;
GO

CREATE PROCEDURE sp_CalcularPromedioAlumno
    @AlumnoId INT,
    @CicloId INT
AS
BEGIN
    DECLARE @Promedio DECIMAL(5,2);

    -- Calcular promedio ponderado
    SELECT @Promedio = 
        CASE 
            WHEN SUM(Peso) > 0 THEN SUM(Calificacion * Peso) / SUM(Peso)
            ELSE NULL
        END
    FROM Notas
    WHERE AlumnoId = @AlumnoId 
      AND CicloId = @CicloId 
      AND IsActive = 1;

    -- Actualizar promedio del alumno
    UPDATE Alumnos
    SET PromedioGeneral = @Promedio
    WHERE Id = @AlumnoId;

    -- Retornar el promedio calculado
    SELECT @Promedio AS PromedioCalculado;
END;
GO

PRINT '? Stored Procedure sp_CalcularPromedioAlumno creado';

GO

-- =============================================
-- 6. Función para obtener promedio por ciclo
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'fn_ObtenerPromedioCiclo')
    DROP FUNCTION fn_ObtenerPromedioCiclo;
GO

CREATE FUNCTION fn_ObtenerPromedioCiclo
(
    @AlumnoId INT,
    @CicloId INT
)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @Promedio DECIMAL(5,2);

    SELECT @Promedio = 
        CASE 
            WHEN SUM(Peso) > 0 THEN SUM(Calificacion * Peso) / SUM(Peso)
            ELSE NULL
        END
    FROM Notas
    WHERE AlumnoId = @AlumnoId 
      AND CicloId = @CicloId 
      AND IsActive = 1;

    RETURN @Promedio;
END;
GO

PRINT '? Función fn_ObtenerPromedioCiclo creada';

GO

-- =============================================
-- 7. Vista para reporte de notas
-- =============================================

IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_ReporteNotas')
    DROP VIEW vw_ReporteNotas;
GO

CREATE VIEW vw_ReporteNotas
AS
SELECT 
    n.Id AS NotaId,
    a.Id AS AlumnoId,
    a.Nombre + ' ' + a.Apellido AS NombreCompleto,
    a.DNI,
    a.Email,
    s.Nombre AS Salon,
    c.Nombre AS Ciclo,
    n.Materia,
    n.Descripcion,
    n.Calificacion,
    n.Peso,
    CASE n.TipoEvaluacion
        WHEN 0 THEN 'Práctica'
        WHEN 1 THEN 'Tarea'
        WHEN 2 THEN 'Examen Parcial'
        WHEN 3 THEN 'Examen Final'
        WHEN 4 THEN 'Proyecto'
        WHEN 5 THEN 'Exposición'
        WHEN 6 THEN 'Participación'
        WHEN 7 THEN 'Otro'
        ELSE 'N/A'
    END AS TipoEvaluacion,
    n.FechaEvaluacion,
    n.RegistradoPor,
    n.FechaRegistro,
    a.PromedioGeneral,
    CASE 
        WHEN a.PromedioGeneral >= 13 THEN 'Aprobado'
        WHEN a.PromedioGeneral >= 11 THEN 'Regular'
        WHEN a.PromedioGeneral IS NULL THEN 'Sin notas'
        ELSE 'Desaprobado'
    END AS Estado
FROM Notas n
INNER JOIN Alumnos a ON n.AlumnoId = a.Id
INNER JOIN Ciclos c ON n.CicloId = c.Id
LEFT JOIN Salones s ON n.SalonId = s.Id
WHERE n.IsActive = 1;
GO

PRINT '? Vista vw_ReporteNotas creada';

GO

-- =============================================
-- 8. Datos de ejemplo (opcional)
-- =============================================

PRINT '';
PRINT '=== Insertando notas de ejemplo (opcional) ===';

-- Solo insertar si existen alumnos y ciclos
IF EXISTS (SELECT 1 FROM Alumnos) AND EXISTS (SELECT 1 FROM Ciclos)
BEGIN
    DECLARE @AlumnoIdEjemplo INT;
    DECLARE @CicloIdEjemplo INT;
    
    SELECT TOP 1 @AlumnoIdEjemplo = Id FROM Alumnos WHERE IsActive = 1;
    SELECT TOP 1 @CicloIdEjemplo = Id FROM Ciclos;
    
    IF NOT EXISTS (SELECT 1 FROM Notas WHERE AlumnoId = @AlumnoIdEjemplo)
    BEGIN
        PRINT '? Insertando notas de ejemplo...';
        
        INSERT INTO Notas (AlumnoId, CicloId, Materia, Descripcion, Calificacion, Peso, TipoEvaluacion, FechaEvaluacion, RegistradoPor)
        VALUES 
            (@AlumnoIdEjemplo, @CicloIdEjemplo, 'Matemáticas', 'Práctica 1', 15.5, 1.0, 0, GETUTCDATE(), 'SYSTEM'),
            (@AlumnoIdEjemplo, @CicloIdEjemplo, 'Matemáticas', 'Examen Parcial', 16.0, 2.0, 2, GETUTCDATE(), 'SYSTEM'),
            (@AlumnoIdEjemplo, @CicloIdEjemplo, 'Física', 'Laboratorio 1', 14.0, 1.0, 0, GETUTCDATE(), 'SYSTEM');
        
        -- Calcular promedio
        EXEC sp_CalcularPromedioAlumno @AlumnoIdEjemplo, @CicloIdEjemplo;
        
        PRINT '? Notas de ejemplo insertadas';
    END
END

GO

-- =============================================
-- 9. Consultas de verificación
-- =============================================

PRINT '';
PRINT '=== Resumen del Sistema ===';

-- Total de notas
SELECT COUNT(*) AS TotalNotas FROM Notas WHERE IsActive = 1;

-- Alumnos con promedio
SELECT COUNT(*) AS AlumnosConPromedio FROM Alumnos WHERE PromedioGeneral IS NOT NULL;

-- Distribución de promedios
PRINT '';
PRINT 'Distribución de Promedios:';
SELECT 
    CASE 
        WHEN PromedioGeneral >= 16 THEN 'Excelente (16-20)'
        WHEN PromedioGeneral >= 13 THEN 'Aprobado (13-15)'
        WHEN PromedioGeneral >= 11 THEN 'Regular (11-12)'
        WHEN PromedioGeneral IS NULL THEN 'Sin notas'
        ELSE 'Desaprobado (0-10)'
    END AS Categoria,
    COUNT(*) AS Cantidad
FROM Alumnos
WHERE IsActive = 1
GROUP BY 
    CASE 
        WHEN PromedioGeneral >= 16 THEN 'Excelente (16-20)'
        WHEN PromedioGeneral >= 13 THEN 'Aprobado (13-15)'
        WHEN PromedioGeneral >= 11 THEN 'Regular (11-12)'
        WHEN PromedioGeneral IS NULL THEN 'Sin notas'
        ELSE 'Desaprobado (0-10)'
    END
ORDER BY Categoria;

GO

PRINT '';
PRINT '=== ? Sistema de Notas implementado exitosamente ===';
PRINT '';
PRINT 'Próximos pasos:';
PRINT '1. Ejecutar la aplicación: dotnet run';
PRINT '2. Login como Admin/Tutor/Coordinador';
PRINT '3. Ir a: /Admin/GestionarNotas';
PRINT '4. Registrar notas para los alumnos';
PRINT '';
PRINT 'Configuración de pasarela actualizada a Culqi por defecto';
PRINT '';

GO
