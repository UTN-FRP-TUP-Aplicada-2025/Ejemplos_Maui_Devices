
# Ejemplos de uso de dispositivos desde MAUI .NET


### 📄[Thermal Printer BlueTooth](Ejemplos_Devices/Printer/Ejemplo_Docs_Printer/Readme.md)

Ejemplo MAUI .NET y Documentación de la Impresora térmica 58H6

<img src="Ejemplos_Devices/Printer/Ejemplo_Docs_Printer/Pruebas_realizadas/ejemplo_print_thermal.gif" width="200">

### 📄[Lectura de QR](Ejemplos_Devices/QR/Ejemplo_Docs_QR/Readme.md)
En construcción ...

### 📄[Camara](Ejemplos_Devices/Camera/Ejemplo_Docs_Photo/Readme.md)
En construcción ...

### 📄[Red y Conectividad](Ejemplos_Devices/Red/Ejemplo_Docs_Red/Readme.md)
En construcción ...

### 📄[GPS](Ejemplos_Devices/GPS/Ejemplo_Docs_GPS/Readme.md)
En construcción ...

### 📄[Maps](Ejemplos_Devices/Maps/Ejemplo_Docs_Maps/Readme.md)
En construcción ...

## Delegación del merge al agente

- **Fecha:** 2026-11-02.
- **Quién lo decidió:** Fernando Filipuzzi, responsable del producto (PO).
- **Decisión, literal:** «segui la conformacion de PR automatica» y «aplica este conocimiento para conformar los PR automatico siempre : `/IA/IA.Knowledge.Base/Knowledges/Knowledge-Conformacion-Pull-Request-Automatico.md`», en respuesta a la propuesta de preparar el primer PR de este repositorio (plan de runners propios para los ejemplos de APLICADA).
- **Conocimiento aplicado:** `IA/IA.Knowledge.Base/Knowledges/Knowledge-Conformacion-Pull-Request-Automatico.md`, versión 2.0 (2026-10-24), SHA-256 `74ffd17b1283204e21c1882fb9da652626292036cf8f022e9b9ca424f18b9286`. Hereda de `Knowledge-Conformacion-Pull-Request-Manual.md`, que rige si esta delegación no se encuentra.
- **Parámetros de la guarda (§3.3):** techo de espera de 30 minutos; intervalo de relectura de 30 segundos.
- **Guarda:** G1 a G4. La cabeza tiene que ser la auditada y `mergeable_state` tiene que dar `clean`. Para esa cabeza tiene que haber al menos una corrida de controles, y todas en éxito. El merge se hace con commit de merge atado al SHA, nunca `squash`. La rama remota se borra sólo si la respuesta dice `merged: true`.
- **Control que alimenta G3:** el workflow `Verificar` (`.github/workflows/verificar.yml`), que corre en `pull_request` y en `push` a `main`.
- **Excepción:** si hay un cuestionamiento particular sobre la unidad, se para y se hace manual. Las reservas de §3.4 del conocimiento también devuelven el merge al PO.
- **Arranque:** este primer pull request, que asienta la delegación, lo fusiona el PO.
- **Revocable** por unidad, sin derogar esta delegación.
