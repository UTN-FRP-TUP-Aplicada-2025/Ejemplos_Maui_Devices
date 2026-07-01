using Ejemplo_ws_Blazor.Components;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

namespace Ejemplo_ws_Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents().AddInteractiveServerComponents(o => o.DetailedErrors = true);

        // 1. Controllers
        builder.Services.AddControllers();

        // 2. Generación nativa del documento OpenAPI
        builder.Services.AddOpenApi();

/*
UseHttpsRedirection (Program.cs:37) puede redirigir de más o hacer lío.
La lógica sensible al esquema puede derivar http:///ws:// en vez de https:///wss:// → en iOS un ws:// (cleartext) desde una página https lo bloquea ATS, mientras que Android con usesCleartextTraffic lo deja pasar (justo el diferenciador candidato #2 del doc).
Antiforgery y cookies Secure dependen de IsHttps.
*/
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;

            // Por defecto solo confía en proxies loopback. Detrás de somee (proxy desconocido),
            // hay que vaciar estas listas para que ACEPTE las cabeceras. Tradeoff de seguridad:
            // solo hacelo si el borde de somee es la única vía de entrada.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });



        var app = builder.Build();

       // if (app.Environment.IsDevelopment())
        {
            // 3. Expone /openapi/v1.json
            app.MapOpenApi();
            // 4. UI de Scalar en /scalar
            app.MapScalarApiReference();
        }
        //else
        //{
        //    app.UseExceptionHandler("/Error");
        //    app.UseHsts();
        //}

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        
        app.UseForwardedHeaders();     // ← acá, arriba de todo
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // Mapeo de los controllers de API
        app.MapControllers();


        app.Run();
    }
}