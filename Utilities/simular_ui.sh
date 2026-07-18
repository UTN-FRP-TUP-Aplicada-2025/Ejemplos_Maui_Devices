#!/bin/bash
# ─────────────────────────────────────────────────────────────────────────────
# simular_ui.sh — Ejecuta pruebas UI automatizadas ("dedo virtual" con Maestro)
# sobre el simulador iOS y graba un VIDEO real del recorrido con simctl recordVideo.
#
# Reemplaza el enfoque pasivo (screenshots -> GIF) de simular.sh.
# NO modifica la app. Deja intacto simular.sh (usado por otros pipelines).
#
# Variables esperadas del entorno (las provee el workflow):
#   APP_PATH          ruta al .app compilado para el simulador
#   PACKAGE_NAME      bundle id (com.ejemplos.devices.integrada.hibrida)
#   DEVICE_SIMULATOR  nombre del simulador (iPhone 17 Pro Max)
#   PROJECT_NAME      nombre del binario dentro del .app (Ejemplo_Maui_Hibrida)
#
# Artefactos que deja en el CWD:
#   recorrido.mp4     video del recorrido automatizado
#   debug_logs/       logs de dispositivo/app/sistema + crash reports
#   app_stream_full.txt
# ─────────────────────────────────────────────────────────────────────────────
set -e

echo "Ruta aplicacion: ${APP_PATH}"
echo "Package name:    ${PACKAGE_NAME}"
echo "Device:          ${DEVICE_SIMULATOR}"

# Resuelve la ubicacion del flujo Maestro relativo a este script (robusto ante el CWD).
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
FLOW="${SCRIPT_DIR}/end2end/${PACKAGE_NAME}.yaml"
VIDEO_OUT="recorrido.mp4"

mkdir -p debug_logs

# --- Timeout portable en macOS (alternativa a `timeout`) ---
run_with_timeout() {
    local timeout=$1; shift
    perl -e "alarm $timeout; exec @ARGV" "$@"
}

# ─── Simulador: obtener UUID y asegurar booteo ───────────────────────────────
echo ""
echo "Obtener UUID del simulador"
UUID=$(xcrun simctl list devices "${DEVICE_SIMULATOR}" available 2>/dev/null \
       | grep -m 1 -oE '[0-9A-F]{8}-([0-9A-F]{4}-){3}[0-9A-F]{12}' || true)

if [ -z "$UUID" ]; then
    echo "ERROR: No se encontro el simulador '${DEVICE_SIMULATOR}'"
    echo "Simuladores disponibles:"
    xcrun simctl list devices available
    exit 1
fi
echo "Usando simulador: $UUID"

# El simulador puede llegar "precalentado" (booteado en segundo plano durante el
# build, ver el step "Verificando simulador instalado" del workflow). Si ya está
# Booted, seguimos sin esperar. Si no, lo booteamos LEVANTANDO Simulator.app: en
# runners headless con Xcode recién instalado, el boot por 'simctl' se cuelga en
# "Waiting on BackBoard" porque el stack gráfico (BackBoard/SpringBoard) no arranca
# solo; abrir la GUI lo fuerza. Los runners macOS de GitHub corren una sesión GUI
# (Aqua), así que es 100% automatizado (no requiere interacción; Maestro sigue
# manejando la app por simctl). Todo con TIMEOUT + un reintento limpio para no
# colgarnos y quemar el timeout del job.
echo "Verificando estado del simulador..."
ESTADO=$(xcrun simctl list devices | grep "$UUID" | grep -oE '\(Booted\)|\(Booting\)|\(Shutdown\)' | head -1 || true)
echo "Estado actual: ${ESTADO:-desconocido}"

# Simulator.app del Xcode activo (no el preinstalado): lo resuelve xcode-select.
SIMULATOR_APP="$(xcode-select -p)/Applications/Simulator.app"
abrir_gui() {
    # Abre la GUI apuntando al device activo → fuerza el arranque de BackBoard.
    open -a "$SIMULATOR_APP" --args -CurrentDeviceUDID "$UUID" 2>/dev/null \
        || open -a Simulator --args -CurrentDeviceUDID "$UUID" 2>/dev/null || true
}

