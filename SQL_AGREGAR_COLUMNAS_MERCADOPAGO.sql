-- Script para agregar columnas de Mercado Pago a la tabla Matriculas
-- Ejecutar este script en SQL Server Management Studio o Azure Data Studio

USE [academic];
GO

-- Verificar si existe la tabla Matriculas
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Matriculas')
BEGIN
    PRINT 'Tabla Matriculas encontrada. Verificando columnas...'
    
    -- Agregar MercadoPagoInitPoint si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'MercadoPagoInitPoint')
    BEGIN
        ALTER TABLE [Matriculas] ADD [MercadoPagoInitPoint] nvarchar(500) NULL;
        PRINT '? Columna MercadoPagoInitPoint agregada'
    END
    ELSE
        PRINT '- MercadoPagoInitPoint ya existe'
    
    -- Agregar MercadoPagoPreferenceId si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'MercadoPagoPreferenceId')
    BEGIN
        ALTER TABLE [Matriculas] ADD [MercadoPagoPreferenceId] nvarchar(200) NULL;
        PRINT '? Columna MercadoPagoPreferenceId agregada'
    END
    ELSE
        PRINT '- MercadoPagoPreferenceId ya existe'
    
    -- Agregar MercadoPagoPaymentId si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'MercadoPagoPaymentId')
    BEGIN
        ALTER TABLE [Matriculas] ADD [MercadoPagoPaymentId] nvarchar(200) NULL;
        PRINT '? Columna MercadoPagoPaymentId agregada'
    END
    ELSE
        PRINT '- MercadoPagoPaymentId ya existe'
    
    -- Agregar FechaPago si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'FechaPago')
    BEGIN
        ALTER TABLE [Matriculas] ADD [FechaPago] datetime2 NULL;
        PRINT '? Columna FechaPago agregada'
    END
    ELSE
        PRINT '- FechaPago ya existe'
    
    -- Agregar PaidAmount si no existe
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Matriculas' AND COLUMN_NAME = 'PaidAmount')
    BEGIN
        ALTER TABLE [Matriculas] ADD [PaidAmount] decimal(18,2) NULL;
        PRINT '? Columna PaidAmount agregada'
    END
    ELSE
        PRINT '- PaidAmount ya existe'
    
    PRINT ''
    PRINT '? Script completado exitosamente'
END
ELSE
BEGIN
    PRINT '? ERROR: Tabla Matriculas no encontrada'
END
GO

-- Verificar columnas agregadas
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Matriculas'
    AND COLUMN_NAME IN ('MercadoPagoInitPoint', 'MercadoPagoPreferenceId', 'MercadoPagoPaymentId', 'FechaPago', 'PaidAmount')
ORDER BY COLUMN_NAME;
GO
