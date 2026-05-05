# Manejo de API Keys / Tokens en proyectos .NET MAUI

Cómo trabajar con claves de APIs (Google Maps, Firebase, etc.) sin exponerlas en el repositorio y sin que sea engorroso desarrollar.

---

## Opciones disponibles

### 1. Archivo local ignorado por git (lo más simple)
Un archivo `secrets.json` o similar agregado al `.gitignore`. Se lee al iniciar la app.

- **Pros**: rápido, sin dependencias.
- **Contras**: hay que acordarse del `.gitignore`, fácil de subir por error.

### 2. User Secrets de .NET (recomendado para desarrollo serio)
Mecanismo oficial de .NET. Las keys se guardan **fuera del repo**, en la carpeta del usuario:

- Windows: `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`

```bash
dotnet user-secrets init
dotnet user-secrets set "GoogleMaps:ApiKey" "AIza..."
```

Se cargan vía `Microsoft.Extensions.Configuration.UserSecrets` desde `MauiProgram.cs`.

- **Pros**: estándar, imposible de commitear por error, integrado con `IConfiguration`.
- **Contras**: solo aplica en Debug; para Release necesitás otra estrategia.

### 3. Clase estática + archivo template (didáctico, cero fricción) ⭐
Tener un `ApiKeys.cs` ignorado por git, y un `ApiKeys.cs.template` versionado como ejemplo:

```csharp
// ApiKeys.cs (ignorado por git)
namespace MiApp.Services;

internal static class ApiKeys
{
    public const string GoogleMaps = "AIza...";
}
```

```csharp
// ApiKeys.cs.template (versionado)
namespace MiApp.Services;

internal static class ApiKeys
{
    public const string GoogleMaps = "REEMPLAZAR_CON_TU_API_KEY";
}
```

Uso desde el código:

```csharp
private const string ApiKey = ApiKeys.GoogleMaps;
```

- **Pros**: cero fricción al usar (autocompletado, sin parseo JSON), compila offline.
- **Contras**: cada dev tiene que crear su archivo la primera vez.

### 4. SecureStorage de MAUI (para apps en producción)
`SecureStorage.SetAsync("apikey", "...")` guarda cifrado en el dispositivo (Keychain/KeyStore).
Útil cuando la key se obtiene en runtime (login, backend), no para keys de build.

### 5. Backend intermedio (lo más seguro)
La app llama a *tu* API y tu API llama a Google con la key. La key nunca vive en el cliente.

---

## Recomendación para esta cátedra

Usamos la **Opción 3** porque:

- Es didáctica y simple de explicar.
- No requiere paquetes NuGet extra ni configuración de `IConfiguration`.
- Funciona igual en Debug y Release.
- Cada alumno usa su propia key sin pisar la de los demás.

### Pasos para configurarlo en un proyecto

1. Crear el archivo `Services/ApiKeys.cs.template` con la estructura y un placeholder (este sí va al repo).
2. Crear el archivo `Services/ApiKeys.cs` con la key real (este **NO** va al repo).
3. Agregar al `.gitignore` de la raíz:

   ```gitignore
   # API keys locales (no subir al repo)
   **/Services/ApiKeys.cs
   ```

4. Verificar que git lo ignora correctamente:

   ```powershell
   git check-ignore -v Ejemplos_Devices/GPS/Ejemplo_Maui_GPS/Services/ApiKeys.cs
   ```

5. En el código usar `ApiKeys.GoogleMaps` (o el nombre del servicio correspondiente).

### Para nuevos clones del repo

```powershell
# Copiar el template y editar con la key propia
Copy-Item Services/ApiKeys.cs.template Services/ApiKeys.cs
```

Luego abrir `ApiKeys.cs` y reemplazar el placeholder con la key real.

---

## Buenas prácticas adicionales

- **Restringir la key en Google Cloud Console**: por package name de Android (+ SHA-1) y por API (solo Geocoding, no todas).
- **Cuotas y alertas de billing**: poner un límite diario para evitar sorpresas si la key se filtra.
- **Si la key se filtró**: revocarla y generar una nueva. No alcanza con borrar el commit (queda en el historial).
- **Nunca** poner keys reales en archivos versionados, comentarios, READMEs ni capturas de pantalla.
