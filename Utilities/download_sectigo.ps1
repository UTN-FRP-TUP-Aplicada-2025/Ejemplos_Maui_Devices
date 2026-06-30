# ============================================================
#  download_sectigo.ps1
#  (El nombre dice "sectigo" por historia del repo, pero la CA
#   real de aplicada.somee.com NO es Sectigo: es Let's Encrypt /
#   ISRG. El ancla de confianza es la raiz "ISRG Root X2".)
#
#  OBJETIVO:
#    Conectarse por TLS a un host, obtener la cadena de
#    certificados y descargar SOLO los certificados de CA
#    necesarios (intermedios + raiz; NUNCA la hoja) dentro de la
#    carpeta res/raw de Android, con nombres VALIDOS para
#    recursos Android.
#
#    Este script SOLO descarga los certs. No instala nada via adb.
#
#  NOTAS X.509 / TLS:
#    - Para aplicada.somee.com la cadena real es Let's Encrypt /
#      ISRG y el ancla de confianza es la raiz "ISRG Root X2".
#    - Let's Encrypt ROTA sus intermedios periodicamente. Si los
#      intermedios cambian, hay que re-correr este script y
#      actualizar las referencias @raw del archivo
#      network_security_config.xml.
#
#  Uso:
#    .\Utilities\download_sectigo.ps1 -HostUrl "aplicada.somee.com"
#
#  Windows PowerShell 5.1 compatible (sin && / || / ternario).
# ============================================================
param(
    [string]$HostUrl = "aplicada.somee.com",
    [int]   $Port    = 443
)

# ------------------------------------------------------------
#  Saneo de nombre para recurso Android (res/raw).
#  Reglas: solo [a-z0-9_], todo minusculas.
#    ToLower -> no [a-z0-9] => '_' -> colapsar '_' repetidos ->
#    quitar '_' al inicio/fin. Si empieza con digito => prefijar 'ca_'.
# ------------------------------------------------------------
function Get-AndroidResName {
    param([string]$Name)

    $n = $Name.ToLowerInvariant()
    $n = ($n -replace '[^a-z0-9]', '_')   # todo lo que no sea [a-z0-9] -> _
    $n = ($n -replace '_+', '_')          # colapsar _ repetidos
    $n = ($n -replace '^_+', '')          # quitar _ al inicio
    $n = ($n -replace '_+$', '')          # quitar _ al final

    if ([string]::IsNullOrEmpty($n)) {
        $n = "ca"
    }
    if ($n -match '^[0-9]') {              # si empieza con digito -> prefijar ca_
        $n = "ca_" + $n
    }
    return $n
}

# ------------------------------------------------------------
#  1. Carpeta destino (res/raw) relativa al repo
# ------------------------------------------------------------
$RepoRoot   = Split-Path -Parent $PSScriptRoot
$OUTPUT_DIR = Join-Path $RepoRoot "Ejemplos_Devices\Integrada\Ejemplo_Maui_Hibrida\Platforms\Android\Resources\raw"

Write-Host "[OUT] Carpeta destino: $OUTPUT_DIR" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $OUTPUT_DIR | Out-Null

# ------------------------------------------------------------
#  2. Conectar por TLS y obtener el cert remoto
# ------------------------------------------------------------
Write-Host ""
Write-Host "[SSL] Inspeccionando: $HostUrl`:$Port" -ForegroundColor Cyan

$tcp        = $null
$ssl        = $null
$serverCert = $null

