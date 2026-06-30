# Propuesta: compilar el simulador iOS en `x86_64` (Rosetta) para sortear la limitación de Google ML Kit

> **Estado:** Propuesta para reevaluar en el workspace de pruebas de pipelines.
> **Ámbito:** Job de CD iOS (build de simulador) en pipelines tipo `cd-main.yml` / `cd-ios.yml`.
> **No incluye:** cambios de código de la app ni de dependencias NuGet. Solo YAML del workflow.
> **Origen del análisis:** fallo de compilación en `Analisis/log.qr.build.ios.md`.

---

## 1. Contexto y problema

El job de iOS construye **dos** binarios distintos:

| Build | RuntimeIdentifier | Propósito | ¿Falla? |
|-------|-------------------|-----------|---------|
| **Simulador** | `iossimulator-arm64` | Smoke test: arranca la app en el simulador del runner y graba un **GIF de evidencia** | ❌ **Sí** |
| **Release / IPA** | `ios-arm64` | Entregable real para iPhone físico | ✅ No (compila bien) |

El build de **simulador** falla en el enlazado nativo (`ld`/`clang++`):

```
ld: building for 'iOS-simulator', but linking in object file
(.../XamarinBuildDownload/MLKCommon-12.0.0/Frameworks/MLKitCommon.framework/MLKitCommon[arm64][2](MLKAnalyticsLogger.o))
built for 'iOS'
clang++: error: linker command failed with exit code 1
```

### Causa raíz

- El runner es **Apple Silicon (arm64)** (`runs-on: macos-15`), por lo que el RID del simulador es `iossimulator-arm64`.
- La librería de QR **`BarcodeScanner.Mobile.Maui` 9.0.1** usa por debajo el **SDK nativo de Google ML Kit** (`MLKitCommon.framework`, pod `12.0.0`).
- Google ML Kit **NO publica un slice de simulador `arm64`**. Solo provee:
  - `arm64` → **device** (por eso el IPA `ios-arm64` compila bien),
  - `x86_64` → **simulador** (Intel / Rosetta).
- En Apple Silicon, device y simulador comparten CPU arm64, pero el Mach-O los distingue por el flag `LC_BUILD_VERSION` (`PLATFORM_IOS` vs `PLATFORM_IOSSIMULATOR`). `ld` rechaza mezclar device + simulador → error.

**Conclusión:** no es un problema de la app ni de iOS en general. Es una limitación conocida y **aún abierta** de ML Kit: no soporta el simulador arm64.

> La app en **iPhones reales funciona perfecto**. El fallo es exclusivo del simulador arm64 en CI.

---

## 2. Decisión propuesta

Compilar el **build de simulador** para **`iossimulator-x64` (x86_64)** y ejecutarlo bajo **Rosetta 2**, ya que ML Kit **sí** trae el slice x86_64 de simulador.

Ventaja: **mantiene la validación en simulador + el GIF de evidencia, sin migrar de librería ni tocar código**.

### Dos niveles de cambio (importante)

| Objetivo | Requiere |
|----------|----------|
| **(A)** Que el `dotnet build` deje de fallar (error de linker) | Solo el **Cambio 1** (RID → `x64`) |
| **(B)** Que además la app **corra** en el simulador (GIF) | **Cambios 1 + 2 + 3** |

---

## 3. Cambios propuestos en el workflow

> Localizá los puntos por **nombre de variable / step**, no solo por número de línea (los pipelines de prueba pueden diferir).

### Cambio 1 — RID del simulador *(obligatorio: arregla la compilación)*

Buscar la variable de entorno del RID de simulador (en el bloque `env:`):

```yaml
# ANTES
RUNTIME_IDENTIFIER_SIMULATOR: 'iossimulator-arm64'

# DESPUÉS
RUNTIME_IDENTIFIER_SIMULATOR: 'iossimulator-x64'
```

**Efecto:** se propaga automáticamente a los pasos de `dotnet restore`, `dotnet build` y al cálculo de `BASE_PATH`/`APP_PATH` del simulador. El linker pasa a enlazar contra el slice x86_64 que ML Kit sí provee. **Solo con esto el build deja de fallar.**

> ⚠️ Verificá que ningún paso esté **condicionado** al string literal `iossimulator-arm64` (p. ej. `if:` en el step de grabación del GIF, presente en algunos pipelines tipo `cd-ios.yml`). Si existe, actualizá esa condición a `iossimulator-x64` o el paso se saltará.

---

### Cambio 2 — Descargar el runtime de simulador con slice x86_64 *(para que la app corra)*

En Xcode 26, `xcodebuild -downloadPlatform iOS` baja **solo el runtime arm64** por defecto. Hay que pedir la variante **Universal** (arm64 + x86_64).

Buscar el step que descarga el simulador (tipo *"descarga el simulador requerido"*):

```yaml
# ANTES
- name: XCODE. descarga el simulador requerido
  run: |
    sudo xcodebuild -downloadPlatform iOS
    echo "Espacio actual: $(df -Th )"

# DESPUÉS
- name: XCODE. descarga el simulador requerido
  run: |
    sudo xcodebuild -downloadPlatform iOS -architectureVariant universal
    echo "Espacio actual: $(df -Th )"
```

**Efecto:** instala el runtime con el slice x86_64, requisito para ejecutar la app x86_64 bajo Rosetta.
**Contra menor:** ocupa más disco (vigilar `df -h`).

---

### Cambio 3 — Instalar Rosetta 2 en el runner *(para que la app corra)*

