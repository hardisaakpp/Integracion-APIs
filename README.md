# IntegracionKoach360

Aplicación de integración automática con la API de Koach360 para envío de ventas y asistencias desde SQL Server cada hora.

## 🎯 Características

✅ **Conexión directa a SQL Server** - Consulta datos en tiempo real  
✅ **Autenticación automática** con renovación de token cada 50 minutos  
✅ **Ejecución automática cada hora** (configurable)  
✅ **Validación completa de datos** antes del envío  
✅ **Logging detallado** con timestamps y rotación diaria  
✅ **Manejo robusto de errores** con reintentos automáticos  
✅ **Configuración externa** via `config.json`  
✅ **Completado automático** de campos faltantes  
✅ **Ejecución como servicio** en Linux con systemd  
✅ **Monitoreo y mantenimiento** automatizado  
✅ **Sin dependencia de archivos JSON** - Consulta directa a BD  

---

## 📊 Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                  FLUJO DE INTEGRACIÓN                    │
└─────────────────────────────────────────────────────────┘

SQL Server (10.10.100.12)
    │  Bases de datos:
    │  - Analitica (ventas)
    │  - BISTAGING (empleados)
    │  - ElRayoBiometricos (biométricos)
    │  - BDDNOMINAMABEL19 (nómina)
    │  - plataforma_web (datos web)
    │
    ▼ [Query SQL cada hora]
    
IntegracionKoach360 (.NET App)
    │  - Consulta SQL Server
    │  - Valida datos
    │  - Completa campos
    │  - Genera logs
    │
    ▼ [POST con JWT Token]
    
API Koach360 (koach360.kliente.tech:5000)
    │  Endpoints:
    │  - /api/Auth/login
    │  - /api/Ventas/cargaVentasV1
    │  - /api/AsistenciaReal/cargaAsistenciaRealV1
    │
    ▼
    
