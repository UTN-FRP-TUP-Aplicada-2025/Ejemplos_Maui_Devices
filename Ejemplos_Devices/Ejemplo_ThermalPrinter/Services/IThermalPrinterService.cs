using System;
using System.Collections.Generic;
using System.Text;

namespace Ejemplo_ThermalPrinter.Services;

using System.Collections.Generic;
using System.Threading.Tasks;


/// <summary>
/// Interface para el servicio de impresión térmica por Bluetooth
/// </summary>
public interface IThermalPrinterService
{
    /// <summary>
    /// Indica si hay una impresora conectada
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Escanea dispositivos Bluetooth disponibles
    /// </summary>
    /// <returns>Lista de dispositivos encontrados con nombre y dirección MAC</returns>
    Task<List<BluetoothDevice>> ScanDevicesAsync();

    /// <summary>
    /// Conecta con un dispositivo Bluetooth específico
    /// </summary>
    /// <param name="deviceAddress">Dirección MAC del dispositivo</param>
    /// <returns>True si la conexión fue exitosa</returns>
    Task<bool> ConnectAsync(string deviceAddress);

    /// <summary>
    /// Desconecta del dispositivo actual
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Imprime texto simple
    /// </summary>
    /// <param name="text">Texto a imprimir</param>
    /// <param name="fontSize">Tamaño de fuente (1-8)</param>
    /// <param name="bold">Negrita</param>
    /// <param name="centered">Centrado</param>
    Task PrintTextAsync(string text, int fontSize = 1, bool bold = false, bool centered = false);

    /// <summary>
    /// Imprime un ticket de venta completo
    /// </summary>
    Task PrintReceiptAsync(
        string storeName,
        List<(string name, int qty, decimal price)> items,
        decimal subtotal,
        decimal tax,
        decimal total,
        string footer = null);

    /// <summary>
    /// Imprime un código de barras
    /// </summary>
    /// <param name="data">Datos del código de barras</param>
    /// <param name="barcodeType">Tipo de código de barras</param>
    Task PrintBarcodeAsync(string data, BarcodeType barcodeType = BarcodeType.CODE128);

    /// <summary>
    /// Imprime un código QR
    /// </summary>
    /// <param name="data">Datos del código QR</param>
    /// <param name="size">Tamaño del QR (1-16)</param>
    Task PrintQRCodeAsync(string data, int size = 8);

    /// <summary>
    /// Imprime una imagen
    /// </summary>
    /// <param name="imagePath">Ruta de la imagen</param>
    Task PrintImageAsync(string imagePath);

    /// <summary>
    /// Avanza el papel
    /// </summary>
    /// <param name="lines">Número de líneas a avanzar</param>
    Task FeedLinesAsync(int lines = 3);

    /// <summary>
    /// Corta el papel (si la impresora tiene cortador automático)
    /// </summary>
    Task CutPaperAsync();
}

/// <summary>
/// Representa un dispositivo Bluetooth
/// </summary>
public class BluetoothDevice
{
    public string Name { get; set; }
    public string Address { get; set; }
    public bool IsPaired { get; set; }

    public override string ToString() => $"{Name} ({Address})";
}

/// <summary>
/// Tipos de códigos de barras soportados
/// </summary>
public enum BarcodeType
{
    UPC_A,
    UPC_E,
    EAN13,
    EAN8,
    CODE39,
    ITF,
    CODABAR,
    CODE93,
    CODE128
}
