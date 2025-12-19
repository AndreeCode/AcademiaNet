-- ============================================
-- VERIFICACIÓN Y LIMPIEZA DE APODERADOS
-- ============================================

-- 1. Verificar apoderados en la base de datos
SELECT * FROM Apoderados;

-- 2. Verificar usuarios Identity de apoderados
SELECT u.Email, u.EmailConfirmed, r.Name AS Role
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Apoderado';

-- 3. Si necesitas limpiar y reiniciar (CUIDADO: Esto borra TODOS los datos)
/*
-- Eliminar apoderados existentes
DELETE FROM Apoderados;

-- Eliminar usuarios Identity de apoderados
DELETE FROM AspNetUserRoles 
WHERE RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Apoderado');

DELETE FROM AspNetUsers 
WHERE Email IN ('roberto.sanchez@example.com', 'carmen.lopez@example.com');
*/

-- 4. Verificar si existen los alumnos necesarios
SELECT Id, Nombre, Apellido, Email FROM Alumnos WHERE Email IN ('carlos@academia.local', 'pedro@academia.local');

-- 5. Insertar apoderados manualmente si no existen (después de ejecutar la aplicación)
/*
-- Primero verifica que los alumnos existan
DECLARE @AlumnoCarlosId INT = (SELECT Id FROM Alumnos WHERE Email = 'carlos@academia.local');
DECLARE @AlumnoPedroId INT = (SELECT Id FROM Alumnos WHERE Email = 'pedro@academia.local');

-- Insertar apoderados si no existen
IF NOT EXISTS (SELECT 1 FROM Apoderados WHERE Email = 'roberto.sanchez@example.com')
BEGIN
    INSERT INTO Apoderados (AlumnoId, Nombre, Apellido, DNI, Email, Telefono, Direccion, Parentesco, RecibeNotificaciones, FechaRegistro)
    VALUES (@AlumnoCarlosId, 'Roberto', 'Sanchez', '40123456', 'roberto.sanchez@example.com', '987654321', 'Av. Los Olivos 456', 'Padre', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Apoderados WHERE Email = 'carmen.lopez@example.com')
BEGIN
    INSERT INTO Apoderados (AlumnoId, Nombre, Apellido, DNI, Email, Telefono, Direccion, Parentesco, RecibeNotificaciones, FechaRegistro)
    VALUES (@AlumnoPedroId, 'Carmen', 'Lopez', '40234567', 'carmen.lopez@example.com', '987654322', 'Jr. Las Flores 789', 'Madre', 1, GETUTCDATE());
END
*/

-- 6. Verificar que los usuarios Identity de apoderados existan
/*
-- Si no existen, debes ejecutar la aplicación que los creará automáticamente
-- O crearlos manualmente (NO RECOMENDADO - mejor dejar que la app los cree)
*/

-- 7. Resultado final esperado
SELECT 
    a.Nombre + ' ' + a.Apellido AS Apoderado,
    a.Email AS EmailApoderado,
    a.DNI,
    a.Telefono,
    al.Nombre + ' ' + al.Apellido AS Alumno,
    al.Email AS EmailAlumno,
    a.Parentesco
FROM Apoderados a
INNER JOIN Alumnos al ON a.AlumnoId = al.Id;
