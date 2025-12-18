using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MercadoPago.Config;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using Academic.Models;

namespace Academic.Services;

public sealed class MercadoPagoOptions
{
    public bool Enabled { get; set; } = true;
    public string Environment { get; set; } = "sandbox";
    public string? AccessToken { get; set; }
    public string? PublicKey { get; set; }
}

public sealed class MercadoPagoService
{
    private readonly MercadoPagoOptions _options;
    private readonly ILogger<MercadoPagoService> _logger;

    public MercadoPagoService(IConfiguration configuration, ILogger<MercadoPagoService> logger)
    {
        _options = configuration.GetSection("MercadoPago").Get<MercadoPagoOptions>() ?? new MercadoPagoOptions();
        _logger = logger;
        
        // Configurar SDK
        if (!string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            MercadoPagoConfig.AccessToken = _options.AccessToken;
        }
        else
        {
            _logger.LogWarning("MercadoPago Access Token no configurado");
        }
    }

    public async Task<(string? initPoint, string? preferenceId)> CreatePreferenceAsync(Matricula matricula, Academic.Models.Alumno alumno, Ciclo ciclo, string baseUrl)
    {
        try
        {
            // Asegurar que baseUrl esté completo y sin barra final
            if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
            {
                baseUrl = "http://" + baseUrl;
            }
            baseUrl = baseUrl.TrimEnd('/');
            
            // Generar un ID temporal único para tracking si no hay matrícula real
            var referenceId = matricula.Id > 0 ? matricula.Id.ToString() : Guid.NewGuid().ToString("N").Substring(0, 10);
            
            _logger.LogInformation("Preparando preferencia MP. BaseUrl: {BaseUrl}, Monto: {Monto}, Reference: {Ref}", 
                baseUrl, matricula.Monto, referenceId);
            
            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Id = $"MAT-{referenceId}",
                        Title = $"Matrícula {ciclo.Nombre}",
                        Description = $"Matrícula - {alumno.Nombre} {alumno.Apellido}",
                        CurrencyId = "PEN",
                        Quantity = 1,
                        UnitPrice = matricula.Monto
                    }
                },
                Payer = new PreferencePayerRequest
                {
                    Name = alumno.Nombre,
                    Surname = alumno.Apellido,
                    Email = alumno.Email
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{baseUrl}/Public/MatriculaResult?status=success",
                    Failure = $"{baseUrl}/Public/MatriculaResult?status=failure",
                    Pending = $"{baseUrl}/Public/MatriculaResult?status=pending"
                },
                ExternalReference = $"MATRICULA-{referenceId}",
                StatementDescriptor = "ACADEMIA ZOE"
            };
            
            _logger.LogInformation("URLs configuradas - Success: {Success}, Failure: {Failure}, Pending: {Pending}", 
                request.BackUrls.Success, request.BackUrls.Failure, request.BackUrls.Pending);

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(request);
            
            _logger.LogInformation("? Preferencia creada. ID: {PreferenceId}, InitPoint: {InitPoint}", preference.Id, preference.InitPoint);
            
            return (preference.InitPoint, preference.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al crear preferencia. MatriculaId: {MatriculaId}, BaseUrl: {BaseUrl}", matricula.Id, baseUrl);
            return (null, null);
        }
    }
}
