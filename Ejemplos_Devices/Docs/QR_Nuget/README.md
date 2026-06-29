# QR / Barcode — Evaluación de NuGets para .NET MAUI

> **Objetivo:** Encontrar alternativas al NuGet `BarcodeScanner.Mobile.Maui` usado en
> `Ejemplo_Maui_Hibrida` y `Ejemplo_LectorQR_Dialog`, priorizando **APIs nativas en iOS
> (Apple Vision/AVFoundation)** para eliminar la dependencia actual de **ML Kit + Rosetta**.
>
> **Fecha:** 2026-06-27 · **Alcance:** evaluación read-only (sin cambios en código de los proyectos).

---

## Hallazgo central

El paquete actual `BarcodeScanner.Mobile.Maui` **9.0.1** (JimmyPun610) usa **Google ML Kit en
ambas plataformas, incluida iOS**. ML Kit en iOS **no publica slice de simulador `arm64`**, lo que
obliga al workaround `iossimulator-x64` + **Rosetta** (ver
[`propuesta-rosetta.md`](../propuesta-rosetta.md)). Rosetta está siendo retirado por Apple, por lo
que esa solución tiene fecha de caducidad. **La migración busca usar Apple Vision/AVFoundation
nativo en iOS.**

---

## Índice del documento

| # | Documento | Contenido | Tareas cubiertas |
|---|-----------|-----------|------------------|
| 1 | [01-analisis-uso-actual.md](01-analisis-uso-actual.md) | Uso actual de la librería, superficie de API, secuencia de llamadas iOS, permisos y acoplamiento de migración. | Tareas 1 y 2 |
| 2 | [02-alternativas-nuget.md](02-alternativas-nuget.md) | Fichas de las 5 alternativas (TFM, APIs nativas, autor/confianza, permisos, ejemplos de integración). | Tareas 3 y 4 |
| 3 | [03-matriz-rid.md](03-matriz-rid.md) | Matriz comparativa de RID / arquitecturas (arm64, arm, x86, simuladores, Windows, Mac Catalyst). | Tarea 4 |
| 4 | [04-recomendacion.md](04-recomendacion.md) | Conclusión, ranking por confianza + soporte nativo, y ruta de migración sugerida. | Tarea 5 |
| 5 | [05-plan-migracion.md](05-plan-migracion.md) | Plan paso a paso (diffs reales) de `QRLectorPage` hacia `BarcodeScanning.Native.Maui`. | Extra (implementación) |

---

## Resumen ejecutivo (TL;DR)

| Ranking | Paquete | iOS nativo | Autor / confianza | Veredicto |
|--------:|---------|-----------|-------------------|-----------|
| 🥇 1 | **BarcodeScanning.Native.Maui** | 🟢 Apple Vision | afriscic · MIT · ~856K desc · activo | **Recomendado** — elimina ML Kit/Rosetta en iOS |
| 🥈 2 | **CameraScanner.Maui** | 🟢 Apple VisionKit | T. Galliker · MIT · activo | Plan B equivalente, soporta net9 y net10 |
| 🥉 3 | **ZXing.Net.Maui** | 🟡 AVFoundation + decode managed | Redth/jfversluis (Microsoft) · MIT | Máxima portabilidad de CI/simuladores |
| — | **CameraMaui (fork janusw)** | 🟡 AVFoundation + decode managed | janusw · MIT | Solo si también necesitas cámara genérica |
| — | **Dynamsoft / Scandit / Scanbot** | 🔵 motor propio nativo | Empresas · de pago | Robustez enterprise / x86 / Windows desktop |

Leyenda: 🟢 decodificación nativa iOS · 🟡 preview nativo + decodificación *managed* · 🔵 motor propietario nativo.

---

## Convenciones

- ✅ soportado · ❌ no soportado · ⚠️ parcial / con limitación / no verificado.
- "Nativo iOS" = decodificación con **Apple Vision/VisionKit** (no ML Kit, no managed).
- Datos de NuGet/GitHub verificados a **junio 2026**; los puntos no confirmables se marcan "no verificado".
