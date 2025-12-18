using Academic.Models;

namespace Academic.Services;

public class FileStorageService
{
    private readonly string _baseStoragePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _baseStoragePath = Path.Combine(env.ContentRootPath, "FileStorage");
        _logger = logger;
        EnsureStorageDirectoryExists();
    }

    private void EnsureStorageDirectoryExists()
    {
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
            _logger.LogInformation("Created base storage directory at: {Path}", _baseStoragePath);
        }
    }

    public string GetCicloPath(string cicloNombre)
    {
        var cicloPath = Path.Combine(_baseStoragePath, SanitizeFileName(cicloNombre));
        if (!Directory.Exists(cicloPath))
        {
            Directory.CreateDirectory(cicloPath);
        }
        return cicloPath;
    }

    public string GetSemanaPath(string cicloNombre, int numeroSemana)
    {
        var semanaPath = Path.Combine(GetCicloPath(cicloNombre), $"Semana_{numeroSemana:D2}");
        if (!Directory.Exists(semanaPath))
        {
            Directory.CreateDirectory(semanaPath);
        }
        return semanaPath;
    }

    public string GetTipoMaterialPath(string cicloNombre, int numeroSemana, TipoMaterial tipo)
    {
        var tipoFolder = tipo switch
        {
            TipoMaterial.PDF => "PDFs",
            TipoMaterial.Video => "Videos",
            TipoMaterial.Documento => "Documentos",
            _ => "Otros"
        };

        var tipoPath = Path.Combine(GetSemanaPath(cicloNombre, numeroSemana), tipoFolder);
        if (!Directory.Exists(tipoPath))
        {
            Directory.CreateDirectory(tipoPath);
        }
        return tipoPath;
    }

    public async Task<(bool Success, string? FilePath, string? Error)> SaveFileAsync(
        IFormFile file, 
        string cicloNombre, 
        int numeroSemana, 
        TipoMaterial tipoMaterial)
    {
        try
        {
            if (file == null || file.Length == 0)
                return (false, null, "No se proporcionó ningún archivo o el archivo está vacío.");

            // Validar tamaño del archivo (max 100MB)
            if (file.Length > 100 * 1024 * 1024)
                return (false, null, "El archivo excede el tamaño máximo permitido de 100MB.");

            var targetPath = GetTipoMaterialPath(cicloNombre, numeroSemana, tipoMaterial);
            var fileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
            var fullPath = Path.Combine(targetPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File saved successfully: {Path}", fullPath);
            return (true, fullPath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");
            return (false, null, $"Error al guardar el archivo: {ex.Message}");
        }
    }

    public bool DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {Path}", filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Path}", filePath);
            return false;
        }
    }

    public string GetRelativePath(string fullPath)
    {
        return Path.GetRelativePath(_baseStoragePath, fullPath);
    }

    public string GetFullPath(string relativePath)
    {
        return Path.Combine(_baseStoragePath, relativePath);
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    public long GetFileSize(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                return new FileInfo(filePath).Length;
            }
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    public string GetStorageBasePath()
    {
        return _baseStoragePath;
    }
}
