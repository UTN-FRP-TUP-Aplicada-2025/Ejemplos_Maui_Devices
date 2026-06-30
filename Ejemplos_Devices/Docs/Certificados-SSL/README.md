# Certificados SSL/TLS en Android: resolver `Trust anchor for certification path not found`

> Guía didáctica (es-AR) para una app **.NET MAUI Híbrida** cuyo **WebView (Android System WebView = Chromium)** abre una URL HTTPS y falla con
> `Trust anchor for certification path not found`.
>
> Caso de estudio: `https://aplicada.somee.com` en un **Moto g42 (Android 12/13)**.
> La técnica sirve como **referencia reutilizable para cualquier dominio**.

**Documentos de esta guía:**

- `README.md` (este archivo): resumen ejecutivo, conceptos, diagnóstico, procedimiento, solución aplicada, errores comunes y mantenimiento.
- [`Anexo-A-Comandos.md`](./Anexo-A-Comandos.md): comandos de inspección (openssl, PowerShell + `X509Chain`, adb) con salidas de ejemplo.
- [`Anexo-B-Glosario.md`](./Anexo-B-Glosario.md): glosario PKI/TLS.

---

## 1. Resumen ejecutivo (TL;DR)

| | |
|---|---|
| **Síntoma** | El WebView (Chromium) no abre `https://aplicada.somee.com`. En el log (`cr_X509Util`) aparece: `Failed to validate the certificate chain, error: java.security.cert.CertPathValidatorException: Trust anchor for certification path not found.` |
| **Causa raíz** | La raíz de la cadena es **`ISRG Root X2`** (Let's Encrypt / ISRG), una raíz **ECDSA de 2020** que **no está en el almacén de confianza** de Android en dispositivos viejos. Sin esa raíz, el validador **no puede cerrar la cadena**. |
| **NO es** | **No es Sectigo.** Esa fue una suposición equivocada del setup original (incluso el script se llamaba `download_sectigo.ps1`). La CA real es **Let's Encrypt / ISRG**. |
| **Solución** | Embeber la **raíz** (y, por robustez, los **2 intermedios**) en `res/raw` como `.pem`, y declararlos como `trust-anchors` para ese dominio en `network_security_config.xml`. |
| **Archivos clave** | `Platforms/Android/Resources/xml/network_security_config.xml`, `Platforms/Android/Resources/raw/*.pem`, `Platforms/Android/AndroidManifest.xml` |

Rutas reales en este repo (relativas a la raíz del proyecto MAUI `Ejemplos_Devices/Integrada/Ejemplo_Maui_Hibrida/`):

```
Platforms/Android/
├── AndroidManifest.xml
└── Resources/
    ├── xml/
    │   └── network_security_config.xml
    └── raw/
        ├── isrg_root_x2.pem     <- raíz (trust anchor) ISRG Root X2
        ├── root_ye.pem          <- intermedio  CN=Root YE, O=ISRG
        └── ye2.pem              <- intermedio  CN=YE2, O=Let's Encrypt
```

---

## 2. Conceptos previos (PKI / TLS en 5 minutos)

Si ya manejás PKI, saltá a la [sección 3](#3-diagnóstico-del-error). El [Anexo B](./Anexo-B-Glosario.md) tiene el glosario completo.

### 2.1. PKI y certificados X.509

- **PKI (Public Key Infrastructure):** conjunto de reglas, roles y entidades (las **Autoridades de Certificación / CA**) que permiten **confiar** en una clave pública sin conocer a su dueño de antemano.
- **Par de claves:** cada servidor TLS tiene una **clave privada** (secreta) y una **clave pública** (compartible). Lo que se cifra/firma con una sólo se verifica con la otra.
- **Certificado X.509:** un archivo que dice *"esta clave pública pertenece a `aplicada.somee.com`"* y viene **firmado** por una CA. Contiene, entre otros campos:
  - **Subject** (`s:`): a quién identifica (ej. `CN=aplicada.somee.com`).
  - **Issuer** (`i:`): qué CA lo **emitió/firmó**.
  - Clave pública, período de validez (`NotBefore` / `NotAfter`), extensiones, etc.
- **Firma:** la CA calcula un hash del certificado y lo cifra con **su** clave privada. Cualquiera con la clave pública de la CA puede verificar que el certificado no fue alterado y que efectivamente lo emitió esa CA.

### 2.2. Cadena de certificación: hoja, intermedios y raíz

Ningún servidor presenta un único certificado "mágico". Presenta una **cadena**, donde cada certificado está firmado por el de arriba:

```
[0] HOJA / leaf        CN=aplicada.somee.com        emitido por -> YE2
        ^ firmado por
[1] INTERMEDIO         CN=YE2, O=Let's Encrypt       emitido por -> Root YE
        ^ firmado por
[2] INTERMEDIO         CN=Root YE, O=ISRG            emitido por -> ISRG Root X2
        ^ firmado por
[3] RAÍZ (anchor)      CN=ISRG Root X2, O=ISRG       AUTO-FIRMADO (issuer == subject)
```

- **Hoja (leaf):** el certificado del servidor; el que lleva el nombre de dominio.
- **Intermedios:** CAs subordinadas. Existen para que la **clave privada de la raíz** se use lo menos posible (se guarda offline). La raíz firma intermedios, y los intermedios firman las hojas.
- **Raíz (root):** está en el **tope** y es **auto-firmada** → su `Issuer` es igual a su `Subject`. No la firma nadie más; en ella la confianza es **axiomática**.

### 2.3. ¿Qué es un "trust anchor"?

Un **trust anchor** ("ancla de confianza") es un certificado **en el que el cliente confía por decisión propia**, sin pedir que nadie más lo avale. Normalmente es una **raíz** preinstalada en el sistema operativo o el navegador.

La validación de una cadena consiste en **encadenar firmas desde la hoja hacia arriba hasta llegar a un trust anchor que el cliente ya tenga**. Si la cadena no termina en un ancla conocida, **la validación falla** — exactamente el error de esta guía.

### 2.4. ¿Por qué el servidor NO envía la raíz?

Por diseño del protocolo TLS, **el servidor envía la hoja y los intermedios, pero NO la raíz**. Razones:

1. **No agrega seguridad:** si el cliente no tuviera ya la raíz, recibirla del propio servidor no probaría nada (un atacante mandaría "su" raíz). La raíz **debe** venir de una fuente confiable: el almacén del cliente.
2. **Ahorra bytes** en cada handshake.

En nuestro caso, el servidor envía `[0]`, `[1]` y `[2]`, y **espera que el cliente ya tenga `[3] ISRG Root X2`**. En desktop/celulares modernos así es. En un Android viejo, **no**.

### 2.5. ¿Por qué a un Android viejo le puede faltar una raíz nueva?

El almacén de raíces (`system`) de Android se **congela** con la versión del sistema y se actualiza con parches/OTA. `ISRG Root X2` es una raíz **relativamente nueva (2020) y ECDSA**. Dispositivos que dejaron de recibir actualizaciones (o que nunca incluyeron esa raíz ECDSA) **no la tienen**. Y Android **no descarga** automáticamente raíces/intermedios faltantes vía **AIA** (*Authority Information Access*) como sí hacen algunos navegadores de escritorio. Resultado: la cadena queda **incompleta** del lado del dispositivo.

### 2.6. DER vs PEM

Son dos **codificaciones** del mismo certificado X.509:

| Formato | Qué es | Extensión típica |
|---|---|---|
| **DER** | Binario (ASN.1 crudo) | `.der`, `.cer`, `.crt` |
| **PEM** | DER en Base64 entre `-----BEGIN CERTIFICATE-----` / `-----END CERTIFICATE-----` (texto) | `.pem`, `.crt`, `.cer` |

`network_security_config.xml` acepta **ambos**. En esta guía usamos **PEM** (`.pem`) porque es legible en texto y fácil de versionar en git.

---

## 3. Diagnóstico del error

### 3.1. El mensaje exacto

```
Failed to validate the certificate chain, error:
java.security.cert.CertPathValidatorException:
Trust anchor for certification path not found.
```

Traducción literal: *"No se encontró un ancla de confianza para la ruta de certificación."* Es decir: el dispositivo **construyó la cadena hasta donde pudo**, pero el certificado del tope (o el emisor del último que recibió) **no coincide con ninguna raíz que el dispositivo confíe**. La cadena queda **abierta** y la validación se aborta.

> No confundir con otros errores TLS:
> - `Hostname mismatch` / `CertificateException: ... does not match` → el `CN`/`SAN` no coincide con el dominio.
> - `Certificate expired` → fechas vencidas.
> - **`Trust anchor ... not found`** → el problema es **el cierre de la cadena**, no el nombre ni la fecha. Es el nuestro.

### 3.2. La cadena real (verificada)

Verificada con `openssl s_client` y con `X509Chain` de .NET (ver [Anexo A](./Anexo-A-Comandos.md)):

| # | Rol | Subject (`s:`) | Issuer (`i:`) | ¿Lo envía el server? |
|---|---|---|---|---|
| 0 | **Hoja** | `CN=aplicada.somee.com` | `CN=YE2, O=Let's Encrypt, C=US` | Sí |
| 1 | Intermedio | `CN=YE2, O=Let's Encrypt, C=US` | `CN=Root YE, O=ISRG, C=US` | Sí |
| 2 | Intermedio | `CN=Root YE, O=ISRG, C=US` | `CN=ISRG Root X2, O=ISRG, C=US` | Sí |
| 3 | **Raíz (anchor)** | `CN=ISRG Root X2, O=Internet Security Research Group, C=US` | *(auto-firmado)* | **NO** (debe estar en el cliente) |

```
   Servidor envía  ───────────────────────────►   El cliente debe tener
   ┌───────────────────────────────────┐           ┌───────────────────────┐
   │ [0] aplicada.somee.com  (hoja)     │           │ [3] ISRG Root X2      │
   │ [1] YE2        (intermedio)        │  ── ??? ──►│     (RAÍZ / anchor)   │
   │ [2] Root YE    (intermedio)        │           │  FALTA en Moto g42 →  │
   └───────────────────────────────────┘           │  cadena ABIERTA       │
                                                    └───────────────────────┘
```

### 3.3. Conclusión del diagnóstico

- En **Windows** la cadena valida sin problema (Windows **sí** tiene `ISRG Root X2`). Eso confirma que **el problema es específico de Android viejo**, no del servidor ni del certificado.
- El servidor está **bien configurado** (envía hoja + ambos intermedios).
- Falta **una sola pieza** en el dispositivo: la raíz `ISRG Root X2`. **Esa** es la que hay que aportar.

---

## 4. Solución: `network_security_config.xml` + `res/raw`

Desde **Android 7.0 (API 24)**, una app puede declarar su política de confianza TLS en un archivo XML, sin tocar código Java/C#. Es el mecanismo **recomendado** (mejor que parchear el `TrustManager` a mano).

### 4.1. Anatomía del archivo

- **`<base-config>`**: política **por defecto** para todo el tráfico que no caiga en un `<domain-config>`.
- **`<domain-config>`**: política **específica** para uno o varios dominios. **Tiene prioridad** sobre `base-config`.
- **`<domain includeSubdomains="true">`**: el dominio (y, si `includeSubdomains`, sus subdominios) al que aplica el bloque.
- **`<trust-anchors>`**: lista de **fuentes** de anclas de confianza. Cada `<certificates>` es una fuente:

| `src` | Significado | Uso recomendado |
|---|---|---|
| `"system"` | Raíces que vienen con el SO. | **Siempre** incluir (así sigue funcionando el resto de la PKI mundial). |
| `"user"` | CAs que **el usuario** instaló a mano en el dispositivo. | **Evitar en producción** (ver 4.3). |
| `"@raw/<nombre>"` | Un certificado **embebido en tu APK** en `res/raw/<nombre>.(pem|der)`. | Para aportar **la raíz que falta**. `<nombre>` va **sin extensión**. |

### 4.2. Estrategia elegida en este repo

Para `aplicada.somee.com` confiamos en `system` **más** la raíz embebida **más** los dos intermedios (robustez; ver [§6 Mantenimiento](#6-mantenimiento-y-trade-offs)). El resto del tráfico usa sólo `system`.

`Platforms/Android/Resources/xml/network_security_config.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
	<base-config cleartextTrafficPermitted="true">
		<trust-anchors>
			<certificates src="system" />
		</trust-anchors>
	</base-config>
	<domain-config>
		<domain includeSubdomains="true">aplicada.somee.com</domain>
		<trust-anchors>
			<certificates src="system" />
			<certificates src="@raw/isrg_root_x2" />
			<certificates src="@raw/root_ye" />
			<certificates src="@raw/ye2" />
		</trust-anchors>
	</domain-config>
</network-security-config>
```

Y `res/raw/` contiene **solo** estos tres archivos (PEM, nombres en minúscula):

```
raw/
├── isrg_root_x2.pem
├── root_ye.pem
└── ye2.pem
```

> **Mínimo imprescindible:** con embeber **solo** `isrg_root_x2.pem` (la raíz) ya alcanza, porque el servidor envía los intermedios. Los intermedios se agregan por robustez. El trade-off está en la [sección 6](#6-mantenimiento-y-trade-offs).

### 4.3. Por qué NO usar `src="user"` en producción

`user` confía en **cualquier CA que el usuario haya instalado** en *Ajustes → Seguridad → Credenciales*. Eso abre la puerta a ataques **MITM**: una app de "VPN/control parental", una red corporativa o un atacante con acceso físico pueden instalar una CA propia y **interceptar/descifrar** tu tráfico HTTPS sin que la app lo note. Por eso:

- En producción, los `trust-anchors` deberían ser `system` + (si hace falta) tus `@raw` controlados.
- **El config actual del repo todavía incluye `user`** en algunos bloques (legado del setup original). Como mejora, conviene **quitar `user`** y dejar la versión de §4.2.

### 4.4. Enganche en el `AndroidManifest.xml`

El `<application>` debe apuntar al config:

```xml
<application
    android:usesCleartextTraffic="true"
    android:networkSecurityConfig="@xml/network_security_config" >
    ...
</application>
```

> **Importante — precedencia:** cuando existe `android:networkSecurityConfig`, **ese archivo manda** sobre el atributo `android:usesCleartextTraffic`. Es decir, el `cleartextTrafficPermitted` definido dentro del XML (en `base-config` / `domain-config`) es el que realmente decide si se permite HTTP en claro; el atributo del manifest queda como respaldo/compatibilidad.

Ubicación real: `Platforms/Android/AndroidManifest.xml` de `Ejemplo_Maui_Hibrida`.

---

## 5. Procedimiento paso a paso (reproducible)

Sirve para **cualquier dominio**: reemplazá `aplicada.somee.com` por el tuyo. Comandos detallados con salidas en el [Anexo A](./Anexo-A-Comandos.md).

### Paso 1 — Inspeccionar la cadena con OpenSSL

```bash
openssl s_client -connect aplicada.somee.com:443 -servername aplicada.somee.com -showcerts
```

Leé los pares `s:` (subject) e `i:` (issuer) de cada certificado y **mapeá hoja → intermedios → emisor de la raíz**:

```
 0 s:CN=aplicada.somee.com          i:C=US, O=Let's Encrypt, CN=YE2
 1 s:C=US, O=Let's Encrypt, CN=YE2  i:C=US, O=ISRG, CN=Root YE
 2 s:C=US, O=ISRG, CN=Root YE       i:C=US, O=ISRG, CN=ISRG Root X2
```

El `i:` del **último** certificado que envía el server (`CN=ISRG Root X2`) es **la raíz que falta**.

### Paso 2 (alternativa Windows) — PowerShell + `X509Chain`

En Windows podés inspeccionar la cadena con `System.Net.Security.SslStream` + `X509Chain`. El repo trae el script:

```
Utilities\download_sectigo.ps1
```

> El nombre `download_sectigo.ps1` quedó del **error original** (se creía que la CA era Sectigo). Ya **no** descarga Sectigo: descarga **solo las CA necesarias** (las de Let's Encrypt/ISRG) y debe generarlas con **nombres válidos** (ver Paso 4 y §7). Conviene renombrarlo a algo como `inspect_chain.ps1`.

Uso:

```powershell
.\Utilities\download_sectigo.ps1 -HostUrl "aplicada.somee.com"
```

Como Windows **sí** confía en `ISRG Root X2`, el script reporta `Cadena válida desde Windows` → eso **confirma que el problema es exclusivo de Android**.

### Paso 3 — Identificar el trust anchor

El **trust anchor** es:

- el certificado **auto-firmado** del tope (`issuer == subject`), o, equivalentemente,
- el **emisor (`i:`) del último** certificado que envía el servidor.

Acá: **`CN=ISRG Root X2`**. Embebé **la raíz** (obligatorio) y, opcionalmente, **los intermedios** (robustez).

### Paso 4 — Guardar en `res/raw` con nombre válido

Exportá cada certificado a **PEM** y guardalo en `Platforms/Android/Resources/raw/` con **nombre en minúscula** que cumpla `[a-z0-9_]`:

```
raw/isrg_root_x2.pem
raw/root_ye.pem
raw/ye2.pem
```

Referencialos en el XML **sin extensión**: `@raw/isrg_root_x2`, `@raw/root_ye`, `@raw/ye2`. (Reglas estrictas de `res/raw` en la [sección 7](#7-reglas-críticas-de-resraw-en-android).)

### Paso 5 — Recompilar, desplegar y verificar

```bash
dotnet build -t:Run -f net10.0-android   # o desde Visual Studio
```

Desplegá en el dispositivo y abrí de nuevo la URL en el WebView. Mirá el log con `adb logcat` filtrando `cr_X509Util`: **ya no debe aparecer** `Trust anchor ... not found` y la página carga por HTTPS. (Comandos `adb` en el [Anexo A](./Anexo-A-Comandos.md).)

---

## 6. Mantenimiento y trade-offs

**Let's Encrypt rota sus intermedios** periódicamente. Los CN tipo `YE2` / `Root YE` **pueden cambiar** con el tiempo (Let's Encrypt usa una nomenclatura rotativa para sus CAs intermedias). En cambio, la **raíz `ISRG Root X2` es de larga duración: válida hasta 2040**.

Esto define dos estrategias:

| Estrategia | Qué embebés | Pros | Contras |
|---|---|---|---|
| **Solo la raíz** (recomendada para mantenimiento) | `isrg_root_x2.pem` | Mínimo mantenimiento (la raíz dura hasta 2040). El server ya envía los intermedios. | Depende de que el server **siga enviando** intermedios correctos. |
| **Raíz + intermedios** (la elegida acá, robustez) | `isrg_root_x2.pem`, `root_ye.pem`, `ye2.pem` | Funciona aunque el server mande una cadena incompleta. | **Cuando Let's Encrypt rote los intermedios, romperá** y habrá que re-correr el script y actualizar las referencias `@raw` del XML. |

**Recordatorio de mantenimiento:** si embebés intermedios y un día el sitio vuelve a fallar (o cambia el `CN` de los intermedios), **re-ejecutá el script de inspección**, regenerá los `.pem` y actualizá las líneas `@raw` del `network_security_config.xml`. La raíz casi nunca cambia.

---

## 7. Reglas críticas de `res/raw` en Android

`res/raw/` es estricto. Estas reglas **rompieron el build original**:

1. **Solo `[a-z0-9_]` (minúsculas).** Una mayúscula → error de `aapt`/`aapt2`: *"invalid resource name"*. El script viejo generaba `cert_3_ISRG_Root_X2` (¡con mayúsculas!) → build roto. Por eso los archivos finales son `isrg_root_x2.pem`, etc.
2. **No puede haber dos archivos con el mismo nombre base y distinta extensión.** `foo.cer` y `foo.pem` mapean **ambos** a `R.raw.foo` → error *"Duplicate resources"*. El script viejo generaba **`.cer` Y `.pem`** por cada certificado → build roto. **Elegir un solo formato** (acá, `.pem`).
3. **En `<certificates src="@raw/NOMBRE"/>`, `NOMBRE` va SIN extensión.** El recurso `R.raw.isrg_root_x2` corresponde al archivo `isrg_root_x2.pem`.
4. **Formatos aceptados:** PEM (texto Base64 con `-----BEGIN CERTIFICATE-----`) o DER (binario). Acá usamos `.pem`.

---

## 8. Errores comunes / diagnóstico del setup original

Lo que estaba **mal** en la primera versión (sirve como checklist de qué NO hacer):

1. **Dominio equivocado en `<domain>`.** Apuntaba a `gobdigital.com.ar` en vez del real `aplicada.somee.com` → el ancla **nunca se aplicaba** al sitio que fallaba.
2. **Referencia a un `@raw` inexistente:** `@raw/cert_2_sectigo_public_server_authenticatio` (archivo que no existe) → la referencia no resuelve.
3. **Suposición equivocada de la CA:** se creyó que era **Sectigo**; la CA real es **Let's Encrypt / ISRG**. De ahí el nombre engañoso `download_sectigo.ps1`. **Lección:** primero **inspeccioná la cadena real** (Paso 1) y recién después decidís qué raíz embeber.
4. **Archivos de `raw/` inválidos:** con **mayúsculas** (`cert_3_ISRG_Root_X2`) y con `.cer` **+** `.pem` duplicados → **build de Android roto** por las reglas de la [sección 7](#7-reglas-críticas-de-resraw-en-android).
5. **`src="user"` en los trust-anchors:** innecesario y **riesgoso** (MITM); ver [§4.3](#43-por-qué-no-usar-srcuser-en-producción).

### Checklist de verificación final

- [ ] El `<domain>` es **exactamente** el host que falla (`aplicada.somee.com`).
- [ ] Cada `@raw/<nombre>` referenciado **existe** en `res/raw/<nombre>.pem`.
- [ ] Los nombres en `raw/` son **minúscula** y `[a-z0-9_]`, **sin duplicados** de base.
- [ ] La raíz embebida es la correcta (`isrg_root_x2.pem` = `CN=ISRG Root X2`).
- [ ] El manifest tiene `android:networkSecurityConfig="@xml/network_security_config"`.
- [ ] (Producción) **sin** `src="user"`.
- [ ] Recompilado, desplegado y verificado en el device (sin `Trust anchor ... not found` en `cr_X509Util`).

---

## 9. Referencias

- Android — *Network security configuration*: https://developer.android.com/privacy-and-security/security-config
- Let's Encrypt — *Chain of Trust* (raíces e intermedios, rotación): https://letsencrypt.org/certificates/
- Anexos de esta guía: [`Anexo-A-Comandos.md`](./Anexo-A-Comandos.md) · [`Anexo-B-Glosario.md`](./Anexo-B-Glosario.md)
