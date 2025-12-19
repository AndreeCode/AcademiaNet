-- =============================================
-- Script: Actualizar BD para Culqi Integration
-- Fecha: 2025-01-20
-- Descripción: Agrega columnas y tabla necesarias para Culqi
-- =============================================

USE [academic];
GO

PRINT '=== Iniciando actualización de BD para Culqi Integration ===';
GO

-- =============================================
-- 1. Agregar columnas a tabla Matriculas
-- =============================================

PRINT 'Verificando columnas de Matriculas...';

-- TipoPasarela
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'TipoPasarela')
BEGIN
    PRINT '  ? Agregando columna TipoPasarela...';
    ALTER TABLE [Matriculas] ADD [TipoPasarela] INT NOT NULL CONSTRAINT DF_Matriculas_TipoPasarela DEFAULT(0);
    PRINT '  ? Columna TipoPasarela agregada';
END
ELSE
    PRINT '  ? Columna TipoPasarela ya existe';

-- CulqiChargeId
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'CulqiChargeId')
BEGIN
    PRINT '  ? Agregando columna CulqiChargeId...';
    ALTER TABLE [Matriculas] ADD [CulqiChargeId] NVARCHAR(200) NULL;
    PRINT '  ? Columna CulqiChargeId agregada';
END
ELSE
    PRINT '  ? Columna CulqiChargeId ya existe';

-- CulqiTokenId
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'CulqiTokenId')
BEGIN
    PRINT '  ? Agregando columna CulqiTokenId...';
    ALTER TABLE [Matriculas] ADD [CulqiTokenId] NVARCHAR(200) NULL;
    PRINT '  ? Columna CulqiTokenId agregada';
END
ELSE
    PRINT '  ? Columna CulqiTokenId ya existe';

-- CulqiOrderId
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'CulqiOrderId')
BEGIN
    PRINT '  ? Agregando columna CulqiOrderId...';
    ALTER TABLE [Matriculas] ADD [CulqiOrderId] NVARCHAR(200) NULL;
    PRINT '  ? Columna CulqiOrderId agregada';
END
ELSE
    PRINT '  ? Columna CulqiOrderId ya existe';

GO

-- =============================================
-- 2. Crear tabla ConfiguracionPasarelas
-- =============================================

PRINT 'Verificando tabla ConfiguracionPasarelas...';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ConfiguracionPasarelas')
BEGIN
    PRINT '  ? Creando tabla ConfiguracionPasarelas...';
    
    CREATE TABLE [ConfiguracionPasarelas](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PasarelaActiva] INT NOT NULL DEFAULT(0),
        [UltimaModificacion] DATETIME2 NOT NULL DEFAULT(GETUTCDATE()),
        [ModificadoPor] NVARCHAR(256) NULL
    );
    
    PRINT '  ? Tabla ConfiguracionPasarelas creada';
    
    -- Insertar configuración por defecto
    PRINT '  ? Insertando configuración por defecto (SinPasarela)...';
    INSERT INTO [ConfiguracionPasarelas] (PasarelaActiva, UltimaModificacion)
    VALUES (0, GETUTCDATE());
    
    PRINT '  ? Configuración por defecto insertada';
END
ELSE
    PRINT '  ? Tabla ConfiguracionPasarelas ya existe';

GO

-- =============================================
-- 3. Verificar estructura final
-- =============================================

PRINT '';
PRINT '=== Verificación de estructura ===';

-- Verificar columnas de Matriculas
PRINT 'Columnas de Matriculas relacionadas a pasarelas:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Matriculas'
AND COLUMN_NAME IN (
    'TipoPasarela',
    'MercadoPagoInitPoint',
    'MercadoPagoPreferenceId',
    'MercadoPagoPaymentId',
    'CulqiChargeId',
    'CulqiTokenId',
    'CulqiOrderId'
)
ORDER BY ORDINAL_POSITION;

-- Verificar tabla ConfiguracionPasarelas
PRINT '';
PRINT 'Contenido de ConfiguracionPasarelas:';
SELECT * FROM ConfiguracionPasarelas;

GO

-- =============================================
-- 4. Índices opcionales (para performance)
-- =============================================

PRINT '';
PRINT '=== Creando índices opcionales ===';

-- Índice en TipoPasarela para consultas rápidas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Matriculas_TipoPasarela')
BEGIN
    PRINT '  ? Creando índice IX_Matriculas_TipoPasarela...';
    CREATE INDEX IX_Matriculas_TipoPasarela ON Matriculas(TipoPasarela);
    PRINT '  ? Índice IX_Matriculas_TipoPasarela creado';
END
ELSE
    PRINT '  ? Índice IX_Matriculas_TipoPasarela ya existe';

-- Índice en EstadoPago para consultas de matrículas pendientes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Matriculas_EstadoPago')
BEGIN
    PRINT '  ? Creando índice IX_Matriculas_EstadoPago...';
    CREATE INDEX IX_Matriculas_EstadoPago ON Matriculas(EstadoPago);
    PRINT '  ? Índice IX_Matriculas_EstadoPago creado';
END
ELSE
    PRINT '  ? Índice IX_Matriculas_EstadoPago ya existe';

GO

-- =============================================
-- 5. Consultas de verificación
-- =============================================

PRINT '';
PRINT '=== Estadísticas del sistema ===';

-- Total de matrículas por tipo de pasarela
PRINT 'Matrículas por tipo de pasarela:';
SELECT 
    CASE TipoPasarela
        WHEN 0 THEN 'Sin Pasarela'
        WHEN 1 THEN 'MercadoPago'
        WHEN 2 THEN 'Culqi'
        ELSE 'Desconocido'
    END AS TipoPasarela,
    COUNT(*) AS Total
FROM Matriculas
GROUP BY TipoPasarela;

-- Total de matrículas por estado de pago
PRINT '';
PRINT 'Matrículas por estado de pago:';
SELECT 
    CASE EstadoPago
        WHEN 0 THEN 'Pendiente'
        WHEN 1 THEN 'Pagado'
        WHEN 2 THEN 'Cancelado'
        WHEN 3 THEN 'Rechazado'
        ELSE 'Desconocido'
    END AS EstadoPago,
    COUNT(*) AS Total
FROM Matriculas
GROUP BY EstadoPago;

GO

PRINT '';
PRINT '=== ? Actualización completada exitosamente ===';
PRINT '';
PRINT 'Próximos pasos:';
PRINT '1. Verificar que las credenciales de Culqi estén en appsettings.json';
PRINT '2. Ir a /Admin/ConfigurarPasarela para seleccionar la pasarela activa';
PRINT '3. Probar el flujo de matrícula con la pasarela seleccionada';
PRINT '';

GO
