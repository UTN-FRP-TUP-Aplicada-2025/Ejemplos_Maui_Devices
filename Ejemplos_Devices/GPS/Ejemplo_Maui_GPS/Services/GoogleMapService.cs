using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Ejemplo_Maui_GPS.Services;

public class GoogleMapService
{
    // La API key se carga desde ApiKeys.cs (archivo ignorado por git).
    // Ver Services/ApiKeys.cs.template para la plantilla.
    private const string ApiKey = ApiKeys.GoogleMaps;
    private const string GeocodeUrl = "https://maps.googleapis.com/maps/api/geocode/json";

    private static readonly HttpClient _httpClient = new();

    public async Task<string> GetDomicilioAsync(double latitude, double longitude)
    {
        try
        {
            var lat = latitude.ToString(CultureInfo.InvariantCulture);
            var lng = longitude.ToString(CultureInfo.InvariantCulture);
            var url = $"{GeocodeUrl}?latlng={lat},{lng}&key={ApiKey}&language=es";

            var response = await _httpClient.GetFromJsonAsync<GeocodeResponse>(url);

            if (response is null)
                return "Sin respuesta del servicio";

            if (!string.Equals(response.Status, "OK", StringComparison.OrdinalIgnoreCase))
                return $"Error: {response.Status} - {response.ErrorMessage}";

            if (response.Results is { Length: > 0 })
                return response.Results[0].FormattedAddress ?? "Domicilio no disponible";

            return "Domicilio no encontrado";
        }
        catch (Exception ex)
        {
            return $"Error al obtener domicilio: {ex.Message}";
        }
    }

    private sealed class GeocodeResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("results")]
        public GeocodeResult[]? Results { get; set; }
    }

    private sealed class GeocodeResult
    {
        [JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }
    }
}
