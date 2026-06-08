using Ejemplo_ws_Blazor.Components;
using Scalar.AspNetCore;

namespace Ejemplo_ws_Blazor;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // 1. Controllers
        builder.Services.AddControllers();

        // 2. Generación nativa del documento OpenAPI
        builder.Services.AddOpenApi();

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
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // Mapeo de los controllers de API
        app.MapControllers();

        app.Run();
    }
}