Dashboard Koach360 ✅
```

---

## ⚙️ Configuración

### config.json

```json
{
  "usuario": "tu-usuario-koach360",
  "password": "tu-password",
  "clienteId": 21,
  "usuarioApi": "tu-usuario-api",
  "claveApi": "tu-clave-api",
  "intervaloHoras": 1,
  "baseUrl": "https://koach360.kliente.tech:5000",
  
  "database": {
    "connectionString": "Server=TU_SERVIDOR;Database=STORECONTROL;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "commandTimeout": 120
  },
  
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

### Parámetros de Configuración

| Parámetro | Descripción | Ejemplo |
|-----------|-------------|---------|
| `usuario` | Usuario para autenticación en Koach360 | `"rolandpruebas-int"` |
| `password` | Contraseña para autenticación | `"password123"` |
| `clienteId` | ID del cliente en Koach360 | `21` |
| `usuarioApi` | Usuario API para ventas/asistencias | `"rolandpruebas-int"` |
| `claveApi` | Clave API para ventas/asistencias | `"clave123"` |
| `intervaloHoras` | Frecuencia de ejecución | `1` (cada hora) |
| `baseUrl` | URL base de la API | `"https://koach360.kliente.tech:5000"` |
| `database.connectionString` | Cadena de conexión a SQL Server | Ver ejemplo arriba |
| `database.commandTimeout` | Timeout de consultas SQL (segundos) | `120` |

---

## 🚀 Uso

### Desarrollo

#### 1. Configurar el proyecto

```bash
# Clonar el repositorio
git clone https://github.com/hardisaakpp/Integracion-APIs.git
cd Integracion-APIs

# Configurar config.json con tus credenciales
cp config.example.json config.json
nano config.json  # Editar con tus datos
```

#### 2. Compilar

```bash
dotnet build
```

#### 3. Ejecutar en modo desarrollo

```bash
dotnet run
```

Presiona `q` para salir.

---

### Producción (Servidor Linux)

Ver **[DEPLOYMENT.md](DEPLOYMENT.md)** para instrucciones completas de despliegue.

#### Resumen rápido:

```bash
# 1. Compilar para Linux
dotnet publish -c Release -r linux-x64 --self-contained -o publish

# 2. Copiar al servidor (WinSCP o SCP)
#    - publish/IntegracionKoach360 → /storage/IntegracionKoach360/publish/
#    - config.json (configurado) → /storage/IntegracionKoach360/publish/

# 3. Configurar permisos
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360
sudo chmod 600 /storage/IntegracionKoach360/publish/config.json

# 4. Iniciar servicio
sudo systemctl start integracion-koach360
sudo systemctl status integracion-koach360
```

---

## 🗂️ Estructura de Archivos en Producción

```
/storage/IntegracionKoach360/publish/
├── IntegracionKoach360          # Ejecutable principal
├── config.json                  # Configuración (credenciales + connection string)
├── *.dll                        # Dependencias .NET
├── *.so                         # Librerías nativas Linux
└── backups/                     # Backups de versiones anteriores
    ├── IntegracionKoach360_YYYYMMDD_HHMMSS
    └── config.json_YYYYMMDD_HHMMSS

/storage/sc22/logs/integracion/
└── integracion-koach360-YYYYMMDD.log  # Logs diarios (30 días de retención)
```

---

## 📋 Funcionalidades Implementadas

### 1. **Consulta Directa a SQL Server**

#### Ventas (Últimos 8 días):
- Consulta: `Analitica..DWH_VENTASGENERAL_VIEW`
- Join con: `BISTAGING..STG_EMPLEADOS`
- Filtros:
  - Tiendas: RL-PSC, RL-QSS2, RL-SCA
  - Excluye vendedores: 114, 1150
  - Fecha: Últimos 8 días (sin incluir hoy)

#### Asistencias (Últimos 7 días):
- Consulta: `ElRayoBiometricos.dbo.VistaRegistrosT`
- Joins con:
  - `BDDNOMINAMABEL19..Tbl_DatosPersonales`
  - `plataforma_web.dbo.tmp_kliente`
- Filtros:
  - Cargos: ASESOR DE VENTAS, ASESOR VARIOS
  - Solo empleados activos
  - Solo con tienda asignada

### 2. **Integración con Koach360**

- **Endpoint Ventas:** `/api/Ventas/cargaVentasV1`
- **Endpoint Asistencias:** `/api/AsistenciaReal/cargaAsistenciaRealV1`
- **Autenticación:** JWT Bearer Token (renovación automática cada 50 min)
- **Método:** POST con JSON

### 3. **Validación y Completado de Datos**

- Valida campos requeridos antes del envío
- Completa `clienteId`, `usuarioApi`, `claveApi` automáticamente
- Filtra registros con datos incompletos
- Registra advertencias para datos rechazados

### 4. **Ejecución Automática**

- Timer configurable (cada 1 hora por defecto)
- Ejecución inmediata al iniciar
- Modo servicio (systemd) con auto-reinicio
- Modo interactivo (desarrollo) con salida 'q'

### 5. **Logging Avanzado**

- Logs a archivo con rotación diaria
- Logs a consola (systemd journal)
- Niveles: Information, Warning, Error, Fatal
- Retención: 30 días por defecto
- Formato: `[YYYY-MM-DD HH:MM:SS] [LEVEL] Mensaje`

---

## 📝 Logs de Ejemplo

```
[2025-09-30 15:00:00] [INF] Iniciando IntegracionKoach360...
[2025-09-30 15:00:00] [INF] Configuración cargada correctamente
[2025-09-30 15:00:00] [INF] Intervalo de ejecución: cada 1 hora(s)
[2025-09-30 15:00:00] [INF] ========================================
[2025-09-30 15:00:00] [INF] Iniciando proceso de integración...
[2025-09-30 15:00:00] [INF] Procesando ventas...
[2025-09-30 15:00:00] [INF] Consulta de ventas ejecutada: 45 registros obtenidos
[2025-09-30 15:00:00] [INF] Enviando 45 venta(s)...
[2025-09-30 15:00:01] [INF] Token obtenido/renovado exitosamente
[2025-09-30 15:00:02] [INF] Ventas enviadas exitosamente
[2025-09-30 15:00:02] [INF] Respuesta: {
  "mensaje": "Proceso completado.",
  "ventasExitosas": 45,
  "ventasFallidas": 0,
  "errores": []
}
[2025-09-30 15:00:02] [INF] Procesando asistencias...
[2025-09-30 15:00:02] [INF] Consulta de asistencias ejecutada: 27 registros obtenidos
[2025-09-30 15:00:02] [INF] Enviando 27 asistencia(s)...
[2025-09-30 15:00:03] [INF] Asistencias enviadas exitosamente
[2025-09-30 15:00:03] [INF] Respuesta: {
  "mensajes": [],
  "estadisticas": {
    "AsistenciasCreadas": 27,
    "AsistenciasEliminadas": 0,
    "AsistenciasActualizadas": 0,
    "PersonasCreadas": 0,
    "CargosCreados": 0
  }
}
[2025-09-30 15:00:03] [INF] Proceso de integración completado exitosamente
[2025-09-30 15:00:03] [INF] ========================================
[2025-09-30 15:00:03] [INF] Timer configurado para ejecutar cada 1 hora(s)
[2025-09-30 15:00:03] [INF] Aplicación ejecutándose como servicio...
```

---

## 💻 Requisitos del Sistema

### Desarrollo (Windows/Mac/Linux)
- .NET 9.0 SDK
- Visual Studio Code o Visual Studio
- Acceso a SQL Server
- Conexión a internet

### Producción (Linux)
- Sistema operativo Linux (Ubuntu 20.04+ recomendado)
- Conexión a SQL Server (puerto 1433)
- Conexión a internet (API Koach360)
- Permisos de escritura en directorio de logs
- **NO requiere** .NET Runtime (self-contained)

---

## 🔧 Configuración del Servicio Systemd

Archivo: `/etc/systemd/system/integracion-koach360.service`

```ini
[Unit]
Description=IntegracionKoach360 - Integración automática con Koach360
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=/storage/IntegracionKoach360/publish
ExecStart=/storage/IntegracionKoach360/publish/IntegracionKoach360
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

### Comandos de gestión del servicio:

```bash
# Recargar configuración
sudo systemctl daemon-reload

# Habilitar inicio automático
sudo systemctl enable integracion-koach360

# Iniciar el servicio
sudo systemctl start integracion-koach360

# Ver estado
sudo systemctl status integracion-koach360

# Ver logs en tiempo real
sudo journalctl -u integracion-koach360 -f

# Detener el servicio
sudo systemctl stop integracion-koach360

# Reiniciar el servicio
sudo systemctl restart integracion-koach360

# Deshabilitar inicio automático
sudo systemctl disable integracion-koach360
```

---

## 📊 Datos Enviados

### Ventas → `/api/Ventas/cargaVentasV1`

**Origen:** `Analitica..DWH_VENTASGENERAL_VIEW` (últimos 8 días)

**Campos enviados:**
- Información de factura: número, fecha, hora, origen, valor, unidades
- Información del vendedor: nombre, cédula, correo
- Información del líder: nombre, cédula, correo
- Información del local: nombre
- Credenciales API: clienteId, usuarioApi, claveApi

**Filtros aplicados:**
- Tiendas: RL-PSC, RL-QSS2, RL-SCA
- Excluye vendedores: 114, 1150

### Asistencias → `/api/AsistenciaReal/cargaAsistenciaRealV1`

**Origen:** `ElRayoBiometricos.dbo.VistaRegistrosT` (últimos 7 días)

**Campos enviados:**
- Información del empleado: nombre, cédula, cargo, correo
- Información de asistencia: fecha, hora
- Información del local: nombre
- Credenciales API: clienteId, usuarioApi, claveApi

**Filtros aplicados:**
- Cargos: ASESOR DE VENTAS, ASESOR VARIOS
- Solo empleados activos
- Solo con tienda asignada

---

## 🛠️ Actualización de la Aplicación

Ver **[DEPLOYMENT.md](DEPLOYMENT.md)** para instrucciones detalladas.

### Resumen rápido:

```bash
# 1. Detener el servicio
sudo systemctl stop integracion-koach360

# 2. Hacer backup
sudo cp /storage/IntegracionKoach360/publish/IntegracionKoach360 \
     /storage/IntegracionKoach360/backups/IntegracionKoach360_$(date +%Y%m%d_%H%M%S)

# 3. Copiar nuevo ejecutable (WinSCP)
#    Origen: publish/IntegracionKoach360
#    Destino: /storage/IntegracionKoach360/publish/

# 4. Dar permisos
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360

# 5. Iniciar el servicio
sudo systemctl start integracion-koach360
sudo journalctl -u integracion-koach360 -f
```

---

## 🔍 Monitoreo y Mantenimiento

### Verificar que el servicio está corriendo

```bash
# Estado del servicio
sudo systemctl is-active integracion-koach360

# Información detallada
sudo systemctl status integracion-koach360

# Ver proceso
ps aux | grep IntegracionKoach360
```

### Ver logs

```bash
# Logs del servicio (systemd)
sudo journalctl -u integracion-koach360 -f

# Logs de la aplicación (archivo)
tail -f /storage/sc22/logs/integracion/integracion-koach360-$(date +%Y%m%d).log

# Últimas 100 líneas
tail -n 100 /storage/sc22/logs/integracion/integracion-koach360-$(date +%Y%m%d).log

# Buscar errores
sudo journalctl -u integracion-koach360 | grep -i error
```

### Verificar conectividad

```bash
# Conectividad con API Koach360
curl -X POST https://koach360.kliente.tech:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"tu-usuario","password":"tu-password"}'

# Conectividad con SQL Server (desde el servidor Linux)
# Requiere instalar sqlcmd: sudo apt install mssql-tools
/opt/mssql-tools/bin/sqlcmd -S 10.10.100.12 -U consultas -P 'Datos.22' -Q "SELECT @@VERSION"
```

### Verificar espacio en disco

```bash
# Espacio en /storage
df -h /storage/

# Tamaño de logs
du -sh /storage/sc22/logs/integracion/

# Listar logs antiguos
ls -lht /storage/sc22/logs/integracion/
```

---

## 🆘 Solución de Problemas

### Error: "No se pudo conectar a SQL Server"

```bash
# Verificar conectividad de red
ping 10.10.100.12

# Verificar puerto SQL (1433)
telnet 10.10.100.12 1433

# Ver error específico en logs
sudo journalctl -u integracion-koach360 -n 50 | grep -A 5 "Error al consultar"
```

**Soluciones:**
- Verificar firewall del servidor SQL Server
- Verificar que SQL Server acepta conexiones remotas
- Verificar usuario y contraseña en `config.json`

### Error: "Token obtenido/renovado exitosamente" no aparece

```bash
# Verificar conectividad con API
curl -v https://koach360.kliente.tech:5000/api/Auth/login

# Ver logs de error
sudo journalctl -u integracion-koach360 | grep -A 5 "Error al obtener token"
```

**Soluciones:**
- Verificar credenciales en `config.json`
- Verificar que la URL es correcta
- Verificar firewall/proxy

### Error: "Consulta de ventas ejecutada: 0 registros"

**Posibles causas:**
- No hay datos en el rango de fechas (últimos 8 días)
- Filtros muy restrictivos (solo 3 tiendas)
- Problema con la consulta SQL

```bash
# Ver logs detallados
tail -100 /storage/sc22/logs/integracion/integracion-koach360-$(date +%Y%m%d).log
```

### Error: El servicio no inicia

```bash
# Ver error completo
sudo journalctl -u integracion-koach360 -xe

# Probar ejecutar manualmente
cd /storage/IntegracionKoach360/publish
./IntegracionKoach360

# Verificar permisos
ls -la /storage/IntegracionKoach360/publish/IntegracionKoach360
```

### Restaurar versión anterior (Rollback)

```bash
# Detener servicio
sudo systemctl stop integracion-koach360

# Listar backups disponibles
ls -lht /storage/IntegracionKoach360/backups/

# Restaurar versión anterior
sudo cp /storage/IntegracionKoach360/backups/IntegracionKoach360_YYYYMMDD_HHMMSS \
     /storage/IntegracionKoach360/publish/IntegracionKoach360

# Dar permisos
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360

# Iniciar servicio
sudo systemctl start integracion-koach360
```

---

## 📦 Tecnologías Utilizadas

- **.NET 9.0** - Framework principal
- **Microsoft.Data.SqlClient** - Conexión a SQL Server
- **Serilog** - Logging estructurado
- **System.Net.Http** - Cliente HTTP para API
- **System.Text.Json** - Serialización JSON
- **Systemd** - Gestión de servicios en Linux

---

## 📚 Documentación Adicional

- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Guía completa de despliegue paso a paso
- **[CHANGELOG.md](CHANGELOG.md)** - Historial de cambios y versiones
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Documentación técnica de la arquitectura

---

## 🔐 Seguridad

- Credenciales almacenadas en `config.json` (excluido de Git)
- Connection string protegido (permisos 600)
- TrustServerCertificate para SSL/TLS
- Token JWT con renovación automática
- Logs sin información sensible

---

## 📞 Soporte

Para problemas o preguntas:
1. Revisar los logs: `sudo journalctl -u integracion-koach360 -f`
2. Verificar [DEPLOYMENT.md](DEPLOYMENT.md) para guías de solución de problemas
3. Consultar el código fuente en: https://github.com/hardisaakpp/Integracion-APIs

---

## 📄 Licencia

Proyecto privado - Uso interno únicamente.

---

## 🎯 Notas Importantes

- ⚠️ **NO subir `config.json` a Git** - Contiene credenciales sensibles
- ⚠️ **Hacer backup antes de actualizar** - Usa los scripts de despliegue
- ⚠️ **Monitorear logs regularmente** - Detectar problemas temprano
- ⚠️ **Verificar espacio en disco** - Los logs pueden crecer
- ✅ **El servicio se reinicia automáticamente** si falla
- ✅ **Los logs se rotan automáticamente** cada día
- ✅ **La aplicación es independiente** - No requiere .NET instalado (self-contained)
