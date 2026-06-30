# Anexo A — Comandos de inspección y diagnóstico

Comandos para **inspeccionar la cadena TLS**, **identificar la raíz que falta** y **verificar** la solución. Las salidas son **ejemplos** basados en el caso real `aplicada.somee.com` (Moto g42, Android 12/13). Reemplazá el host por el tuyo cuando reutilices la técnica.

> Volver a la guía principal: [`README.md`](./README.md) · Glosario: [`Anexo-B-Glosario.md`](./Anexo-B-Glosario.md)

---

## A.1. OpenSSL — ver la cadena completa

### Comando

```bash
openssl s_client -connect aplicada.somee.com:443 -servername aplicada.somee.com -showcerts
```

- `-connect HOST:443` — abre el socket TLS.
- `-servername HOST` — manda **SNI** (imprescindible en hostings compartidos como somee).
- `-showcerts` — vuelca **todos** los certificados que envía el server (en PEM).

### Salida (recortada) y cómo leerla

Mirá el bloque `Certificate chain`. Cada entrada tiene `s:` (**subject**) e `i:` (**issuer**):

```
Certificate chain
 0 s:CN=aplicada.somee.com
   i:C=US, O=Let's Encrypt, CN=YE2
-----BEGIN CERTIFICATE-----
... (hoja) ...
-----END CERTIFICATE-----
 1 s:C=US, O=Let's Encrypt, CN=YE2
   i:C=US, O=ISRG, CN=Root YE
-----BEGIN CERTIFICATE-----
... (intermedio 1) ...
-----END CERTIFICATE-----
 2 s:C=US, O=ISRG, CN=Root YE
   i:C=US, O=ISRG, CN=ISRG Root X2
-----BEGIN CERTIFICATE-----
... (intermedio 2) ...
-----END CERTIFICATE-----
```

**Cómo mapear la cadena:** el `i:` (issuer) de cada línea debe ser el `s:` (subject) de la siguiente. Se "sube" así:

```
[0] s: aplicada.somee.com   i: YE2          ─┐
[1] s: YE2                  i: Root YE       ─┤  cada issuer = subject del de arriba
[2] s: Root YE              i: ISRG Root X2  ─┘
                                   ▲
                      Este issuer NO aparece como s: de ningún [n]
                      → es la RAÍZ que el server NO envía  =  el trust anchor que falta
```

El server llega hasta `[2]`. El issuer de `[2]` es **`CN=ISRG Root X2`** y **no hay un `[3]`** → esa raíz es **la que tenés que aportar** al cliente.

> En el `s_client` también suele verse, al final, la línea
> `Verify return code: 21 (unable to verify the first certificate)` o `20 (unable to get local issuer certificate)`
> cuando la máquina que corre openssl tampoco tiene la raíz/intermedios. Con la raíz instalada: `Verify return code: 0 (ok)`.

### Extraer un certificado puntual a PEM

Para guardar **solo el intermedio que envía el server** (ej. el `[1]`), podés combinar con `awk`, pero lo más simple para la **raíz** (que el server NO envía) es bajarla del repositorio oficial de la CA o exportarla desde un almacén que sí la tenga (ver A.2). Para los intermedios que sí vienen en el handshake:

```bash
# Vuelca la cadena y separa cada BEGIN/END CERTIFICATE en archivos cert-0, cert-1, ...
openssl s_client -connect aplicada.somee.com:443 -servername aplicada.somee.com -showcerts </dev/null 2>/dev/null \
  | awk 'BEGIN{c=-1} /BEGIN CERTIFICATE/{c++} c>=0{print > ("cert-" c ".pem")} /END CERTIFICATE/{}'
```

> Inspeccionar un `.pem` ya guardado:
> ```bash
> openssl x509 -in ye2.pem -noout -subject -issuer -dates
> ```
> ```
> subject=C=US, O=Let's Encrypt, CN=YE2
> issuer=C=US, O=ISRG, CN=Root YE
> notBefore=...   notAfter=...
> ```

---

## A.2. PowerShell + `X509Chain` (alternativa en Windows)

En Windows conviene usar `System.Net.Security.SslStream` + `X509Chain`, que es lo que hace el script del repo:

```
Utilities\download_sectigo.ps1
```

> **Nota sobre el nombre:** `download_sectigo.ps1` es un resabio del **error original** (se creía que la CA era Sectigo). La CA real es **Let's Encrypt / ISRG**. El script ya no baja Sectigo; **inspecciona la cadena real** y exporta las CA necesarias. Conviene renombrarlo (p.ej. `inspect_chain.ps1`).

### Uso

```powershell
.\Utilities\download_sectigo.ps1 -HostUrl "aplicada.somee.com"
```

El script (resumen de lo que hace, ver código fuente):

