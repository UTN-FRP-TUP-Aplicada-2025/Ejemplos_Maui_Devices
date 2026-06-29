# 3. Matriz comparativa de RID / arquitecturas

> Cubre la parte de **soporte de RID (arm / x86)** de la Tarea 4.
> ✅ soportado · ❌ no soportado · ⚠️ parcial / con limitación / no verificado.

[⬅ Volver al índice](README.md) · [⬅ Anterior: 2. Alternativas](02-alternativas-nuget.md)

---

## 3.1 Matriz por arquitectura

| Paquete | Android arm64-v8a | Android armeabi-v7a (arm) | Android x86 | Android x86_64 | iOS device arm64 | iOS sim arm64 | iOS sim x64 | Mac Catalyst | Windows |
|---|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| **BarcodeScanner.Mobile.Maui** *(actual, ML Kit iOS)* | ✅ | ✅ | ❌ | ✅ | ✅ | ⚠️ | ❌ | ❌ | ❌ |
| **BarcodeScanning.Native.Maui** *(ML Kit + Vision)* | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ❌ | ✅ | ❌ |
| **CameraScanner.Maui** *(ML Kit + VisionKit)* | ✅ | ✅ | ❌ | ✅ | ✅ | ⚠️ | ❌ | ❌ | ❌ |
| **ZXing.Net.Maui** *(managed)* | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ solo generación |
| **CameraMaui (fork janusw)** *(managed)* | ✅ | ✅ | ⚠️ | ✅ | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ |
| **Dynamsoft.BarcodeReaderBundle.Maui** *(motor propio)* | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | ❌ | ✅ (paquete hermano) |
| **Scandit.DataCapture.Barcode.Maui** *(motor propio)* | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ❌ | ⚠️ | ❌ |
| **ScanbotBarcodeSDK.MAUI** *(motor propio)* | ✅ | ✅ | ❌ | ✅ | ✅ | ⚠️ | ❌ | ❌ | ❌ |

---

## 3.2 Notas de interpretación

- **Android x86 (32-bit):** prácticamente extinto en dispositivos reales; solo relevante para emuladores antiguos. **ML Kit lo eliminó hace años**, por eso todas las librerías basadas en ML Kit lo marcan ❌. Solo **Dynamsoft** (motor propio) y **ZXing** (managed) lo cubren.
- **iOS simulador x64 (Intel):** solo aplica en Macs Intel. Los SDK de cámara con binarios nativos (`.xcframework`) suelen incluir **solo slice arm64**, por lo que el simulador x64 casi nunca funciona. En Macs Apple Silicon se usa el **simulador arm64**. Además, el simulador iOS **no tiene cámara real**, así que el escaneo en vivo se valida en dispositivo físico de todos modos.
- **Por qué importa para este proyecto:** el paquete actual usa **ML Kit en iOS** (sin slice de simulador arm64) → de ahí el workaround `iossimulator-x64` + Rosetta. Migrar a **BarcodeScanning.Native.Maui** (Apple Vision) habilita el **simulador arm64** y elimina Rosetta.
- **ZXing.Net.Maui** es el **único 100% managed** → cobertura completa de arquitecturas y simuladores, a costa de no usar APIs nativas de decodificación.
- Celdas ⚠️ "no verificado" no se pudieron confirmar de forma fehaciente en la documentación pública a junio 2026.

---

## 3.3 Lectura rápida según prioridad

| Si tu prioridad es… | Mejor opción |
|---|---|
| **Eliminar Rosetta / nativo iOS** | BarcodeScanning.Native.Maui · CameraScanner.Maui |
| **Correr en cualquier simulador/emulador (CI)** | ZXing.Net.Maui (managed) |
| **Android x86** | Dynamsoft · ZXing.Net.Maui |
| **Windows desktop** | Dynamsoft (paquete hermano) · ZXing (solo generación) |
| **Mac Catalyst** | BarcodeScanning.Native.Maui |

---

[⬅ Volver al índice](README.md) · [Siguiente: 4. Recomendación ➡](04-recomendacion.md)
