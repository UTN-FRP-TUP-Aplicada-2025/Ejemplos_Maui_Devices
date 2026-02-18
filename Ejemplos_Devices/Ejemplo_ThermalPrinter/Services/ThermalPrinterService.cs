using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Ejemplo_ThermalPrinter.Services;


#if ANDROID
using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using Java.Util;
using System.IO;
#endif

namespace Ejemplo_ThermalPrinter.Services;

/// <summary>
/// Implementación del servicio de impresión térmica para Android
/// </summary>
public class ThermalPrinterService : IThermalPrinterService
{
    private const int PAPER_WIDTH_58MM = 384; // Puntos para papel de 58mm

#if ANDROID
    private BluetoothSocket _socket;
    private BluetoothAdapter _bluetoothAdapter;
    private Stream _outputStream;
    private EPSON _printer;
#endif

    public bool IsConnected { get; private set; }

    public ThermalPrinterService()
    {
#if ANDROID
        _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        _printer = new EPSON();
#endif
    }

    public async Task<List<BluetoothDevice>> ScanDevicesAsync()
    {
        var devices = new List<BluetoothDevice>();

#if ANDROID
        if (_bluetoothAdapter == null)
        {
            throw new Exception("Bluetooth no está disponible en este dispositivo");
        }

        if (!_bluetoothAdapter.IsEnabled)
        {
            throw new Exception("Bluetooth está desactivado. Por favor actívalo.");
        }

        // Obtener dispositivos emparejados
        var bondedDevices = _bluetoothAdapter.BondedDevices;

        if (bondedDevices != null && bondedDevices.Count > 0)
        {
            foreach (BluetoothDevice device in bondedDevices)
            {
                devices.Add(new BluetoothDevice
                {
                    Name = device.Name ?? "Dispositivo desconocido",
                    Address = device.Address,
                    IsPaired = true
                });
            }
        }
#else
        await Task.CompletedTask;
#endif
        return devices;
    }

