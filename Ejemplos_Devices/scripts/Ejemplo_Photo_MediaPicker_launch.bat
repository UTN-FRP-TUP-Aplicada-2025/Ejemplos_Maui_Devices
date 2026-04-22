@echo off
setlocal EnableDelayedExpansion

:: Ruta al .csproj relativa a la ubicacion de este script
set "SCRIPT_DIR=%~dp0"
set "PROJECT=!SCRIPT_DIR!..\Camera\Ejemplo_Photo_MediaPicker\Ejemplo_Photo_MediaPicker.csproj"
set "FRAMEWORK=net10.0-android"

:: Android SDK platform-tools (adb) - requerido para deploy y launch
:: EnableDelayedExpansion evita que los parentesis de "Program Files (x86)" rompan el parser
set "ADB_DIR=%ProgramFiles(x86)%\Android\android-sdk\platform-tools"
set "ADB_EXE=!ADB_DIR!\adb.exe"

if exist "!ADB_EXE!" (
    set "PATH=!ADB_DIR!;!PATH!"
) else (
    echo  [AVISO] No se encontro adb.exe en !ADB_DIR!
    echo  El despliegue puede fallar si adb no esta en el PATH del sistema.
)

echo.
echo ============================================================
echo  Ejemplo_Maui_GPS  ^|  Build + Deploy (Android^)
echo ============================================================
echo  Proyecto : !PROJECT!
echo  Framework: !FRAMEWORK!
echo.

:: Verifica que haya un dispositivo conectado antes de compilar
"!ADB_EXE!" devices 2>nul | findstr /v "List of" | findstr "device" >nul
if !ERRORLEVEL! neq 0 (
    echo  [ERROR] No se detecto ningun dispositivo Android conectado.
    echo  Conecta el dispositivo y habilita la depuracion USB.
    exit /b 1
)

dotnet build "!PROJECT!" -f !FRAMEWORK! -t:Run

if !ERRORLEVEL! neq 0 (
    echo.
    echo  [ERROR] La compilacion o el despliegue fallaron. Revisa la salida anterior.
    exit /b !ERRORLEVEL!
)

echo.
echo  [OK] Aplicacion desplegada y lanzada en el dispositivo.

endlocal
