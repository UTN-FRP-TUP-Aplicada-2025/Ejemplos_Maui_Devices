
## Contenido del Readme.md para el Prompt en copilot para generar documentación 

**Prompt**:

```

### 📘 Documento 1: Manual de Usuario

Debe estar orientado a usuarios finales (no programadores) e incluir como mínimo:

* Descripción general del producto
* Contenido del paquete
* Especificaciones técnicas básicas
* Descripción de LEDs y botones
* Procedimiento de carga de papel
* Encendido y apagado
* Conexión por USB
* Emparejamiento Bluetooth paso a paso
* Impresión de prueba
* Solución de problemas comunes
* Recomendaciones de mantenimiento

Usar lenguaje claro, instructivo y orientado a operación. (No uses texto de estilo parataxico)


### 📗 Documento 2: Manual Técnico del Dispositivo (ESC/POS)
Muchas veces los fabricantes lo llaman: **ESC/POS Programming Guide**

Debe estar orientado a desarrolladores de bajo nivel e incluir:

* Arquitectura del dispositivo
* Interfaces de comunicación disponibles
* Parámetros de comunicación
* Compatibilidad ESC/POS
* Tabla de comandos ESC/POS soportados (en hexadecimal)
* Fuentes internas disponibles
* Code pages soportadas
* Soporte de códigos de barras
* Soporte de QR
* Manejo de buffer
* Estados de la impresora
* Secuencias de inicialización recomendadas
* Limitaciones conocidas

Formato técnico detallado. (No uses texto de estilo parataxico)


### 📙 Documento 3: Guía de Integración con ESCPOS_NET v2.2.1 para .NET MAUI

Debe incluir:

* Objetivo de la guía
* Requisitos previos
* Instalación del paquete ESCPOS_NET versión 2.2.1
* Configuración en proyecto .NET MAUI
* Ejemplo de conexión USB
* Ejemplo de conexión Bluetooth
* Ejemplos completos de impresión:

  * texto simple
  * ticket formateado
  * código de barras
  * QR
* Manejo de errores
* Buenas prácticas
* Problemas comunes y soluciones

Incluir ejemplos de código en C# listos para usar.

**Requisitos generales:**

* Usar formato Markdown
* Estructura profesional
* Títulos jerárquicos claros
* Tablas cuando corresponda
* Español técnico neutro
* No inventar características no estándar; marcar como "por confirmar" si falta información

## 🚀 Extra (muy recomendado)

 Basarse en impresoras térmicas ESC/POS genéricas de 58mm cuando falte información específica del modelo.

 ## resumen de los objetivos
 1. **Manual de Usuario (operativo)**
 2. **Manual Técnico del Dispositivo (hardware + protocolo)**
 3. **Guía de Integración para Desarrolladores (SDK / librerías específicas)**
```
