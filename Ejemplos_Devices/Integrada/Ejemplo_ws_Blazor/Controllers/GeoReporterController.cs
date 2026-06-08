using Ejemplo_ws_Blazor.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ejemplo_ws_Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GeoReporterController : ControllerBase
{
    private readonly ILogger<GeoReporterController> _logger;

    public GeoReporterController(ILogger<GeoReporterController> logger)
    {
        _logger = logger;
    }

    [HttpPost("track")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<string> GetTrackCoordenates([FromBody] LocationDto location)
    {
        // Con [ApiController], si el modelo es inválido (data annotations en el DTO)
        // ASP.NET Core ya devuelve 400 automáticamente antes de entrar acá.

        try
        {
            if (location is null)
                return BadRequest("No se recibió la ubicación.");

            var result = $"{location.Latitude}-{location.Longitude}";
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar las coordenadas {@Location}", location);
            return Problem(
                detail: "Ocurrió un error al procesar las coordenadas.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}