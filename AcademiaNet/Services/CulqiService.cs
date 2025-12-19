using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Academic.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Academic.Services;

public sealed class CulqiOptions
{
    public bool Enabled { get; set; } = true;
    public string Environment { get; set; } = "sandbox"; // sandbox | production
    public string? PublicKey { get; set; }
    public string? SecretKey { get; set; }
    public string? RsaId { get; set; }
    public string? RsaPublicKey { get; set; }
}

public sealed class CulqiService
{
    private readonly CulqiOptions _options;
    private readonly ILogger<CulqiService> _logger;
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://api.culqi.com/v2";

    public CulqiService(IConfiguration configuration, ILogger<CulqiService> logger, IHttpClientFactory httpClientFactory)
    {
        _options = configuration.GetSection("Culqi").Get<CulqiOptions>() ?? new CulqiOptions();
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Culqi");
        
        if (!string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.SecretKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);
        }
        else
        {
            _logger.LogWarning("Culqi Secret Key no configurado");
        }
    }

    /// <summary>
    /// Crea un cargo (charge) en Culqi
    /// </summary>
    public async Task<(bool Success, string? ChargeId, string? Error)> CreateChargeAsync(
        string tokenId, 
        decimal amount, 
        string email, 
        string description,
        string externalReference)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_options.SecretKey))
            {
                return (false, null, "Culqi Secret Key no configurado");
            }

            // Convertir el monto a centavos (Culqi trabaja en centavos)
            var amountInCents = (int)(amount * 100);

            var chargeRequest = new
            {
                amount = amountInCents,
                currency_code = "PEN",
                email = email,
                source_id = tokenId,
                description = description,
                metadata = new
                {
                    reference = externalReference
                }
            };

            var json = JsonSerializer.Serialize(chargeRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Creando cargo Culqi. Monto: {Amount} PEN, Email: {Email}", amount, email);

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/charges", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var chargeResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var chargeId = chargeResponse.GetProperty("id").GetString();
                
                _logger.LogInformation("? Cargo Culqi creado exitosamente. ChargeId: {ChargeId}", chargeId);
                return (true, chargeId, null);
            }
            else
            {
                _logger.LogError("? Error al crear cargo Culqi. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.GetProperty("user_message").GetString() ?? "Error desconocido";
                    return (false, null, errorMessage);
                }
                catch
                {
                    return (false, null, $"Error HTTP {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Excepción al crear cargo Culqi");
            return (false, null, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene información de un cargo
    /// </summary>
    public async Task<(bool Success, JsonElement? Charge, string? Error)> GetChargeAsync(string chargeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/charges/{chargeId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var charge = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return (true, charge, null);
            }
            else
            {
                _logger.LogError("Error al obtener cargo Culqi {ChargeId}: {Response}", chargeId, responseContent);
                return (false, null, $"Error HTTP {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al obtener cargo Culqi {ChargeId}", chargeId);
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Crea una orden de pago (para PagoEfectivo, Yape, etc.)
    /// </summary>
    public async Task<(bool Success, string? OrderId, string? Error)> CreateOrderAsync(
        decimal amount,
        string email,
        string firstName,
        string lastName,
        string phoneNumber,
        string description,
        string orderNumber,
        DateTime expirationDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_options.SecretKey))
            {
                return (false, null, "Culqi Secret Key no configurado");
            }

            var amountInCents = (int)(amount * 100);
            var expirationTimestamp = new DateTimeOffset(expirationDate).ToUnixTimeSeconds();

            var orderRequest = new
            {
                amount = amountInCents,
                currency_code = "PEN",
                description = description,
                order_number = orderNumber,
                client_details = new
                {
                    first_name = firstName,
                    last_name = lastName,
                    email = email,
                    phone_number = phoneNumber
                },
                expiration_date = expirationTimestamp,
                confirm = false
            };

            var json = JsonSerializer.Serialize(orderRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Creando orden Culqi. Monto: {Amount} PEN, OrderNumber: {OrderNumber}", amount, orderNumber);

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/orders", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var orderResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var orderId = orderResponse.GetProperty("id").GetString();
                
                _logger.LogInformation("? Orden Culqi creada exitosamente. OrderId: {OrderId}", orderId);
                return (true, orderId, null);
            }
            else
            {
                _logger.LogError("? Error al crear orden Culqi. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorMessage = errorResponse.GetProperty("user_message").GetString() ?? "Error desconocido";
                    return (false, null, errorMessage);
                }
                catch
                {
                    return (false, null, $"Error HTTP {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Excepción al crear orden Culqi");
            return (false, null, $"Error: {ex.Message}");
        }
    }

    public CulqiOptions GetOptions() => _options;
}
