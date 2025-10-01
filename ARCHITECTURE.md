# Arquitectura de la Aplicación IntegracionKoach360

## 📁 Estructura del Proyecto

```
IntegracionKoach360/
├── Program.cs                 # Punto de entrada de la aplicación
├── config.json               # Configuración de la aplicación
├── ventas.json               # Datos de ventas
├── asistencias.json          # Datos de asistencias
├── logs/                     # Directorio de logs (se crea automáticamente)
├── Models/                   # Modelos de datos
│   ├── ConfigurationModels.cs
│   ├── DataModels.cs
│   └── ApiResponseModels.cs
├── Interfaces/               # Contratos/Interfaces
│   ├── IConfigurationService.cs
│   ├── ILoggingService.cs
│   ├── IApiService.cs
│   ├── IDataService.cs
│   └── IIntegrationService.cs
└── Services/                 # Implementaciones de servicios
    ├── ConfigurationService.cs
    ├── LoggingService.cs
    ├── ApiService.cs
    ├── DataService.cs
    └── IntegrationService.cs
```

## 🏗️ Arquitectura por Capas

### 1. **Capa de Presentación** (`Program.cs`)
- Punto de entrada de la aplicación
- Inicialización de servicios
- Control del ciclo de vida de la aplicación

### 2. **Capa de Servicios** (`Services/`)
- **ConfigurationService**: Maneja la carga y validación de configuración
- **LoggingService**: Gestiona el sistema de logging con Serilog
- **ApiService**: Maneja la comunicación con la API de Koach360
- **DataService**: Gestiona la lectura y validación de archivos JSON
- **IntegrationService**: Orquesta el proceso completo de integración

### 3. **Capa de Modelos** (`Models/`)
- **ConfigurationModels**: Modelos para configuración y logging
- **DataModels**: Modelos para ventas y asistencias
- **ApiResponseModels**: Modelos para respuestas de la API

### 4. **Capa de Interfaces** (`Interfaces/`)
- Contratos que definen las responsabilidades de cada servicio
- Permite inyección de dependencias y testing

## 🔄 Flujo de la Aplicación

```
1. Program.cs
   ↓
2. ConfigurationService → Carga config.json
   ↓
3. LoggingService → Configura Serilog
   ↓
4. IntegrationService → Orquesta el proceso
   ↓
5. DatabaseService → Consulta SQL Server
   ↓
6. DataService → Valida y completa datos
   ↓
7. ApiService → Envía datos a Koach360 API
```

## 🔄 Comportamiento de la API Koach360

### **Estrategia de Sincronización: DELETE + INSERT**

La API de Koach360 implementa una estrategia de **reemplazo selectivo** en cada integración:

```sql
-- La API ejecuta internamente:
1. DELETE FROM ventas 
   WHERE cliente_id = X
     AND fecha IN (fechas del payload)
     AND local IN (locales del payload)

2. INSERT INTO ventas VALUES (payload completo)
```

### **Implicaciones del Diseño**

#### ✅ **Ventajas:**
- **Datos históricos protegidos**: Solo se eliminan/actualizan fechas y locales específicos del payload
- **Sincronización incremental por fecha**: Cada fecha se puede actualizar independientemente
- **Sin duplicados**: La API garantiza que no habrá registros duplicados
- **Tolerante a fallos**: Si una ejecución falla, la siguiente reemplaza los datos completos

#### ⚠️ **Consideraciones:**
- **Se debe enviar el día completo**: Cada ejecución debe incluir TODAS las ventas del día hasta la hora actual
- **No enviar solo la última hora**: Esto eliminaría las ventas de horas anteriores del día
- **Consulta acumulativa**: La query SQL debe retornar datos desde las 00:00 hasta la hora de ejecución

### **Ejemplo de Funcionamiento**

**Ejecución a las 09:00:**
```json
Payload: [
  { "fecha": "20251001", "local": "RL-PSC", ... },  // 5 ventas
  { "fecha": "20251001", "local": "RL-QSS2", ... }  // 8 ventas
]

API ejecuta:
  DELETE WHERE fecha='20251001' AND local IN ('RL-PSC', 'RL-QSS2')
  INSERT 13 ventas nuevas
```