Rosetta 2 es el traductor que ejecuta el binario x86_64 sobre el chip arm64. En runners Apple Silicon de GitHub **no siempre viene preinstalado**.

Agregar un step **antes** del paso de grabación de video/GIF (idealmente temprano, p. ej. tras el checkout o la instalación de Xcode):

```yaml
- name: ROSETTA. Instalar traductor x86_64 -> arm64
  run: |
    softwareupdate --install-rosetta --agree-to-license
```

**Efecto:** habilita la ejecución (`simctl launch`) del binario x86_64 en el simulador. Es la pieza que materializa el "Rosetta simulator".

---

## 4. Qué NO hay que cambiar

| Elemento | Motivo |
|----------|--------|
| `Utils/simular.sh` (script de simulación/GIF) | Es **agnóstico de arquitectura**: opera por UUID y `PACKAGE_NAME`. Funciona igual con app x86_64 + Rosetta. |
| Código de la app / `QRLectorPage` | El error es de toolchain/CI, no de código. |
| `BarcodeScanner.Mobile.Maui` y demás NuGet | No se migra ni se actualiza nada. |
| Build RELEASE / IPA (`ios-arm64`) | **Nunca estuvo afectado**; ML Kit sí trae slice device-arm64. |

---

## 5. Resumen del alcance

| # | Dónde | Cambio | Imprescindible para… |
|---|-------|--------|----------------------|
| 1 | `env:` → `RUNTIME_IDENTIFIER_SIMULATOR` | `iossimulator-arm64` → `iossimulator-x64` | Arreglar el **build** |
| 2 | Step "descarga el simulador" | `+ -architectureVariant universal` | Que la app **corra** |
| 3 | Step nuevo (Rosetta) | `softwareupdate --install-rosetta --agree-to-license` | Que la app **corra** |

**Total: 3 ajustes en un único archivo YAML. Sin tocar código ni dependencias.**

---

## 6. Riesgos y puntos a vigilar

1. **Rendimiento:** el simulador correrá x86_64 traducido → más lento. Revisar que los `sleep`/timeouts del script de simulación (arranque ~120s) tengan margen. Síntoma de fallo: GIF vacío o timeout de arranque.
2. **`simctl` + Rosetta:** con runtime Universal + Rosetta instalados, `simctl install/launch` de un binario x86_64 corre transparente. Si no levantara, el síntoma aparece en el step de grabación (que suele tener `continue-on-error: true`, así que no rompe el pipeline).
3. **Disco:** la variante Universal pesa más; vigilar `df -h` si el runner va justo de espacio.
4. **Fecha de caducidad:** esta solución depende de **Rosetta 2**, que Apple está retirando a futuro (soporte garantizado en las próximas versiones de macOS y luego limitado). **Funciona hoy; no es definitiva.**

---

## 7. Checklist de validación (post-cambio)

- [ ] El step de `dotnet build` del simulador termina con `0 Error(s)` (ya no aparece `clang++ exited with code 1` / `building for 'iOS-simulator'`).
- [ ] El runtime Universal quedó instalado (`xcrun simctl list runtimes` muestra iOS 26 disponible).
- [ ] Rosetta quedó instalado (el step `softwareupdate --install-rosetta` no falló).
- [ ] El step de simulación instala la app sin "app fantasma" y `simctl launch` devuelve PID.
- [ ] Se genera `evidencia_app.gif` no vacío.
- [ ] El build RELEASE / IPA (`ios-arm64`) sigue compilando igual que antes (no debe haber cambiado).

---

## 8. Rollback

Revertir es trivial y sin efectos colaterales:
1. `RUNTIME_IDENTIFIER_SIMULATOR` → `iossimulator-arm64`.
2. Quitar `-architectureVariant universal`.
3. Eliminar el step de Rosetta.

(El IPA de device no se ve afectado en ningún sentido.)

---

## 9. Alternativas consideradas (descartadas para este caso)

| Opción | Por qué se descartó |
|--------|---------------------|
| **Eliminar el build de simulador** | Se pierde la validación/GIF que el equipo quiere conservar. |
| **Runner Intel `macos-13`** | En deprecación; Xcode 26 puede no estar disponible; el flujo de descarga manual de Xcode asume arm64. |
| **Excluir ML Kit solo en simulador** | El código referencia el escáner; romper o "stubbear" deja el simulador sin QR y es alto esfuerzo. |
| **Migrar a un escáner sin ML Kit (ZXing.Net.Maui u otro basado en Vision/AVFoundation)** | Única solución *a prueba de futuro* (corre nativo en simulador arm64), pero implica migración de API + re-test en Android e iOS device. Recomendable a largo plazo, no como fix inmediato. |

---

## 10. Referencias

- ML Kit no soporta simuladores arm64 (feature request abierto): https://github.com/googlesamples/mlkit/issues/810
- ML Kit: build para iOS Simulator falla en Apple Silicon: https://issuetracker.google.com/issues/178965151
- Falta soporte arm64-simulator en simuladores iOS 26: https://github.com/capawesome-team/capacitor-mlkit/issues/291
- Restaurar simuladores x86_64 (Rosetta) en Xcode 26: https://iifx.dev/en/articles/460237100/how-to-restore-x86-64-rosetta-simulators-in-xcode-26-on-macos-tahoe
- Descarga de componentes de Xcode (`-downloadPlatform`, `-architectureVariant universal`): https://developer.apple.com/tutorials/data/documentation/xcode/downloading-and-installing-additional-xcode-components.md
