# 4. Conclusión y recomendación

> Cubre la **Tarea 5**: recomendación según **confianza del autor** y **soporte nativo por plataforma**.

[⬅ Volver al índice](README.md) · [⬅ Anterior: 3. Matriz RID](03-matriz-rid.md)

---

## 4.1 Criterio de decisión

El criterio decisivo es doble:

1. **Soporte nativo en iOS** (Tarea 3.b) → eliminar **ML Kit + Rosetta** usando **Apple Vision/AVFoundation**.
2. **Confianza del autor** (Tarea 3.c) → librería viva, descargada y mantenida en 2025-2026.

Y un condicionante del proyecto: ambos targetean **net10**, y el acoplamiento está concentrado en
`QRLectorPage` (ver [1.6](01-analisis-uso-actual.md#16-acoplamiento--esfuerzo-de-migración)) → migración localizada.

---

## 4.2 Ranking

### 🥇 1 — BarcodeScanning.Native.Maui *(recomendación principal)*
Única open-source que combina **ML Kit (Android) + Apple Vision (iOS)**. Elimina el workaround
Rosetta/`iossimulator-x64`, mantiene rendimiento nativo en ambas plataformas, es **MIT**, muy
descargada (~856K) y activa, y su **3.0.4 ya encaja con net10**. Habilita el simulador iOS arm64 y
Mac Catalyst. **Mejor relación confianza + nativo + encaje con el proyecto.**

### 🥈 2 — CameraScanner.Maui
Stack equivalente (**ML Kit + Apple VisionKit**), de un autor de alta reputación (Thomas Galliker),
MIT y con soporte explícito de **net9 y net10**. Excelente **plan B** o segunda prueba comparativa.

### 🥉 3 — ZXing.Net.Maui
La de **mayor confianza de autor** (Redth + jfversluis, Microsoft) y **máxima portabilidad**
(funciona en simulador iOS x64 y emulador Android x86 por ser *managed*). Compromiso: decodificación
no nativa (algo menos robusta en baja luz/ángulos), aunque el preview de cámara sí es nativo.
**Elígela si priorizas compatibilidad de CI/simuladores sobre rendimiento de decodificación.**

### Descartar / casos especiales
- **`Camera.MAUI` original:** abandonado (sin net9/10). Usar el fork `CameraMaui` (janusw) solo si necesitas **cámara genérica además** del escaneo.
- **Comerciales (Dynamsoft / Scandit / Scanbot):** solo si necesitas **precisión enterprise**, **Android x86** (Dynamsoft) o **Windows desktop**, y puedes asumir licencias de pago.

---

## 4.3 Cuadro de decisión final

| Necesidad principal | Recomendación |
|---|---|
| Reemplazo nativo que elimine Rosetta en iOS (caso de este proyecto) | **BarcodeScanning.Native.Maui** |
| Igual que arriba pero requiriendo net9 explícito | **CameraScanner.Maui** |
| Robustez de build/CI en cualquier simulador o emulador | **ZXing.Net.Maui** |
| Precisión enterprise / Android x86 / Windows desktop | **Dynamsoft** (u otro comercial) |

---

## 4.4 Ruta de migración sugerida (no ejecutada)

> Pasos propuestos a futuro; **este informe no introduce cambios en código**.

1. **Prototipar** `BarcodeScanning.Native.Maui` en **`Ejemplo_LectorQR_Dialog`** (proyecto minimalista, menor riesgo).
2. Reemplazar en `QRLectorPage`:
   - `AddBarcodeScannerHandler()` → `.UseBarcodeScanning()`
   - `<gv:CameraView OnDetected=...>` → `<scanner:CameraView OnDetectionFinished=...>`
   - `Methods.SetSupportBarcodeFormat(...)` → opciones del nuevo control
   - `Methods.AskForRequiredPermission()` → `Permissions.RequestAsync<Permissions.Camera>()`
   - mapeo `BarcodeResult.{BarcodeType, DisplayValue}` → resultado de la nueva lib → `QRContent {Type, Value}`
3. Quitar el condicional `AdamE.Google.iOS.GoogleUtilities` y la dependencia de Rosetta del CSPROJ/pipeline.
4. **Validar** en **Moto g42 físico** (Android arm64) + **simulador iOS arm64** + iPhone físico.
5. Portar el mismo `QRLectorPage` ya validado a **`Ejemplo_Maui_Hibrida`** (donde además existe `OnQrCallback`).

> Verificar siempre la compatibilidad versión↔TFM: la 3.0.4 exige **.NET 10** (cumplido por ambos proyectos).

> 📄 El detalle con los **diffs concretos** de cada archivo está en [05-plan-migracion.md](05-plan-migracion.md).

---

[⬅ Volver al índice](README.md) · [Siguiente: 5. Plan de migración ➡](05-plan-migracion.md)