1. Abre `TcpClient` + `SslStream` hacia `HOST:443` con un callback que acepta todo (para **poder inspeccionar** aunque la cadena no valide).
2. Toma el `RemoteCertificate` (la hoja) y construye la cadena con `X509Chain` (`RevocationMode = NoCheck`).
3. Imprime cada elemento con `Subject`, `Issuer`, `Expira` y marca el `[!!]` donde haya problema.
4. Exporta cada certificado a archivo.

### Salida (ejemplo)

```
============================================================
  CADENA DE CERTIFICADOS
============================================================

  [0] Servidor (leaf)
      Subject : CN=aplicada.somee.com
      Issuer  : C=US, O=Let's Encrypt, CN=YE2
      Expira  : ...

  [1] Intermedio
      Subject : C=US, O=Let's Encrypt, CN=YE2
      Issuer  : C=US, O=ISRG, CN=Root YE
      Expira  : ...

  [2] Intermedio
      Subject : C=US, O=ISRG, CN=Root YE
      Issuer  : C=US, O=ISRG, CN=ISRG Root X2
      Expira  : ...

  [3] CA Raiz
      Subject : CN=ISRG Root X2, O=Internet Security Research Group, C=US
      Issuer  : CN=ISRG Root X2, O=Internet Security Research Group, C=US   <- AUTO-FIRMADO
      Expira  : 2040-...

--- Errores globales de cadena ---
  [OK] Cadena valida desde Windows (el problema es especifico de Android)
```

> Que **Windows** valide la cadena **confirma el diagnóstico**: el certificado y el server están bien; lo que falla es el **almacén de Android viejo**, al que le falta `ISRG Root X2`.

### Inspección inline mínima (sin script)

Si querés un one-liner para ver subject/issuer de la cadena:

```powershell
$h = "aplicada.somee.com"
$tcp = [System.Net.Sockets.TcpClient]::new($h, 443)
$cb  = [System.Net.Security.RemoteCertificateValidationCallback]{ param($a,$b,$c,$d) $true }
$ssl = [System.Net.Security.SslStream]::new($tcp.GetStream(), $false, $cb)
$ssl.AuthenticateAsClient($h)
$leaf = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($ssl.RemoteCertificate)
$chain = [System.Security.Cryptography.X509Certificates.X509Chain]::new()
$chain.ChainPolicy.RevocationMode = 'NoCheck'
[void]$chain.Build($leaf)
$chain.ChainElements | ForEach-Object {
    "S: $($_.Certificate.Subject)`nI: $($_.Certificate.Issuer)`n"
}
$ssl.Dispose(); $tcp.Dispose()
```

### Reglas que el script debe respetar al exportar (importante)

Para que el build de Android **no se rompa**, los archivos generados en `res/raw` deben:

- usar **nombre en minúscula** `[a-z0-9_]` (no `cert_3_ISRG_Root_X2`, sí `isrg_root_x2`);
- generar **un solo formato** por certificado (no `.cer` **y** `.pem` con el mismo nombre base → "Duplicate resources").

(Detalle de las reglas en [`README.md` §7](./README.md#7-reglas-críticas-de-resraw-en-android).)

---

## A.3. ADB — desplegar y leer el log del WebView

### Ver dispositivos conectados

```bash
adb devices
```
```
List of devices attached
ZY22XXXXXX	device
```
(Si no aparece: activar *Depuración USB* en el celular y aceptar la huella RSA.)

### Filtrar el log del validador de certificados de Chromium

El **Android System WebView** (Chromium) loguea los problemas de TLS bajo el tag `cr_X509Util`:

```bash
adb logcat -s cr_X509Util:*
```

**Antes** de la solución (el error de esta guía):

```
E cr_X509Util: Failed to validate the certificate chain, error:
  java.security.cert.CertPathValidatorException: Trust anchor for certification path not found.
```

**Después** de embeber la raíz: **ya no aparece** esa línea y la página HTTPS carga.

> Para no perderte el mensaje, limpiá el buffer antes de reproducir:
> ```bash
> adb logcat -c                 # limpia el log
> # (abrí la URL en el WebView de la app)
> adb logcat -s cr_X509Util:* CertPathValidator:*
> ```

### Reinstalar/redeplegar el APK (si lo armás a mano)

```bash
adb install -r ./bin/Release/net10.0-android/<paquete>-Signed.apk
adb shell am start -n <applicationId>/crc64....MainActivity   # o abrir desde el launcher
```
(En la práctica, desplegá desde Visual Studio o con `dotnet build -t:Run -f net10.0-android`.)

---

## A.4. Resumen de qué buscás en cada herramienta

| Herramienta | Qué te dice | Señal clave |
|---|---|---|
| `openssl s_client -showcerts` | Qué certificados **envía el server** y su `s:`/`i:` | El `i:` del último cert = la **raíz que falta** |
| PowerShell `X509Chain` | Si **Windows** valida la cadena | `Cadena válida desde Windows` → problema **solo** en Android |
| `adb logcat -s cr_X509Util` | Si el **WebView** acepta la cadena | Desaparece `Trust anchor ... not found` cuando está resuelto |
