
using Ejemplo_ThermalPrinter.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Ejemplo_ThermalPrinter.Services;

#if ANDROID
using Android;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Microsoft.Maui.ApplicationModel;
#endif

namespace Ejemplo_ThermalPrinter.Pages;

public partial class MainPage : ContentPage
{
    private readonly IThermalPrinterService _printerService;
    private List<BluetoothDevice> _devices;

    public MainPage(IThermalPrinterService printerService)
    {
        InitializeComponent();
        _printerService = printerService;
        _devices = new List<BluetoothDevice>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Solicitar permisos en Android
#if ANDROID
        await RequestBluetoothPermissions();
#endif
    }

#if ANDROID
    private async Task<bool> RequestBluetoothPermissions()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Permisos Requeridos",
                    "Se necesitan permisos de ubicación para escanear dispositivos Bluetooth.",
                    "OK");
                return false;
            }

            // Para Android 12+ (API 31+)
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                var activity = Platform.CurrentActivity;

                string[] permissions = new[]
                {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothConnect
                };

                foreach (var permission in permissions)
                {
                    if (ContextCompat.CheckSelfPermission(activity, permission) != (int)Android.Content.PM.Permission.Granted)
                    {
                        ActivityCompat.RequestPermissions(activity, permissions, 1);
                        break;
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Error al solicitar permisos: {ex.Message}", "OK");
            return false;
        }
    }
