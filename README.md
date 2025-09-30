# IntegracionKoach360

Aplicación de integración automática con la API de Koach360 para envío de ventas y asistencias cada hora.

## Instalación Rápida

```bash
# 1. Clonar el repositorio
git clone https://github.com/hardisaakpp/Integracion-APIs.git
cd Integracion-APIs

# 2. Crear archivos de configuración desde las plantillas
cp config.example.json config.json
cp ventas.example.json ventas.json
cp asistencias.example.json asistencias.json

# 3. Editar config.json con tus credenciales reales
nano config.json  # o usa tu editor favorito

# 4. Compilar y ejecutar
dotnet build
dotnet run
```

## Características

✅ **Autenticación automática** con renovación de token cada 50 minutos
✅ **Ejecución automática cada hora** (configurable)
✅ **Validación completa de datos** antes del envío
✅ **Logging detallado** con timestamps
✅ **Manejo robusto de errores**
✅ **Configuración externa** via `config.json`
✅ **Completado automático** de campos faltantes
✅ **Ejecución como servicio** en Linux
✅ **Monitoreo y mantenimiento** automatizado

## Configuración

### config.json
Copia `config.example.json` a `config.json` y configura tus credenciales:

```json
{
  "usuario": "tu-usuario-koach360",
  "password": "tu-password",
  "clienteId": 0,
  "usuarioApi": "tu-usuario-api",
  "claveApi": "tu-clave-api",
  "intervaloHoras": 1,
  "baseUrl": "https://koach360.kliente.tech:5000",
  "logging": {
    "nivelMinimo": "Information",
    "guardarEnArchivo": true,
    "rutaArchivoLogs": "/storage/sc22/logs/integracion",
    "nombreArchivo": "integracion-koach360-{Date}.log",
    "tamañoMaximoArchivo": "10MB",
    "archivosRetenidos": 30,
    "mostrarEnConsola": true
  }
}
```

### ventas.json
Archivo con las ventas del día en formato array JSON (se actualiza dinámicamente por el sistema externo):
```json
[
  {
    "asesorCedula": "1728662147",
    "asesorCorreo": "vendedor@empresa.com",
    "asesorNombre": "NOMBRE VENDEDOR",
    "facturaHora": "21:11:00",
    "facturaFecha": "2025-01-15",
    "facturaNumero": "001-00012345",
    "liderCedula": "1715033674",
    "liderCorreo": "lider@empresa.com",
    "liderNombre": "NOMBRE LIDER",
    "localNombre": "TIENDA PRINCIPAL",
    "valorTransaccion": 150.50,
    "cantidadUnidades": 1,
    "facturaOrigen": "FAC"
  }
]
```

### asistencias.json
Archivo con las asistencias del día en formato array JSON (se actualiza dinámicamente por el sistema externo):
```json
[
  {
    "asesorCedula": "1728662147",
    "asesorNombre": "NOMBRE EMPLEADO",
    "asesorCargo": "ASESOR DE VENTAS",
    "asesorCorreo": "empleado@empresa.com",
    "fecha": "20250115",
    "hora": "08:00",
    "localNombre": "TIENDA PRINCIPAL"
  }
]
```

## Uso

### Desarrollo

#### Compilar
```bash
dotnet build
```

#### Ejecutar en modo desarrollo
```bash
dotnet run
```

### Producción (Servidor Linux)

#### 1. Despliegue
```bash
# Compilar para producción
dotnet publish -c Release -r linux-x64 --self-contained

# Mover a directorio de producción
sudo mv /ruta/origen/IntegracionKoach360 /storage/

# Configurar permisos
cd /storage/IntegracionKoach360
sudo chmod +x IntegracionKoach360
sudo chmod 644 *.json
sudo mkdir -p logs
sudo chmod 755 logs
```

#### 2. Ejecución Manual
```bash
# Ejecutar una vez
cd /storage/IntegracionKoach360
./IntegracionKoach360

# Ejecutar en background
nohup ./IntegracionKoach360 > output.log 2>&1 &

# Ver logs en tiempo real
tail -f logs/integracion-koach360-$(date +%Y%m%d).log
```

#### 3. Ejecución como Servicio (Recomendado)

Crear archivo de servicio: `/etc/systemd/system/integracion-koach360.service`

```ini
[Unit]
Description=IntegracionKoach360 - Integración automática con Koach360
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=/storage/IntegracionKoach360
ExecStart=/storage/IntegracionKoach360/IntegracionKoach360
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

Comandos para gestionar el servicio:
```bash
# Recargar configuración del sistema
sudo systemctl daemon-reload

# Habilitar el servicio
sudo systemctl enable integracion-koach360

# Iniciar el servicio
sudo systemctl start integracion-koach360

# Ver estado del servicio
sudo systemctl status integracion-koach360

# Ver logs del servicio
sudo journalctl -u integracion-koach360 -f

# Detener el servicio
sudo systemctl stop integracion-koach360

# Reiniciar el servicio
sudo systemctl restart integracion-koach360

# Para DESHABILITAR que inicie automáticamente al arrancar el servidor
sudo systemctl disable integracion-koach360
```

#### 4. Monitoreo y Mantenimiento

```bash
# Verificar que el proceso está ejecutándose
ps aux | grep IntegracionKoach360

# Ver logs más recientes
tail -n 50 logs/integracion-koach360-$(date +%Y%m%d).log

# Verificar espacio en disco
df -h /storage/