if [ "$ESTADO" = "(Booted)" ]; then
    echo "Simulador ya precalentado (Booted) — no se espera boot."
else
    echo "Asegurando booteo (GUI + timeout y reintento limpio)..."
    xcrun simctl shutdown all 2>/dev/null || true   # slate limpio: descarta estados zombi
    abrir_gui
    sleep 5
    if ! run_with_timeout 240 xcrun simctl bootstatus "$UUID" -b; then
        echo "AVISO: boot colgado (>240s, tipico 'Waiting on BackBoard'). Reinicio limpio..."
        xcrun simctl shutdown "$UUID" 2>/dev/null || true
        sleep 3
        xcrun simctl erase "$UUID" 2>/dev/null || true
        sleep 3
        abrir_gui                                    # reabrir la GUI sobre el device reseteado
        sleep 5
        if ! run_with_timeout 300 xcrun simctl bootstatus "$UUID" -b; then
            echo "ERROR: el simulador no bootea tras reintento."
            echo "=== CoreSimulator.log (ultimas 120 lineas) para diagnostico ==="
            tail -n 120 "$HOME/Library/Logs/CoreSimulator/CoreSimulator.log" 2>/dev/null || true
            exit 1
        fi
    fi
fi
echo "Esperando SpringBoard..."
sleep 5

# ─── Preparacion e instalacion de la app ─────────────────────────────────────
echo ""
echo "Preparacion de la APP"
if [ ! -d "${APP_PATH}" ]; then
    echo "ERROR: No se encuentra la app en: ${APP_PATH}"
    exit 1
fi

echo "Limpiando atributos y cuarentena..."
find "${APP_PATH}" -name ".DS_Store" -delete 2>/dev/null || true
chmod -R 755 "${APP_PATH}"
xattr -rc "${APP_PATH}" 2>/dev/null || true
chmod +x "${APP_PATH}/${PROJECT_NAME}" 2>/dev/null || true
sudo xattr -rd com.apple.quarantine "${APP_PATH}" 2>/dev/null || true

echo "Desinstalando version previa (si existe)..."
xcrun simctl uninstall "$UUID" "${PACKAGE_NAME}" 2>/dev/null || true
sleep 3

echo "Instalando app..."
if xcrun simctl install "$UUID" "${APP_PATH}"; then
    echo "App instalada"
    xcrun simctl privacy "$UUID" grant notifications "${PACKAGE_NAME}" 2>/dev/null || true
    xcrun simctl spawn "$UUID" notifyutil -p com.apple.SpringBoard.icons-changed 2>/dev/null || true
else
    echo "ERROR: Fallo la instalacion"
    exit 1
fi
sleep 5

echo "Verificando registro de la app..."
if xcrun simctl listapps "$UUID" | grep -q "${PACKAGE_NAME}"; then
    echo "Confirmado: la app esta registrada."
else
    echo "ERROR: la app se instalo pero NO aparece registrada (posible problema de firma)."
    exit 1
fi

# ─── Captura de logs en background ───────────────────────────────────────────
echo ""
echo "Iniciando captura de logs (stream)..."
LOG_FILE="app_stream_full.txt"
xcrun simctl spawn "$UUID" log stream --level info > "$LOG_FILE" 2>&1 &
LOG_PID=$!
echo "Log stream PID: $LOG_PID"
sleep 3

# ─── GRABACION DE VIDEO (nativa) en background ───────────────────────────────
# Se arranca ANTES del recorrido para capturar toda la sesion (splash -> flujo).
# IMPORTANTE: se cierra con SIGINT (kill -s INT), nunca kill -9, para que el
# encoder finalice el contenedor MP4 (atomo moov) y el video sea reproducible.
echo ""
echo "Iniciando grabacion de video: $VIDEO_OUT"
xcrun simctl io "$UUID" recordVideo --codec h264 --mask black --force "$VIDEO_OUT" &
REC_PID=$!
echo "recordVideo PID: $REC_PID"
sleep 2