    public async Task<bool> ConnectAsync(string deviceAddress)
    {
#if ANDROID
        try
        {
            if (_socket != null && _socket.IsConnected)
            {
                await DisconnectAsync();
            }

            var device = _bluetoothAdapter.GetRemoteDevice(deviceAddress);

            if (device == null)
            {
                throw new Exception("No se pudo encontrar el dispositivo");
            }

            // UUID estándar para SPP (Serial Port Profile)
            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

            _socket = device.CreateRfcommSocketToServiceRecord(uuid);

            await Task.Run(() => _socket.Connect());

            _outputStream = _socket.OutputStream;

            // Inicializar impresora
            await WriteBytes(_printer.Initialize());

            IsConnected = true;
            return true;
        }
        catch (Exception ex)
        {
            IsConnected = false;
            throw new Exception($"Error al conectar: {ex.Message}", ex);
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }

    public async Task DisconnectAsync()
    {
#if ANDROID
        try
        {
            if (_outputStream != null)
            {
                await _outputStream.FlushAsync();
                _outputStream.Dispose();
                _outputStream = null;
            }

            if (_socket != null)
            {
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }

            IsConnected = false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al desconectar: {ex.Message}", ex);
        }
#else
        await Task.CompletedTask;
#endif
    }

    public async Task PrintTextAsync(string text, int fontSize = 1, bool bold = false, bool centered = false)
    {
        EnsureConnected();

#if ANDROID
        var commands = new List<byte[]>();

        // Configurar codificación
        commands.Add(_printer.CodePage(CodePage.PC858_EURO));

        // Alineación
        if (centered)
        {
            commands.Add(_printer.CenterAlign());
        }
        else
        {
            commands.Add(_printer.LeftAlign());
        }

        // Tamaño de fuente
        if (fontSize > 1)
        {
            commands.Add(_printer.SetStyles(PrintStyle.FontB));

            int width = Math.Min(fontSize, 8);
            int height = Math.Min(fontSize, 8);
            commands.Add(_printer.SetScaleFactor((byte)width, (byte)height));
        }

        // Negrita
        if (bold)
        {
            commands.Add(_printer.SetStyles(PrintStyle.Bold));
        }

        // Texto
        commands.Add(_printer.Print(text));
        commands.Add(_printer.PrintLine(""));

        // Reset estilos
        commands.Add(_printer.ResetLineSpacing());
        commands.Add(_printer.SetStyles(PrintStyle.None));
        commands.Add(_printer.LeftAlign());

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
#else
        await Task.CompletedTask;
#endif
    }

    public async Task PrintReceiptAsync(
        string storeName,
        List<(string name, int qty, decimal price)> items,
        decimal subtotal,
        decimal tax,
        decimal total,
        string footer = null)
    {
        EnsureConnected();

#if ANDROID
        var commands = new List<byte[]>();

        // Encabezado
        commands.Add(_printer.Initialize());
        commands.Add(_printer.CenterAlign());
        commands.Add(_printer.SetStyles(PrintStyle.Bold));
        //commands.Add(_printer.seSetScaleFactor(2, 2));
        commands.Add(_printer.Print(storeName));
        commands.Add(_printer.PrintLine(""));
        commands.Add(_printer.SetStyles(PrintStyle.None));
        //commands.Add(_printer.SetScaleFactor(1, 1));
        commands.Add(_printer.PrintLine(""));

        // Fecha y hora
        commands.Add(_printer.LeftAlign());
        commands.Add(_printer.Print($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}"));
        //commands.Add(_printer.PrintLine());
        commands.Add(_printer.Print("--------------------------------"));
        //commands.Add(_printer.PrintLine());

        // Items
        foreach (var item in items)
        {
            decimal itemTotal = item.qty * item.price;

            // Nombre del producto
            commands.Add(_printer.Print(TruncateString(item.name, 32)));
            commands.Add(_printer.PrintLine(""));

            // Cantidad, precio unitario y total
            string line = $"  {item.qty} x ${item.price:F2}";
            string totalStr = $"${itemTotal:F2}";
            int spaces = 32 - line.Length - totalStr.Length;
            line += new string(' ', Math.Max(0, spaces)) + totalStr;

            commands.Add(_printer.Print(line));
            commands.Add(_printer.PrintLine(""));
        }

        // Separador
        commands.Add(_printer.Print("--------------------------------"));
        commands.Add(_printer.PrintLine(""));

        // Subtotal
        commands.Add(_printer.RightAlign());
        commands.Add(_printer.Print($"Subtotal: ${subtotal:F2}"));
        commands.Add(_printer.PrintLine(""));

        // Impuesto
        commands.Add(_printer.Print($"Impuesto: ${tax:F2}"));
        commands.Add(_printer.PrintLine());

        // Total
        commands.Add(_printer.SetStyles(PrintStyle.Bold));
        commands.Add(_printer.SetScaleFactor(2, 2));
        commands.Add(_printer.Print($"TOTAL: ${total:F2}"));
        commands.Add(_printer.PrintLine());
        commands.Add(_printer.SetStyles(PrintStyle.None));
        commands.Add(_printer.SetScaleFactor(1, 1));

        // Pie de página
        if (!string.IsNullOrEmpty(footer))
        {
            commands.Add(_printer.PrintLine(""));
            commands.Add(_printer.CenterAlign());
            commands.Add(_printer.Print(footer));
            commands.Add(_printer.PrintLine(""));
        }

        // Feed y corte
        commands.Add(_printer.FeedLines(3));
        commands.Add(_printer.FullCutAfterFeed(5));

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
#else
        await Task.CompletedTask;
#endif
    }

    public async Task PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.CODE128)
    {
        EnsureConnected();

#if ANDROID
        var commands = new List<byte[]>();

        commands.Add(_printer.CenterAlign());

        // Convertir tipo de código de barras
        TwoDimensionCodeType codeType = barcodeType switch
        {
            BarcodeType.UPC_A => TwoDimensionCodeType.UPCA,
            BarcodeType.UPC_E => TwoDimensionCodeType.UPCE,
            BarcodeType.EAN13 => TwoDimensionCodeType.EAN13,
            BarcodeType.EAN8 => TwoDimensionCodeType.EAN8,
            BarcodeType.CODE39 => TwoDimensionCodeType.CODE39,
            BarcodeType.CODE93 => TwoDimensionCodeType.CODE93,
            BarcodeType.CODE128 => TwoDimensionCodeType.CODE128,
            _ => TwoDimensionCodeType.CODE128
        };

        commands.Add(_printer.Code128(data));
        commands.Add(_printer.PrintLine());
        commands.Add(_printer.Print(data));
        commands.Add(_printer.PrintLine());
        commands.Add(_printer.LeftAlign());

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
#else
        await Task.CompletedTask;
#endif
    }

    public async Task PrintQRCodeAsync(string data, int size = 8)
    {
        EnsureConnected();

#if ANDROID
        var commands = new List<byte[]>();

        commands.Add(_printer.CenterAlign());

        // Limitar tamaño entre 1 y 16
        size = Math.Max(1, Math.Min(16, size));

        commands.Add(_printer.QrCode(data, QrCodeSize.Large));
        commands.Add(_printer.PrintLine());
        commands.Add(_printer.LeftAlign());

        await WriteBytes(ByteSplicer.Combine(commands.ToArray()));
#else
        await Task.CompletedTask;
#endif
    }

    public async Task PrintImageAsync(string imagePath)
    {
        EnsureConnected();

#if ANDROID
        try
        {
            // TODO: Implementar conversión de imagen a formato ESC/POS
            // Esto requiere convertir la imagen a formato raster compatible
            await Task.CompletedTask;
            throw new NotImplementedException("La impresión de imágenes aún no está implementada");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al imprimir imagen: {ex.Message}", ex);
        }
#else
        await Task.CompletedTask;
#endif
    }

    public async Task FeedLinesAsync(int lines = 3)
    {
        EnsureConnected();

#if ANDROID
        await WriteBytes(_printer.FeedLines((byte)lines));
#else
        await Task.CompletedTask;
#endif
    }

    public async Task CutPaperAsync()
    {
        EnsureConnected();

#if ANDROID
        await WriteBytes(_printer.FullCutAfterFeed(5));
#else
        await Task.CompletedTask;
#endif
    }

    #region Métodos Privados

    private void EnsureConnected()
    {
        if (!IsConnected)
        {
            throw new Exception("No hay conexión con la impresora. Conecta primero.");
        }
    }

#if ANDROID
    private async Task WriteBytes(byte[] data)
    {
        try
        {
            if (_outputStream == null)
            {
                throw new Exception("Stream de salida no disponible");
            }

            await _outputStream.WriteAsync(data, 0, data.Length);
            await _outputStream.FlushAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al escribir en la impresora: {ex.Message}", ex);
        }
    }
#endif

    private string TruncateString(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text.Length <= maxLength ? text : text.Substring(0, maxLength - 3) + "...";
    }

    #endregion
}