# Verificar conectividad con la API
curl -X POST https://koach360.kliente.tech:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"rolandpruebas-int","password":"nJ33gzwxC3GL"}'
```

#### 5. Actualización de la Aplicación

```bash
# Detener el servicio
sudo systemctl stop integracion-koach360

# Crear backup
sudo cp -r /storage/IntegracionKoach360 /storage/IntegracionKoach360_backup_$(date +%Y%m%d_%H%M%S)

# Reemplazar archivos (mantener config.json y logs)
sudo cp nueva_version/IntegracionKoach360 /storage/IntegracionKoach360/
sudo chmod +x /storage/IntegracionKoach360/IntegracionKoach360

# Iniciar el servicio
sudo systemctl start integracion-koach360
```

La aplicación:
1. **Se ejecuta inmediatamente** al iniciar
2. **Configura un timer** para ejecutarse cada hora (configurable)
3. **Se mantiene en ejecución** hasta presionar 'q' (desarrollo) o se detiene el servicio (producción)
4. **Procesa automáticamente** ventas y asistencias

## Funcionalidades Implementadas

### ✅ Requerimientos Cumplidos

1. **Autenticación con Koach360**
   - Endpoint: `/api/Auth/login`
   - Credenciales configurables
   - Renovación automática de token

2. **Integración de Ventas**
   - Endpoint: `/api/Ventas/cargaVentasV1`
   - Validación completa de datos
   - Campos auto-completados desde configuración

3. **Integración de Asistencias**
   - Endpoint: `/api/AsistenciaReal/cargaAsistenciaRealV1`
   - Validación completa de datos
   - Campos auto-completados desde configuración

4. **Ejecución Automática**
   - Timer configurable (cada 1 hora por defecto)
   - Ejecución inmediata al iniciar
   - Renovación automática de token

5. **Manejo de Errores**
   - Logging detallado con timestamps
   - Validación de datos antes del envío
   - Manejo graceful de errores de red

6. **Configuración Externa**
   - Archivo `config.json` para credenciales
   - No más credenciales hardcodeadas
   - Configuración del intervalo de ejecución

## Logs de Ejemplo

```
[2025-01-15 10:00:00] Iniciando IntegracionKoach360...
[2025-01-15 10:00:00] Configuración cargada correctamente
[2025-01-15 10:00:00] Intervalo de ejecución: cada 1 hora(s)
[2025-01-15 10:00:00] ========================================
[2025-01-15 10:00:00] Iniciando proceso de integración...
[2025-01-15 10:00:00] Autenticando en: https://koach360.kliente.tech:5000/api/Auth/login
[2025-01-15 10:00:01] Token obtenido/renovado exitosamente
[2025-01-15 10:00:01] Procesando ventas...
[2025-01-15 10:00:01] Enviando 1 venta(s)...
[2025-01-15 10:00:02] Ventas enviadas exitosamente
[2025-01-15 10:00:02] Procesando asistencias...
[2025-01-15 10:00:02] Enviando 1 asistencia(s)...
[2025-01-15 10:00:03] Asistencias enviadas exitosamente
[2025-01-15 10:00:03] Proceso de integración completado exitosamente
[2025-01-15 10:00:03] ========================================
```

## Requisitos del Sistema

### Desarrollo
- .NET 9.0 SDK
- Visual Studio Code o Visual Studio
- Conexión a internet

### Producción
- .NET 9.0 Runtime (o aplicación self-contained)
- Linux (Ubuntu 20.04+ recomendado)
- Conexión a internet
- Archivos `config.json`, `ventas.json`, `asistencias.json` en el directorio de ejecución
- Permisos de escritura en directorio de logs

## Estructura de Archivos en Producción

```
/storage/IntegracionKoach360/
├── IntegracionKoach360          # Ejecutable principal
├── config.json                  # Configuración de la aplicación
├── ventas.json                  # Datos de ventas (se actualiza dinámicamente)
├── asistencias.json             # Datos de asistencias (se actualiza dinámicamente)
├── logs/                        # Directorio de logs
│   └── integracion-koach360-YYYYMMDD.log
└── IntegracionKoach360.deps.json
```

## Notas Importantes

- La aplicación valida todos los campos requeridos antes del envío
- Los campos faltantes (`clienteId`, `usuarioApi`, `claveApi`) se completan automáticamente desde `config.json`
- El token se renueva automáticamente cada 50 minutos
- La aplicación registra todos los eventos con timestamps detallados
- En modo desarrollo: presionar 'q' para salir de la aplicación
- En producción: usar `systemctl` para gestionar el servicio
- Los archivos `ventas.json` y `asistencias.json` deben ser actualizados por el sistema externo
- Los logs se rotan automáticamente cada día y se mantienen por 30 días

## Solución de Problemas

### Error de Conectividad
```bash
# Verificar conectividad
ping koach360.kliente.tech

# Verificar puerto
telnet koach360.kliente.tech 5000
```

### Error de Permisos
```bash
# Verificar permisos
ls -la /storage/IntegracionKoach360/

# Corregir permisos
sudo chmod +x /storage/IntegracionKoach360/IntegracionKoach360
sudo chmod 644 /storage/IntegracionKoach360/*.json
```

### Error de Dependencias
```bash
# Verificar dependencias
ldd /storage/IntegracionKoach360/IntegracionKoach360

# Instalar dependencias si es necesario
sudo apt-get update
sudo apt-get install libc6 libgcc1 libstdc++6
```

### Verificar Estado del Servicio
```bash
# Ver estado
sudo systemctl status integracion-koach360

# Ver logs
sudo journalctl -u integracion-koach360 -f

# Reiniciar si es necesario
sudo systemctl restart integracion-koach360
```
