
## instalar la primera vez 


7zip
https://sourceforge.net/projects/sevenzip/

es requerido por el sdk de android


## nuget

C:\Users\fernando>dotnet --version
10.0.300

OneSignalSDK.DotNet

SkiaSharp
MetadataExtractor            //este se puede reemplazar

CommunityToolkit.Maui.Core
CommunityToolkit.Maui.Camera

## Ver sdk instalados

```
dotnet sdk check

C:\Users\fernando>dotnet sdk check
SDK de .NET:
Versión       Estado
--------------------------------------------------
8.0.406       La revisión 8.0.416 está disponible.
9.0.308       Está actualizado.
10.0.101      Está actualizado.

Entornos de ejecución .NET:
Nombre                            Versión      Estado
----------------------------------------------------------------------------------
Microsoft.AspNetCore.App          8.0.13       La revisión 8.0.22 está disponible.
Microsoft.NETCore.App             8.0.13       La revisión 8.0.22 está disponible.
Microsoft.WindowsDesktop.App      8.0.13       La revisión 8.0.22 está disponible.
Microsoft.AspNetCore.App          8.0.22       Está actualizado.
Microsoft.NETCore.App             8.0.22       Está actualizado.
Microsoft.WindowsDesktop.App      8.0.22       Está actualizado.
Microsoft.AspNetCore.App          9.0.11       Está actualizado.
Microsoft.NETCore.App             9.0.11       Está actualizado.
Microsoft.WindowsDesktop.App      9.0.11       Está actualizado.
Microsoft.AspNetCore.App          10.0.1       Está actualizado.
Microsoft.NETCore.App             10.0.1       Está actualizado.
Microsoft.WindowsDesktop.App      10.0.1       Está actualizado.


Las últimas versiones de .NET pueden instalarse desde https://aka.ms/dotnet-core-download. Para obtener más información sobre los ciclos de vida de .NET, consulte https://aka.ms/dotnet-core-support.


PS H:\repos\notions_sa\GDA.APP\dev.GDA.APP> dotnet --list-sdks
5.0.214 [C:\Program Files\dotnet\sdk]
7.0.317 [C:\Program Files\dotnet\sdk]
8.0.415 [C:\Program Files\dotnet\sdk]
9.0.305 [C:\Program Files\dotnet\sdk]
9.0.306 [C:\Program Files\dotnet\sdk]
```

## ver workloads instalados
```bash
PS H:\repos\notions_sa\GDA.APP\dev.GDA.APP> dotnet workload list --verbosity detailed

Id. de carga de trabajo instalada      Versión del manifiesto      Origen de la instalación
-------------------------------------------------------------------------------------------------
android                                35.0.105/9.0.100            SDK 9.0.300, VS 17.14.36616.10
android-36                             35.0.105/9.0.100            SDK 9.0.300
ios                                    26.0.9752/9.0.100           SDK 9.0.300, VS 17.14.36616.10
maccatalyst                            26.0.9752/9.0.100           SDK 9.0.300, VS 17.14.36616.10
maui-ios                               9.0.111/9.0.100             SDK 9.0.300
maui-windows                           9.0.111/9.0.100             SDK 9.0.300, VS 17.14.36616.10
```

```
PS H:\repos\notions_sa\GDA.APP\dev.GDA.APP> dotnet workload update --print-rollback
{
  "microsoft.net.sdk.android": "35.0.105/9.0.100",
  "microsoft.net.sdk.ios": "26.0.9752/9.0.100",
  "microsoft.net.sdk.maccatalyst": "26.0.9752/9.0.100",
  "microsoft.net.sdk.macos": "26.0.9752/9.0.100",
  "microsoft.net.sdk.maui": "9.0.111/9.0.100",
  "microsoft.net.sdk.tvos": "26.0.9752/9.0.100",
  "microsoft.net.workload.mono.toolchain.current": "9.0.10/9.0.100",
  "microsoft.net.workload.emscripten.current": "9.0.10/9.0.100",
  "microsoft.net.workload.emscripten.net6": "9.0.10/9.0.100",
  "microsoft.net.workload.emscripten.net7": "9.0.10/9.0.100",
  "microsoft.net.workload.emscripten.net8": "9.0.10/9.0.100",
  "microsoft.net.workload.mono.toolchain.net6": "9.0.10/9.0.100",
  "microsoft.net.workload.mono.toolchain.net7": "9.0.10/9.0.100",
  "microsoft.net.workload.mono.toolchain.net8": "9.0.10/9.0.100",
  "microsoft.net.sdk.aspire": "8.2.2/8.0.100"
}
```

## ver .net sdk
```
PS H:\repos\notions_sa\GDA.APP\dev.GDA.APP> dotnet --version
9.0.306
```

## actualizar workloads
```
dotnet workload update
dotnet workload install android
dotnet workload install maui
```

## revisar los workloads instalados
```
dotnet workload list
```