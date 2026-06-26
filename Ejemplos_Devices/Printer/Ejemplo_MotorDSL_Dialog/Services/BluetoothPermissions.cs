namespace Ejemplo_MotorDSL_Dialog.Services;

/// <summary>
/// Permiso awaitable de Bluetooth para el patrón de MAUI Essentials
/// (MAUI no trae <c>Permissions.Bluetooth</c>). En Android 12+ (API 31)
/// pide BLUETOOTH_SCAN + BLUETOOTH_CONNECT; en versiones anteriores el
/// escaneo BT requiere ACCESS_FINE_LOCATION.
/// </summary>
public class BluetoothPermissions : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        OperatingSystem.IsAndroidVersionAtLeast(31)
            ? new[]
              {
                  (Android.Manifest.Permission.BluetoothScan, true),
                  (Android.Manifest.Permission.BluetoothConnect, true),
              }
            : new[] { (Android.Manifest.Permission.AccessFineLocation, true) };
#endif
}