try {
    $tcp = New-Object System.Net.Sockets.TcpClient($HostUrl, $Port)

    # Callback que SIEMPRE retorna $true: queremos inspeccionar la
    # cadena aunque no valide localmente (este es justamente el caso
    # que estamos investigando para Android).
    $callback = [System.Net.Security.RemoteCertificateValidationCallback]{
        param($snd, $cert, $chain, $errors)
        return $true
    }

    $ssl = New-Object System.Net.Security.SslStream($tcp.GetStream(), $false, $callback)
    $ssl.AuthenticateAsClient($HostUrl)

    $serverCert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($ssl.RemoteCertificate)
    Write-Host "[OK] Conexion TLS establecida" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] No se pudo conectar a $HostUrl`:$Port" -ForegroundColor Red
    Write-Host "        $($_.Exception.Message)" -ForegroundColor Red
    if ($null -ne $ssl) { $ssl.Dispose() }
    if ($null -ne $tcp) { $tcp.Dispose() }
    exit 1
}

# ------------------------------------------------------------
#  3. Construir la cadena de certificados
# ------------------------------------------------------------
$chain = New-Object System.Security.Cryptography.X509Certificates.X509Chain
$chain.ChainPolicy.RevocationMode = [System.Security.Cryptography.X509Certificates.X509RevocationMode]::NoCheck

try {
    $chain.Build($serverCert) | Out-Null
}
catch {
    Write-Host "[ERROR] No se pudo construir la cadena: $($_.Exception.Message)" -ForegroundColor Red
    if ($null -ne $ssl) { $ssl.Dispose() }
    if ($null -ne $tcp) { $tcp.Dispose() }
    exit 1
}

$total = $chain.ChainElements.Count

# ------------------------------------------------------------
#  4. Mostrar la cadena
# ------------------------------------------------------------
Write-Host ""
Write-Host "============================================================" -ForegroundColor White
Write-Host "  CADENA DE CERTIFICADOS ($total elementos)" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor White

$i = 0
foreach ($el in $chain.ChainElements) {
    if ($i -eq 0)              { $rol = "hoja (leaf)" }
    elseif ($i -eq $total - 1) { $rol = "raiz (root CA)" }
    else                       { $rol = "intermedio (intermediate CA)" }

    Write-Host ""
    Write-Host "  [$i] $rol" -ForegroundColor Cyan
    Write-Host "      Subject : $($el.Certificate.Subject)"
    Write-Host "      Issuer  : $($el.Certificate.Issuer)"
    Write-Host "      NotAfter: $($el.Certificate.NotAfter)"
    $i++
}

# ------------------------------------------------------------
#  5. Exportar SOLO los certificados de CA (todos menos el [0]).
#     La hoja [0] NUNCA es un trust anchor, por eso no se exporta.
#     Formato PEM, extension unica .pem (NO .cer: evita
#     "Duplicate resources" en Android).
# ------------------------------------------------------------
Write-Host ""
Write-Host "[EXPORT] Exportando certificados de CA (.pem) en res/raw..." -ForegroundColor Cyan

if ($total -lt 2) {
    Write-Host "[WARN] La cadena solo tiene la hoja; no hay certificados de CA para exportar." -ForegroundColor Yellow
}

$generatedNames = @()
$k = 0
foreach ($el in $chain.ChainElements) {

    # Saltar la hoja (elemento [0]): no es trust anchor.
    if ($k -eq 0) {
        $k++
        continue
    }

    $simpleName = $el.Certificate.GetNameInfo(
        [System.Security.Cryptography.X509Certificates.X509NameType]::SimpleName, $false)

    $resName  = Get-AndroidResName -Name $simpleName
    $fileName = "$resName.pem"
    $filePath = Join-Path $OUTPUT_DIR $fileName

    # Export DER -> base64 con saltos de linea -> envoltura PEM (ASCII).
    $der = $el.Certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    $b64 = [Convert]::ToBase64String($der, [Base64FormattingOptions]::InsertLineBreaks)
    $pem = "-----BEGIN CERTIFICATE-----`r`n$b64`r`n-----END CERTIFICATE-----"
    Set-Content -Path $filePath -Value $pem -Encoding ASCII

    Write-Host "  [$k] $fileName" -ForegroundColor Green
    Write-Host "       SimpleName: $simpleName" -ForegroundColor Gray
    Write-Host "       Ruta      : $filePath" -ForegroundColor Gray

    $generatedNames += $resName
    $k++
}

# ------------------------------------------------------------
#  6. Bloque para pegar en network_security_config.xml
# ------------------------------------------------------------
Write-Host ""
Write-Host "============================================================" -ForegroundColor White
Write-Host "  PEGAR EN network_security_config.xml" -ForegroundColor White
Write-Host "  (dentro del <trust-anchors> del <domain-config>)" -ForegroundColor White
Write-Host "============================================================" -ForegroundColor White
Write-Host ""

if ($generatedNames.Count -eq 0) {
    Write-Host "  (no se genero ningun certificado de CA)" -ForegroundColor Yellow
}
else {
    foreach ($name in $generatedNames) {
        Write-Host "    <certificates src=`"@raw/$name`" />" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "[NOTA] Let's Encrypt rota sus intermedios. Si la app deja de" -ForegroundColor DarkYellow
Write-Host "       confiar en el host, re-correr este script y actualizar" -ForegroundColor DarkYellow
Write-Host "       las referencias @raw del XML." -ForegroundColor DarkYellow

# ------------------------------------------------------------
#  7. Liberar recursos
# ------------------------------------------------------------
if ($null -ne $ssl) { $ssl.Dispose() }
if ($null -ne $tcp) { $tcp.Dispose() }