**Ejecución a las 10:00:**
```json
Payload: [
  { "fecha": "20251001", "local": "RL-PSC", ... },  // 10 ventas (00:00-10:00)
  { "fecha": "20251001", "local": "RL-QSS2", ... }  // 15 ventas (00:00-10:00)
]

API ejecuta:
  DELETE WHERE fecha='20251001' AND local IN ('RL-PSC', 'RL-QSS2')
  INSERT 25 ventas nuevas (reemplaza las 13 anteriores)
```

**Resultado:** Los datos en Koach360 siempre reflejan el día completo hasta la última ejecución.

### **Diseño de Consultas SQL**

Debido a este comportamiento, las consultas deben ser **acumulativas por día**:

#### ✅ **CORRECTO - Consulta Actual:**
```sql
WHERE V.Fecha = CAST(GETDATE() AS DATE)           -- Solo HOY
  AND V.Hora <= CONVERT(TIME, GETDATE())           -- Hasta AHORA
```
**Resultado:** Cada ejecución envía todo el día acumulado (00:00 hasta hora actual)

#### ❌ **INCORRECTO - Solo última hora:**
```sql
WHERE V.Hora >= DATEADD(HOUR, -1, GETDATE())  -- Solo última hora
```
**Problema:** La API eliminaría las ventas de horas anteriores del día

## ✨ Beneficios de la Nueva Arquitectura

### **Separación de Responsabilidades**
- Cada clase tiene una responsabilidad específica
- Fácil mantenimiento y testing
- Código más legible y organizado

### **Inyección de Dependencias**
- Servicios son independientes entre sí
- Fácil intercambio de implementaciones
- Mejor testabilidad

### **Logging Avanzado**
- Sistema de logging con Serilog
- Rotación automática de archivos
- Diferentes niveles de log
- Logs tanto en consola como en archivos

### **Manejo de Errores**
- Manejo centralizado de errores
- Logging detallado de excepciones
- Recuperación graceful de errores

### **Configuración Externa**
- Toda la configuración en `config.json`
- Configuración de logging incluida
- Fácil modificación sin recompilar

## 🛠️ Servicios Principales

### **ConfigurationService**
```csharp
- LoadConfigurationAsync(): Carga config.json
- ValidateConfiguration(): Valida configuración
```

### **LoggingService**
```csharp
- ConfigureLogging(): Configura Serilog
- Information/Warning/Error/Fatal(): Métodos de logging
- CloseAndFlush(): Cierra y limpia recursos
```

### **ApiService**
```csharp
- GetTokenAsync(): Obtiene token de autenticación
- RenewTokenIfNeededAsync(): Renueva token si es necesario
- SendVentasAsync(): Envía datos de ventas
- SendAsistenciasAsync(): Envía datos de asistencias
```

### **DataService**
```csharp
- LoadVentasAsync(): Carga datos de ventas
- LoadAsistenciasAsync(): Carga datos de asistencias
- ValidateAndCompleteVentasAsync(): Valida y completa ventas
- ValidateAndCompleteAsistenciasAsync(): Valida y completa asistencias
```

### **IntegrationService**
```csharp
- ExecuteIntegrationAsync(): Ejecuta proceso completo
- StartTimer(): Inicia timer automático
- StopTimer(): Detiene timer
```

## 📝 Configuración de Logging

```json
{
  "logging": {
    "nivelMinimo": "Information",
    "guardarEnArchivo": true,
    "rutaArchivoLogs": "logs",
    "nombreArchivo": "integracion-koach360-{Date}.log",
    "tamañoMaximoArchivo": "10MB",
    "archivosRetenidos": 30,
    "mostrarEnConsola": true
  }
}
```

## 🚀 Uso

```bash
# Compilar
dotnet build

# Ejecutar
dotnet run
```

La aplicación mantiene la misma funcionalidad pero con una arquitectura mucho más limpia y mantenible.
