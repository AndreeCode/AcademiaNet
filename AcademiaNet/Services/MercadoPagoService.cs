using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Academic.Models;

namespace Academic.Services;

public sealed class MercadoPagoOptions
{
    public string Environment { get; set; } = "sandbox";
    public string? AccessTokenSandbox { get; set; }
    public string? AccessTokenProduction { get; set; }
}

public sealed class MercadoPagoService
{
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoOptions _options;

    public MercadoPagoService(IConfiguration configuration)
    {
        _options = configuration.GetSection("MercadoPago").Get<MercadoPagoOptions>() ?? new MercadoPagoOptions();
        _httpClient = new HttpClient();
        var token = _options.Environment == "production" ? _options.AccessTokenProduction : _options.AccessTokenSandbox;
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<(string? initPoint, string? preferenceId)> CreatePreferenceAsync(Matricula matricula, string returnUrl)
    {
        var items = new[]
        {
            new { title = $"Matrícula Ciclo {matricula.CicloId}", quantity = 1, unit_price = Math.Round(matricula.Monto, 2) }
        };

        var payload = new
        {
            items,
            back_urls = new { success = returnUrl, failure = returnUrl, pending = returnUrl },
            auto_return = "approved"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var resp = await _httpClient.PostAsync("https://api.mercadopago.com/checkout/preferences", content);
        if (!resp.IsSuccessStatusCode)
        {
            return (null, null);
        }

        using var stream = await resp.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        var initPoint = root.TryGetProperty("init_point", out var ip) ? ip.GetString() : null;
        var prefId = root.TryGetProperty("id", out var idp) ? idp.GetRawText() : null;
        return (initPoint, prefId);
    }
}