#endif

    private async void OnScanClicked(object sender, EventArgs e)
    {
        try
        {
            ShowMessage("Escaneando dispositivos...");
            ScanButton.IsEnabled = false;

            _devices = await _printerService.ScanDevicesAsync();

            if (_devices.Any())
            {
                DevicePicker.ItemsSource = _devices.Select(d => d.ToString()).ToList();
                DevicePicker.IsEnabled = true;
                ConnectButton.IsEnabled = true;
                ShowMessage($"Se encontraron {_devices.Count} dispositivo(s) emparejado(s)");
            }
            else
            {
                ShowMessage("No se encontraron dispositivos Bluetooth emparejados");
                await DisplayAlertAsync("Información",
                    "No se encontraron dispositivos Bluetooth emparejados. " +
                    "Por favor, empareja tu impresora en la configuración de Bluetooth de Android primero.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            ShowMessage("Error al escanear");
            await DisplayAlert("Error", $"Error al escanear dispositivos: {ex.Message}", "OK");
        }
        finally
        {
            ScanButton.IsEnabled = true;
        }
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        try
        {
            if (DevicePicker.SelectedIndex < 0)
            {
                await DisplayAlert("Información", "Por favor, selecciona un dispositivo", "OK");
                return;
            }

            ShowMessage("Conectando...");
            ConnectButton.IsEnabled = false;

            var selectedDevice = _devices[DevicePicker.SelectedIndex];
            bool success = await _printerService.ConnectAsync(selectedDevice.Address);

            if (success)
            {
                UpdateConnectionStatus(true);
                ShowMessage($"Conectado a {selectedDevice.Name}");
                await DisplayAlert("Éxito", "Conexión establecida correctamente", "OK");
            }
            else
            {
                ShowMessage("Error al conectar");
                await DisplayAlert("Error", "No se pudo establecer la conexión", "OK");
            }
        }
        catch (Exception ex)
        {
            ShowMessage("Error de conexión");
            await DisplayAlert("Error", $"Error al conectar: {ex.Message}", "OK");
        }
        finally
        {
            ConnectButton.IsEnabled = !_printerService.IsConnected;
        }
    }

    private async void OnDisconnectClicked(object sender, EventArgs e)
    {
        try
        {
            ShowMessage("Desconectando...");
            await _printerService.DisconnectAsync();
            UpdateConnectionStatus(false);
            ShowMessage("Desconectado");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al desconectar: {ex.Message}", "OK");
        }
    }

    private async void OnPrintTestClicked(object sender, EventArgs e)
    {
        try
        {
            ShowMessage("Imprimiendo prueba...");

            await _printerService.PrintTextAsync("PRUEBA DE IMPRESIÓN", fontSize: 2, bold: true, centered: true);
            await _printerService.PrintTextAsync("", centered: true);
            await _printerService.PrintTextAsync("Esta es una prueba de", centered: true);
            await _printerService.PrintTextAsync("impresión térmica 58mm", centered: true);
            await _printerService.PrintTextAsync("", centered: true);
            await _printerService.PrintTextAsync($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
            await _printerService.PrintTextAsync($"Hora: {DateTime.Now:HH:mm:ss}");
            await _printerService.FeedLinesAsync(3);

            ShowMessage("Prueba impresa correctamente");
        }
        catch (Exception ex)
        {
            ShowMessage("Error al imprimir");
            await DisplayAlert("Error", $"Error al imprimir: {ex.Message}", "OK");
        }
    }

    private async void OnPrintReceiptClicked(object sender, EventArgs e)
    {
        try
        {
            ShowMessage("Imprimiendo ticket...");

            var items = new List<(string name, int qty, decimal price)>
            {
                ("Producto 1", 2, 15.50m),
                ("Producto 2", 1, 25.00m),
                ("Producto 3", 3, 8.75m)
            };

            decimal subtotal = 67.25m;
            decimal tax = 10.76m;
            decimal total = 78.01m;

            await _printerService.PrintReceiptAsync(
                storeName: "MI NEGOCIO",
                items: items,
                subtotal: subtotal,
                tax: tax,
                total: total,
                footer: "Gracias por su compra!"
            );

            ShowMessage("Ticket impreso correctamente");
        }
        catch (Exception ex)
        {
            ShowMessage("Error al imprimir");
            await DisplayAlert("Error", $"Error al imprimir ticket: {ex.Message}", "OK");
        }
    }

    private async void OnPrintQRClicked(object sender, EventArgs e)
    {
        try
        {
            ShowMessage("Imprimiendo código QR...");

            await _printerService.PrintTextAsync("Código QR de ejemplo:", centered: true);
            await _printerService.PrintQRCodeAsync("https://www.ejemplo.com", size: 8);
            await _printerService.PrintTextAsync("Escanea para más info", centered: true);
            await _printerService.FeedLinesAsync(3);

            ShowMessage("Código QR impreso correctamente");
        }
        catch (Exception ex)
        {
            ShowMessage("Error al imprimir");
            await DisplayAlert("Error", $"Error al imprimir código QR: {ex.Message}", "OK");
        }
    }

    private async void OnPrintBarcodeClicked(object sender, EventArgs e)
    {
        try
        {
            ShowMessage("Imprimiendo código de barras...");

            await _printerService.PrintTextAsync("Código de barras:", centered: true);
            await _printerService.PrintBarcodeAsync("1234567890128", BarcodeType.EAN13);
            await _printerService.FeedLinesAsync(3);

            ShowMessage("Código de barras impreso correctamente");
        }
        catch (Exception ex)
        {
            ShowMessage("Error al imprimir");
            await DisplayAlert("Error", $"Error al imprimir código de barras: {ex.Message}", "OK");
        }
    }

    private async void OnPrintCustomClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CustomTextEntry.Text))
            {
                await DisplayAlert("Información", "Por favor, ingresa un texto", "OK");
                return;
            }

            ShowMessage("Imprimiendo texto personalizado...");

            int fontSize = FontSizePicker.SelectedIndex + 1;
            bool bold = BoldCheckBox.IsChecked;
            bool centered = CenterCheckBox.IsChecked;

            await _printerService.PrintTextAsync(CustomTextEntry.Text, fontSize, bold, centered);
            await _printerService.FeedLinesAsync(3);

            ShowMessage("Texto impreso correctamente");
        }
        catch (Exception ex)
        {
            ShowMessage("Error al imprimir");
            await DisplayAlert("Error", $"Error al imprimir: {ex.Message}", "OK");
        }
    }

    private void UpdateConnectionStatus(bool connected)
    {
        if (connected)
        {
            StatusLabel.Text = "Conectado";
            StatusLabel.TextColor = Colors.Green;

            DisconnectButton.IsEnabled = true;
            PrintTestButton.IsEnabled = true;
            PrintReceiptButton.IsEnabled = true;
            PrintQRButton.IsEnabled = true;
            PrintBarcodeButton.IsEnabled = true;
            CustomTextEntry.IsEnabled = true;
            FontSizePicker.IsEnabled = true;
            BoldCheckBox.IsEnabled = true;
            CenterCheckBox.IsEnabled = true;
            PrintCustomButton.IsEnabled = true;

            ConnectButton.IsEnabled = false;
        }
        else
        {
            StatusLabel.Text = "Desconectado";
            StatusLabel.TextColor = Colors.Red;

            DisconnectButton.IsEnabled = false;
            PrintTestButton.IsEnabled = false;
            PrintReceiptButton.IsEnabled = false;
            PrintQRButton.IsEnabled = false;
            PrintBarcodeButton.IsEnabled = false;
            CustomTextEntry.IsEnabled = false;
            FontSizePicker.IsEnabled = false;
            BoldCheckBox.IsEnabled = false;
            CenterCheckBox.IsEnabled = false;
            PrintCustomButton.IsEnabled = false;

            ConnectButton.IsEnabled = DevicePicker.SelectedIndex >= 0;
        }
    }

    private void ShowMessage(string message)
    {
        MessageLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
    }
}