
```
 echo "Esperando algo como (Signature=adhoc): "
  codesign -vvv --display "${APP_PATH}"
  cd "/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices"
  chmod ugo+rwx ./Utilities/simular.sh
  ./Utilities/simular.sh
  shell: /bin/bash -e {0}
  env:
    PIPELINE_VERSION: 202606262101_ejemplos
    XCODE_VERSION: 26.0
    XCODE_VERSION_SHORT: 26.0
    XCODE_FILE_INSTALLER: Xcode_26_Universal.xip
    XCODE_GOOGLE_FILE_INSTALLER_ID: 1GoDmCUBOMKM5nnXCXxxnAgoKpSwZvaMP
    DOTNET_VERSION: 10.0.300
    DOTNET_TARGET_VERSION: net10.0
    DOTNET_VERSION_WORKLOAD: 10.0.100
    PACKAGE_NAME: com.ejemplos.devices.qr.dialog
    SOLUTION_FOLDER: Ejemplos_Devices
    PROJECTS_ROOT: QR
    PROJECT_NAME: Ejemplo_LectorQR_Dialog
    PROJECT_FILE: Ejemplo_LectorQR_Dialog.csproj
    RUNTIME_IDENTIFIER_SIMULATOR: iossimulator-x64
    BUILD_CONFIG_SIMULATOR: Release
    DEVICE_SIMULATOR: iPhone 17 Pro Max
    SCRIPT_SIMULATOR: ./Utilities/simular.sh
    DOTNET_WORKLOAD_INSTALL_ADDITIONAL_ARGS: --ios-simulator-runtime=26.0
    MD_APPLE_SDK_ROOT: /Applications/Xcode_26.0.app
    PROJECT_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog
    SOLUTION_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices
    PLIST_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/Platforms/iOS/Info.plist
    APP_VERSION_BUILD: 1.0_1.0
    VERSION_FECHA: 202606270037
    BASE_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/bin/Release/net10.0-ios/iossimulator-x64
    APP_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/bin/Release/net10.0-ios/iossimulator-x64/Ejemplo_LectorQR_Dialog.app
    ZIP_NAME: 202606270037_1.0_1.0_com.ejemplos.devices.qr.dialog.app.zip
    ZIP_PATH: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/bin/Release/net10.0-ios/iossimulator-x64/202606270037_1.0_1.0_com.ejemplos.devices.qr.dialog.app.zip
Esperando algo como (Signature=adhoc): 
Executable=/Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/bin/Release/net10.0-ios/iossimulator-x64/Ejemplo_LectorQR_Dialog.app/Ejemplo_LectorQR_Dialog
Identifier=com.ejemplos.devices.qr.dialog
Format=app bundle with Mach-O thin (x86_64)
CodeDirectory v=20400 size=168311 flags=0x2(adhoc) hashes=5253+3 location=embedded
Hash type=sha256 size=32
CandidateCDHash sha256=59ae6655b546a4a6cbbb6f892ff08bcb5f51fd5a
CandidateCDHashFull sha256=59ae6655b546a4a6cbbb6f892ff08bcb5f51fd5a1d3e8d4afb44a99c25fd0e1e
Hash choices=sha256
CMSDigest=59ae6655b546a4a6cbbb6f892ff08bcb5f51fd5a1d3e8d4afb44a99c25fd0e1e
CMSDigestType=2
CDHash=59ae6655b546a4a6cbbb6f892ff08bcb5f51fd5a
Signature=adhoc
Info.plist entries=31
TeamIdentifier=not set
Sealed Resources version=2 rules=10 files=239
Internal requirements count=0 size=12
Ruta aplicaci�n: /Users/runner/work/Ejemplos_Maui_Devices/Ejemplos_Maui_Devices/Ejemplos_Devices/QR/Ejemplo_LectorQR_Dialog/bin/Release/net10.0-ios/iossimulator-x64/Ejemplo_LectorQR_Dialog.app
Package name: com.ejemplos.devices.qr.dialog
Device simulator: iPhone 17 Pro Max
Configuracion inicial
Obtener UUID del simulador
UUID del simulador: 3352C343-F2F9-4F8C-A9FC-D9E957354FEA
? Usando Simulador: 3352C343-F2F9-4F8C-A9FC-D9E957354FEA

Arranque del simulador
Verificar estado actual
Estado actual: (Shutdown)
Intentar arrancar si no est� booted
Arrancando simulador...
Esperando arranque (m�x 120s)...
  [1/12] Estado: (Booted)
? Simulador arrancado
Esperando SpringBoard...

Preparaci�n de la APP
Limpiando archivos innecesarios...
Firmando componentes...

Instalaci�n 

Desinstalando versi�n previa (si existe)...
Limpieza total de atributos de cuarentena
Instalando app...
? App instalada correctamente
Otorgando permisos de notificaci�n...
An error was encountered processing the command (domain=NSPOSIXErrorDomain, code=1):
Simulator device failed to complete the requested operation.
?? simctl privacy fall� (permisos de macOS). Intentando v�a AppleScript...
Operation not permitted
Underlying error (domain=NSPOSIXErrorDomain, code=1):
	Failed to set access
	Operation not permitted
64:96: execution error: System Events got an error: Can’t get process "Simulator". (-1728)
No se pudo hacer clic autom�tico.
Verificando instalaci�n...-si no verifica, fue fantasma , copio pero no registro la app
Verificando si la app es un fantasma...
? Confirmado: La app est� registrada.

Captura de logs
Iniciar captura de logs en background
Log stream iniciado (PID: 41404)

Lanzamiento de la APP
Error: The action 'RELEASE SIMULADOR. GRABAR VIDEO Y CREAR GIF' has timed out after 30 minutes.
```