# ─── DEDO VIRTUAL: Maestro ejecuta el recorrido ──────────────────────────────
echo ""
echo "Ejecutando recorrido con Maestro: $FLOW"
MAESTRO_STATUS=0
if command -v maestro >/dev/null 2>&1; then
    maestro --device "$UUID" test -e APP_ID="${PACKAGE_NAME}" "$FLOW" || MAESTRO_STATUS=$?
    echo "Maestro finalizo con status: $MAESTRO_STATUS"
else
    echo "AVISO: 'maestro' no esta instalado; se graba el arranque de la app sin interaccion."
    # Fallback: al menos lanzar la app para que el video no quede vacio.
    xcrun simctl launch "$UUID" "${PACKAGE_NAME}" || true
    sleep 15
    MAESTRO_STATUS=127
fi

# ─── Detener la grabacion de forma limpia ────────────────────────────────────
echo ""
echo "Deteniendo grabacion de video (SIGINT)..."
kill -s INT "$REC_PID" 2>/dev/null || true
wait "$REC_PID" 2>/dev/null || true

if [ -f "$VIDEO_OUT" ]; then
    echo "Video generado: $(ls -lh "$VIDEO_OUT" | awk '{print $5}')"
else
    echo "AVISO: no se genero el video."
fi

# ─── Detener captura de logs ─────────────────────────────────────────────────
echo ""
echo "Deteniendo log stream..."
kill -TERM "$LOG_PID" 2>/dev/null || true
sleep 2
kill -KILL "$LOG_PID" 2>/dev/null || true
wait "$LOG_PID" 2>/dev/null || true
[ -f "$LOG_FILE" ] && cp "$LOG_FILE" debug_logs/ || true

# ─── Logs del sistema / app / crash reports ──────────────────────────────────
echo ""
echo "Capturando logs del dispositivo..."
run_with_timeout 30 xcrun simctl spawn "$UUID" log show --last 5m > debug_logs/device_full_log.txt 2>&1 || echo "Timeout/error en device log"
run_with_timeout 30 xcrun simctl spawn "$UUID" log show --last 5m --predicate "senderIdentifier == '${PACKAGE_NAME}'" > debug_logs/app_specific_log.txt 2>&1 || echo "Timeout/error en app log"
run_with_timeout 30 xcrun simctl spawn "$UUID" log show --last 5m --predicate "process == '${PROJECT_NAME}'" > debug_logs/app_process_log.txt 2>&1 || echo "Timeout/error en process log"

echo ""
echo "Buscando crash reports..."
CRASH_DIR="$HOME/Library/Logs/DiagnosticReports"
if [ -d "$CRASH_DIR" ]; then
    find "$CRASH_DIR" -name "*${PROJECT_NAME}*" -mtime -1 -exec cp {} debug_logs/ \; 2>/dev/null || true
    find "$CRASH_DIR" \( -name "*.ips" -o -name "*.crash" \) -mtime -1 -exec cp {} debug_logs/ \; 2>/dev/null || true
fi

# ─── Resumen ─────────────────────────────────────────────────────────────────
echo ""
echo "===== Resumen ====="
[ -f "$VIDEO_OUT" ] && echo "Video:  $VIDEO_OUT ($(ls -lh "$VIDEO_OUT" | awk '{print $5}'))" || echo "Video:  NO generado"
echo "Logs:   $(ls debug_logs/*.txt 2>/dev/null | wc -l | tr -d ' ') archivos en debug_logs/"
echo "Maestro status: $MAESTRO_STATUS"
echo "Script completado"

# No propagamos el fallo de Maestro para no romper el pipeline (el step ya usa
# continue-on-error). El video/logs quedan como evidencia igualmente.
exit 0
