# Anexo B — Glosario PKI / TLS

Términos usados en la guía, en orden alfabético. Pensado para consulta rápida; las definiciones largas con ejemplos están en [`README.md` §2](./README.md#2-conceptos-previos-pki--tls-en-5-minutos).

> Volver a la guía principal: [`README.md`](./README.md) · Comandos: [`Anexo-A-Comandos.md`](./Anexo-A-Comandos.md)

---

| Término | Definición |
|---|---|
| **AIA** (*Authority Information Access*) | Extensión de un certificado que indica **dónde descargar** el certificado del emisor (issuer). Algunos navegadores de escritorio lo usan para **completar** intermedios faltantes ("AIA fetching"). **Android NO lo hace por defecto** → si falta una pieza, la cadena queda abierta. |
| **Almacén de confianza** (*trust store*) | Conjunto de **raíces** en las que un cliente confía. En Android: `system` (las del SO) y `user` (las que instaló el usuario). |
| **`base-config`** | Bloque de `network_security_config.xml` con la política **por defecto** para todo el tráfico que no caiga en un `domain-config`. |
| **CA** (*Certificate Authority*, Autoridad de Certificación) | Entidad que **emite y firma** certificados. Las **raíces** son CAs en las que se confía axiomáticamente; los **intermedios** son CAs subordinadas. Ej.: *Let's Encrypt / ISRG*. |
| **Cadena de certificación** (*certificate chain*) | Secuencia hoja → intermedio(s) → raíz, donde **cada certificado está firmado por el siguiente** hacia arriba. Validar = encadenar firmas hasta un **trust anchor** conocido. |
| **`cleartextTrafficPermitted`** | Atributo de `network_security_config.xml` que habilita/deshabilita HTTP **en claro** (no cifrado). Cuando hay `networkSecurityConfig`, **manda** sobre el atributo `android:usesCleartextTraffic` del manifest. |
| **CN** (*Common Name*) | Campo del Subject/Issuer. Históricamente llevaba el nombre de dominio de la hoja; hoy el dominio va en **SAN**, pero el `CN` se sigue usando para nombrar CAs (ej. `CN=ISRG Root X2`). |
| **`cr_X509Util`** | Tag de log del **Android System WebView (Chromium)** donde aparecen los errores de validación TLS. Se filtra con `adb logcat -s cr_X509Util:*`. |
| **DER** | Codificación **binaria** (ASN.1) de un certificado X.509. Extensiones `.der`, `.cer`, `.crt`. Aceptada por `res/raw`. |
| **`domain-config`** | Bloque de `network_security_config.xml` con política **específica** para uno o más dominios. **Tiene prioridad** sobre `base-config`. |
| **ECDSA** | Familia de algoritmos de clave basados en **curvas elípticas**. `ISRG Root X2` es una raíz **ECDSA (2020)**; por ser nueva/ECDSA, falta en almacenes de Android viejos. |
| **Firma digital** | Hash del certificado cifrado con la **clave privada** del emisor. Se verifica con la **clave pública** del emisor; garantiza integridad y origen. |
| **Handshake (TLS)** | Negociación inicial de una conexión TLS donde el server **presenta su hoja + intermedios** (NO la raíz) y se acuerdan claves. |
| **Hoja / leaf** | Certificado del **servidor** (lleva el nombre de dominio en CN/SAN). Es el `[0]` de la cadena. |
| **`includeSubdomains`** | Atributo de `<domain>` que extiende la política a los **subdominios** del dominio indicado. |
| **Intermedio** | CA **subordinada**: la firma una raíz (u otro intermedio) y a su vez firma hojas u otros intermedios. El server **sí** los envía. Let's Encrypt los **rota** periódicamente. |
| **ISRG** (*Internet Security Research Group*) | Organización detrás de **Let's Encrypt**. Sus raíces son `ISRG Root X1` (RSA) e **`ISRG Root X2`** (ECDSA), esta última la del caso de esta guía. |
| **Issuer** (`i:`) | Quién **emitió/firmó** el certificado. En una raíz, `issuer == subject` (**auto-firmado**). |
| **Let's Encrypt** | CA gratuita y automatizada operada por ISRG. Emite las hojas e intermedios de `aplicada.somee.com`. |
| **MITM** (*Man-in-the-Middle*) | Ataque donde alguien **intercepta** la conexión. Confiar en `src="user"` lo facilita: una CA instalada por el usuario/atacante permite descifrar el tráfico. Por eso `user` se **evita en producción**. |
| **`network_security_config.xml`** | Archivo XML (Android 7+/API 24) donde la app declara su **política de confianza TLS** y de tráfico en claro, sin tocar código. Se referencia desde el `<application>` del manifest. |
| **PEM** | Codificación en **texto** (DER en Base64) entre `-----BEGIN CERTIFICATE-----` y `-----END CERTIFICATE-----`. Extensiones `.pem`/`.crt`/`.cer`. Es el formato usado en este repo. |
| **PKI** (*Public Key Infrastructure*) | Infraestructura de CAs, certificados y reglas que permite **confiar** en claves públicas ajenas. |
| **Par de claves** | **Clave privada** (secreta) + **clave pública** (compartible). Lo firmado/cifrado con una se verifica/descifra con la otra. |
| **Raíz / root CA** | Certificado del **tope**, **auto-firmado** (`issuer == subject`). Es el **trust anchor** por excelencia. El server **no la envía**; debe estar en el cliente. Ej.: `CN=ISRG Root X2` (válida hasta **2040**). |
| **`res/raw`** | Carpeta de recursos "crudos" de Android. Aloja los certificados embebidos. Reglas estrictas: nombres `[a-z0-9_]`, sin duplicados de nombre base. Se referencia con `@raw/<nombre>` (sin extensión). |
| **Revocación** | Mecanismo (CRL/OCSP) para invalidar un certificado antes de su vencimiento. En la inspección con `X509Chain` se desactiva (`NoCheck`) para poder ver la cadena aunque no se pueda consultar revocación. |
| **SAN** (*Subject Alternative Name*) | Extensión que lista los **dominios** que cubre una hoja. Hoy es el campo **autoritativo** para el match de hostname (no el CN). |
| **SNI** (*Server Name Indication*) | Extensión TLS que envía el **hostname** en el handshake, para que un server con varios sitios (hosting compartido, como somee) elija el certificado correcto. En openssl: `-servername`. |
| **`src` (de `<certificates>`)** | Fuente de un trust anchor: `"system"` (raíces del SO), `"user"` (instaladas por el usuario — **evitar en producción**), o `"@raw/<nombre>"` (certificado embebido en el APK). |
| **Subject** (`s:`) | A quién **identifica** el certificado (la hoja: el dominio; una CA: su nombre). |
| **TLS** (*Transport Layer Security*) | Protocolo de cifrado/autenticación bajo HTTPS (sucesor de SSL). |
| **Trust anchor** (ancla de confianza) | Certificado en el que el cliente confía **por sí mismo**, sin pedir aval. La validación **debe terminar** en uno. *"Trust anchor for certification path not found"* = la cadena no llegó a ninguno conocido. |
| **`Trust anchor for certification path not found`** | Error de `CertPathValidatorException`: el dispositivo no pudo **cerrar la cadena** contra una raíz conocida. En esta guía: falta `ISRG Root X2` en el almacén de Android. **No** es error de nombre ni de fecha. |
| **`usesCleartextTraffic`** | Atributo del `<application>` del manifest para permitir HTTP en claro. **Subordinado** a `network_security_config.xml` cuando este existe. |
| **WebView / Android System WebView** | Componente que renderiza HTML dentro de una app. En Android es **Chromium**; valida TLS con el almacén del sistema y reporta en `cr_X509Util`. La app MAUI Híbrida lo usa para mostrar el sitio. |
| **X.509** | Estándar de formato de los certificados de clave pública (Subject, Issuer, clave, validez, extensiones, firma). |
| **`X509Chain`** | Clase de .NET que **construye y valida** una cadena de certificados. Usada en el script de inspección en Windows. |
