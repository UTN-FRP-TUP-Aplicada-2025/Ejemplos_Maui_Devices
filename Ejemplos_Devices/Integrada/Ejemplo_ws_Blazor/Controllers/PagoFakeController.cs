using Microsoft.AspNetCore.Mvc;

namespace Ejemplo_ws_Blazor.Controllers;


[ApiController]
[Route("api/pagofake")]
public class PagoFakeController : ControllerBase
{
    // (A) Aísla tu duda "¿es el redirect?": POST same-origin -> 302 a host EXTERNO.
    [HttpPost("pago")]
    [IgnoreAntiforgeryToken]                      // form de prueba, sin token
    public IActionResult PagoRedirect()
        => Redirect("https://httpbin.org/anything");   // 302 cross-host

    // (B) Imita EXACTO el pago real: devuelve HTML que autoenvía un form POST a otro host
    //     (igual que GenerarOrdenCobroMACRO devolvía el form de asjservicios).
    [HttpGet("pago-form")]
    public ContentResult PagoFormAutosubmit() => Content(
        """
        <!DOCTYPE html><html><body>
          <form method="post" action="https://httpbin.org/post" id="f">
            <input type="hidden" name="monto" value="500000" />
          </form>
          <script>document.forms[0].submit();</script>
        </body></html>
        """, "text/html");
}
