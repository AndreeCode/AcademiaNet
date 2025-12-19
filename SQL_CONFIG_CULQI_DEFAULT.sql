-- =============================================
-- Script: Configurar Culqi como Pasarela por Defecto
-- Descripción: Actualiza la configuración para usar Culqi
-- =============================================

USE [academic];
GO

PRINT '=== Configurando Culqi como Pasarela por Defecto ===';
GO

-- Actualizar configuración existente
UPDATE [ConfiguracionPasarelas]
SET 
    [PasarelaActiva] = 2,  -- Culqi
    [UltimaModificacion] = GETUTCDATE(),
    [ModificadoPor] = 'ADMIN - Config Culqi Default'
WHERE [Id] = 1;

-- Si no existe, crear una nueva
IF NOT EXISTS (SELECT 1 FROM [ConfiguracionPasarelas])
BEGIN
    INSERT INTO [ConfiguracionPasarelas] (PasarelaActiva, UltimaModificacion, ModificadoPor)
    VALUES (2, GETUTCDATE(), 'SYSTEM - Default Culqi');
END

PRINT '? Culqi configurado como pasarela por defecto';
GO

-- Verificar configuración
SELECT 
    Id,
    CASE PasarelaActiva
        WHEN 0 THEN 'Sin Pasarela'
        WHEN 1 THEN 'MercadoPago'
        WHEN 2 THEN 'Culqi'
        ELSE 'Desconocida'
    END AS PasarelaActiva,
    UltimaModificacion,
    ModificadoPor
FROM ConfiguracionPasarelas;
GO
