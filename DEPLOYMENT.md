# Guía de Despliegue - IntegracionKoach360

## ⚠️ Errores Críticos y Sus Soluciones

### Error 1: `status=203/EXEC` (Servicio no inicia)
**Causa:** Falta permiso de ejecución en el binario  
**Solución:** `sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360`

### Error 2: `Globalization Invariant Mode is not supported`
**Causa:** Faltan librerías ICU nativas (libicudata.so, libicui18n.so, libicuuc.so)  
**Solución:** Copiar **TODA la carpeta `publish`** desde Windows, no solo el ejecutable

---

## 📦 Pre-requisitos

- Acceso SSH al servidor Linux (PuTTY)
- Acceso SFTP al servidor Linux (WinSCP)
- Servicio `integracion-koach360` ya configurado en el servidor
- Conectividad con SQL Server (10.10.100.12:1433)

---

## 🚀 Proceso de Despliegue

### Paso 1: Compilar para Linux

**Comando correcto (usar solo este):**

```bash
dotnet publish -c Release -o publish
```

✅ **Carpeta generada:** `publish/` (contiene ejecutable y todas las dependencias)

**⚠️ IMPORTANTE:** El comando simplificado funciona porque el `.csproj` ya tiene configurado:
- `RuntimeIdentifier=linux-x64`
- `PublishSelfContained=true`
- `PublishTrimmed=false` (necesario para incluir librerías ICU)

**Verificar que se generaron las librerías ICU:**

```powershell
# Desde PowerShell en Windows
cd C:\xampp\htdocs\klienteAPI\IntegracionKoach360\publish
dir libicu*
```

Debes ver archivos como:
- `libicudata.so.72.1.0.3`
- `libicui18n.so.72.1.0.3`
- `libicuuc.so.72.1.0.3`

Si **NO** aparecen estos archivos, hay un problema con la configuración del proyecto.

---

### Paso 2: Detener el Servicio en el Servidor

**Conectarse con PuTTY** y ejecutar:

```bash
# Detener el servicio
sudo systemctl stop integracion-koach360

# Verificar que se detuvo
sudo systemctl status integracion-koach360
```

---

### Paso 3: Hacer Backup de la Carpeta Actual

```bash
# Crear carpeta de backups si no existe
sudo mkdir -p /storage/IntegracionKoach360/backups

# Hacer backup completo de la carpeta publish con fecha
sudo cp -r /storage/IntegracionKoach360/publish \
       /storage/IntegracionKoach360/backups/publish_$(date +%Y%m%d_%H%M%S)

# Listar backups
ls -la /storage/IntegracionKoach360/backups/
```

---

### Paso 4: Copiar TODA la Carpeta Publish

⚠️ **IMPORTANTE:** Debes copiar **TODA la carpeta `publish`**, NO solo el ejecutable. 
La aplicación necesita todas las librerías nativas (ICU, SQL Client, etc).

**Opción A - Usando WinSCP (Recomendado):**

1. Conectarse al servidor
2. **Navegar local:** `C:\xampp\htdocs\klienteAPI\IntegracionKoach360\publish\`
3. **Navegar remoto:** `/storage/IntegracionKoach360/`
4. **Seleccionar TODOS los archivos de la carpeta `publish` local**
5. **Arrastrar a `/storage/IntegracionKoach360/publish/`** (sobrescribir todo)

⚠️ **NOTA WinSCP:** Al copiar archivos, WinSCP **NO preserva** los permisos de ejecución del archivo original. Por eso es **OBLIGATORIO** ejecutar `chmod +x` después de copiar.

Después de copiar con WinSCP, en **PuTTY** ejecutar:

```bash
# ⚠️ CRÍTICO: Dar permisos de ejecución
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360
sudo chmod 755 /storage/IntegracionKoach360/publish/libicu*
sudo chmod +x /storage/IntegracionKoach360/publish/createdump
sudo chmod 600 /storage/IntegracionKoach360/publish/config.json
sudo chown -R root:root /storage/IntegracionKoach360/publish

# Verificar permisos
ls -la /storage/IntegracionKoach360/publish/IntegracionKoach360
# Debe mostrar: -rwxr-xr-x (con 'x' = ejecutable)
```

**Opción B - Usando SCP desde PowerShell:**

```powershell
# Desde la carpeta del proyecto
cd C:\xampp\htdocs\klienteAPI\IntegracionKoach360

# Copiar toda la carpeta publish al servidor (ajusta usuario@servidor)
scp -r publish/* usuario@servidor:/tmp/publish_nuevo/
```

Luego en **PuTTY**:

```bash
# Mover toda la carpeta
sudo rm -rf /storage/IntegracionKoach360/publish
sudo mv /tmp/publish_nuevo /storage/IntegracionKoach360/publish

# ⚠️ CRÍTICO: Dar permisos de ejecución al ejecutable
# Este paso es OBLIGATORIO, de lo contrario el servicio fallará con error 203/EXEC
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360

# Dar permisos a librerías ICU y otras dependencias
sudo chmod 755 /storage/IntegracionKoach360/publish/libicu*
sudo chmod +x /storage/IntegracionKoach360/publish/createdump

# Dar permisos a configuración (solo lectura)
sudo chmod 600 /storage/IntegracionKoach360/publish/config.json

# Verificar propietario
sudo chown -R root:root /storage/IntegracionKoach360/publish
```

**Verificar que se copiaron las librerías ICU y los permisos:**

```bash
# Verificar librerías ICU
ls -la /storage/IntegracionKoach360/publish/libicu*

# Verificar permisos del ejecutable (DEBE tener 'x' en permisos)
ls -la /storage/IntegracionKoach360/publish/IntegracionKoach360
```

Debes ver:
- `libicudata.so.72.1.0.3`
- `libicui18n.so.72.1.0.3`
- `libicuuc.so.72.1.0.3`

Y el ejecutable debe mostrar: `-rwxr-xr-x` (con 'x' = ejecutable)

Si muestra `-rw-r--r--` (sin 'x'), el servicio **NO funcionará** (error 203/EXEC).

---

### Paso 5: Verificar Configuración

```bash
# Verificar que config.json tiene la configuración correcta
cat /storage/IntegracionKoach360/publish/config.json
```

**Debe contener:**

```json
{
  "usuario": "rolandpruebas-int",
  "password": "tu-password",
  "clienteId": 21,
  "usuarioApi": "rolandpruebas-int",
  "claveApi": "tu-clave-api",
  "intervaloHoras": 1,
  "baseUrl": "https://koach360.kliente.tech:5000",
  
  "database": {
    "connectionString": "Server=10.10.100.12;Database=STORECONTROL;User Id=consultas;Password=Datos.22;TrustServerCertificate=True;MultipleActiveResultSets=True;",
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

**Si no está correcto, editar:**

```bash
sudo nano /storage/IntegracionKoach360/publish/config.json
```

---

### Paso 6: Verificar Conectividad con SQL Server

```bash
# Verificar conectividad con SQL Server (opcional)
# Requiere instalar sqlcmd: sudo apt install mssql-tools
/opt/mssql-tools/bin/sqlcmd -S 10.10.100.12 -U consultas -P 'Datos.22' -Q "SELECT @@VERSION"

# Verificar conectividad de red
ping 10.10.100.12

# Verificar puerto SQL Server (1433)
telnet 10.10.100.12 1433
```

**Nota:** La aplicación consulta directamente SQL Server, no requiere archivos JSON externos.

---

### Paso 7: Iniciar el Servicio

```bash
# ⚠️ ANTES de iniciar, verificar una última vez los permisos del ejecutable
ls -la /storage/IntegracionKoach360/publish/IntegracionKoach360
# DEBE mostrar: -rwxr-xr-x (con 'x' = ejecutable)

# Si NO tiene permisos de ejecución, darlos ahora:
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360

# Iniciar el servicio
sudo systemctl start integracion-koach360

# Ver el estado
sudo systemctl status integracion-koach360

# Ver logs en tiempo real
sudo journalctl -u integracion-koach360 -f
```

**Estado esperado:**
```
Active: active (running)
```

**Si aparece error 203/EXEC:**
- Significa que falta permiso de ejecución
- Ejecuta: `sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360`
- Reinicia: `sudo systemctl restart integracion-koach360`

---

### Paso 8: Verificar Logs de la Aplicación

```bash
# Ver el log del día actual
tail -f /storage/sc22/logs/integracion/integracion-koach360-$(date +%Y%m%d).log
```

**Debes ver:**

```
[2025-09-30 HH:MM:SS] [INF] Iniciando IntegracionKoach360...
[2025-09-30 HH:MM:SS] [INF] Configuración cargada correctamente
[2025-09-30 HH:MM:SS] [INF] Procesando ventas...
[2025-09-30 HH:MM:SS] [INF] Enviando X venta(s)...
[2025-09-30 HH:MM:SS] [INF] Ventas enviadas exitosamente
[2025-09-30 HH:MM:SS] [INF] Procesando asistencias...
[2025-09-30 HH:MM:SS] [INF] Enviando X asistencia(s)...
[2025-09-30 HH:MM:SS] [INF] Asistencias enviadas exitosamente
```

---

## ✅ Verificación Post-Despliegue

### 1. Verificar que el servicio está corriendo

```bash
sudo systemctl is-active integracion-koach360
# Debe devolver: active
```

### 2. Verificar que está leyendo los archivos correctos

```bash
# Ver los últimos logs
sudo journalctl -u integracion-koach360 -n 50
```

Buscar líneas como:
- `Procesando ventas...`
- `Enviando X venta(s)...`
- `Ventas enviadas exitosamente`

### 3. Verificar que no hay errores

```bash
# Buscar errores en los logs
sudo journalctl -u integracion-koach360 | grep -i error
```

### 4. Monitorear la primera ejecución completa

```bash
# Ver logs en tiempo real hasta la próxima ejecución
sudo journalctl -u integracion-koach360 -f
```

Esperar hasta la siguiente hora para verificar que se ejecuta automáticamente.

---

## 🔄 Rollback (Si algo sale mal)

Si hay problemas, restaurar la versión anterior:

```bash
# Detener el servicio
sudo systemctl stop integracion-koach360

# Ver backups disponibles
ls -la /storage/IntegracionKoach360/backups/

# Restaurar el backup más reciente (reemplaza YYYYMMDD_HHMMSS con la fecha del backup)
sudo rm -rf /storage/IntegracionKoach360/publish
sudo cp -r /storage/IntegracionKoach360/backups/publish_YYYYMMDD_HHMMSS \
       /storage/IntegracionKoach360/publish

# Dar permisos
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360
sudo chmod 755 /storage/IntegracionKoach360/publish/libicu*

# Iniciar el servicio
sudo systemctl start integracion-koach360

# Verificar
sudo systemctl status integracion-koach360
```

---

## 📋 Checklist de Despliegue

- [ ] Compilar para Linux (`dotnet publish -c Release -o publish`)
- [ ] Verificar que se generaron las librerías ICU en `publish/` (Windows)
- [ ] Detener servicio en el servidor
- [ ] Hacer backup completo de la carpeta `publish` actual
- [ ] Copiar **TODA** la carpeta `publish` al servidor (no solo el ejecutable)
- [ ] Verificar que las librerías ICU se copiaron al servidor
- [ ] ⚠️ **CRÍTICO:** Dar permisos de ejecución (`chmod +x IntegracionKoach360`)
- [ ] Dar permisos a librerías ICU (`chmod 755 libicu*`)
- [ ] Verificar que el ejecutable tiene permisos (`ls -la` debe mostrar `-rwxr-xr-x`)
- [ ] Verificar config.json
- [ ] Verificar conectividad con SQL Server (opcional)
- [ ] Iniciar servicio
- [ ] Verificar estado del servicio (debe estar `active (running)`, NO `203/EXEC`)
- [ ] Verificar logs (journalctl) - buscar errores de Globalization o 203/EXEC
- [ ] Verificar logs de aplicación (`tail -f /storage/sc22/logs/integracion/...`)
- [ ] Confirmar conexión exitosa a SQL Server en logs
- [ ] Confirmar token de API obtenido en logs
- [ ] Monitorear primera ejecución completa
- [ ] Verificar que se ejecuta cada hora automáticamente

---

## 🆘 Solución de Problemas

### Problema: "No se pudo conectar a SQL Server"

```bash
# Verificar conectividad de red
ping 10.10.100.12

# Verificar puerto SQL (1433)
telnet 10.10.100.12 1433

# Ver error específico en logs
sudo journalctl -u integracion-koach360 -n 50 | grep -A 5 "Error al consultar"
```

### Problema: "Error de autenticación SQL Server"

```bash
# Verificar credenciales en config.json
cat /storage/IntegracionKoach360/publish/config.json | grep -A 3 "database"

# Probar conexión manual (si sqlcmd está instalado)
/opt/mssql-tools/bin/sqlcmd -S 10.10.100.12 -U consultas -P 'Datos.22' -Q "SELECT 1"
```

### Problema: "Token obtenido/renovado exitosamente" no aparece

```bash
# Verificar conectividad con la API
curl -X POST https://koach360.kliente.tech:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"rolandpruebas-int","password":"nJ33gzwxC3GL"}'
```

### Problema: El servicio no inicia (error 203/EXEC)

Este error ocurre cuando el ejecutable **no tiene permisos de ejecución**.

**Causa:** Al copiar archivos con WinSCP/SCP, los permisos de ejecución se pierden.

**Solución:**

```bash
# Verificar permisos actuales
ls -la /storage/IntegracionKoach360/publish/IntegracionKoach360

# Si muestra -rw-r--r-- (sin 'x'), dar permisos de ejecución
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360

# Reiniciar el servicio
sudo systemctl restart integracion-koach360

# Verificar estado
sudo systemctl status integracion-koach360
```

**Probar ejecutar manualmente:**

```bash
cd /storage/IntegracionKoach360/publish
./IntegracionKoach360
```

Si se ejecuta correctamente pero el servicio falla, revisar el archivo de servicio:

```bash
cat /etc/systemd/system/integracion-koach360.service
```

**Verificar que el ejecutable es válido:**

```bash
file /storage/IntegracionKoach360/publish/IntegracionKoach360
# Debe mostrar: ELF 64-bit LSB pie executable, x86-64
```

### Problema: "Globalization Invariant Mode is not supported"

Este error ocurre cuando faltan las **librerías ICU** nativas de Linux.

**Causa:** La carpeta `publish` no fue copiada completamente desde Windows.

**Solución:**

```bash
# 1. Verificar si existen las librerías ICU
ls -la /storage/IntegracionKoach360/publish/libicu*

# Si NO existen, debes republicar y copiar toda la carpeta desde Windows
```

**Desde Windows:**

```powershell
# Limpiar y republicar
cd C:\xampp\htdocs\klienteAPI\IntegracionKoach360
dotnet clean
dotnet publish -c Release -o publish

# Verificar que se generaron las librerías ICU
dir publish\libicu*
```

Si ves archivos como `libicudata.so.72.1.0.3`, entonces **copia TODA la carpeta publish al servidor** siguiendo el **Paso 4**.

**⚠️ CRÍTICO:** 
- NO copies solo el ejecutable
- Debes copiar **TODOS los archivos** de la carpeta `publish`
- Especialmente las librerías: `libicudata.so.*`, `libicui18n.so.*`, `libicuuc.so.*`

**Verificación en el servidor:**

```bash
# Ver las librerías ICU que requiere el ejecutable
cd /storage/IntegracionKoach360/publish
ldd IntegracionKoach360 | grep icu

# Verificar permisos de las librerías
ls -la libicu*

# Si no tienen permisos de ejecución
sudo chmod 755 /storage/IntegracionKoach360/publish/libicu*
```

---

## 📊 Mapeo de Campos PHP → .NET

### Ventas (snake_case → camelCase)

| Campo PHP | Campo .NET | Atributo JSON |
|-----------|------------|---------------|
| `asesor_cedula` | `asesorCedula` | `[JsonPropertyName("asesor_cedula")]` |
| `asesor_correo` | `asesorCorreo` | `[JsonPropertyName("asesor_correo")]` |
| `asesor_nombre` | `asesorNombre` | `[JsonPropertyName("asesor_nombre")]` |
| `factura_numero` | `facturaNumero` | `[JsonPropertyName("factura_numero")]` |
| `factura_fecha` | `facturaFecha` | `[JsonPropertyName("factura_fecha")]` |
| `factura_hora` | `facturaHora` | `[JsonPropertyName("factura_hora")]` |
| `factura_origen` | `facturaOrigen` | `[JsonPropertyName("factura_origen")]` |
| `lider_cedula` | `liderCedula` | `[JsonPropertyName("lider_cedula")]` |
| `lider_correo` | `liderCorreo` | `[JsonPropertyName("lider_correo")]` |
| `lider_nombre` | `liderNombre` | `[JsonPropertyName("lider_nombre")]` |
| `local_nombre` | `localNombre` | `[JsonPropertyName("local_nombre")]` |
| `valor_transaccion` | `valorTransaccion` | `[JsonPropertyName("valor_transaccion")]` |
| `cantidad_unidades` | `cantidadUnidades` | `[JsonPropertyName("cantidad_unidades")]` |

### Asistencias

| Campo PHP | Campo .NET | Atributo JSON |
|-----------|------------|---------------|
| `asesor_nombre` | `asesorNombre` | `[JsonPropertyName("asesor_nombre")]` |
| `asesor_cedula` | `asesorCedula` | `[JsonPropertyName("asesor_cedula")]` |
| `asesor_cargo` | `asesorCargo` | `[JsonPropertyName("asesor_cargo")]` |
| `asesor_correo` | `asesorCorreo` | `[JsonPropertyName("asesor_correo")]` |
| `local_nombre` | `localNombre` | `[JsonPropertyName("local_nombre")]` |

---

## 📝 Notas Finales

- La aplicación se ejecuta **cada 1 hora** automáticamente
- El token se renueva **cada 50 minutos** automáticamente
- Los logs se guardan en: `/storage/sc22/logs/integracion/`
- Los logs se rotan diariamente y se mantienen por **30 días**
- El servicio se reinicia automáticamente si falla (configurado en systemd)

### ⚠️ Errores Comunes y Soluciones Rápidas

| Error | Causa | Solución Rápida |
|-------|-------|----------------|
| `status=203/EXEC` | Falta permiso de ejecución | `sudo chmod +x IntegracionKoach360` |
| `Globalization Invariant Mode` | Faltan librerías ICU | Copiar TODA la carpeta `publish`, no solo el ejecutable |
| `No se pudo conectar a SQL Server` | Conectividad de red | Verificar firewall, ping, telnet al puerto 1433 |
| `Token no obtenido` | Credenciales incorrectas | Verificar `config.json`, probar con curl |

### 🎯 Señales de Éxito

Si todo está funcionando correctamente, deberías ver en los logs:

```
[INF] Iniciando IntegracionKoach360...
[INF] Configuración cargada correctamente
[INF] Procesando ventas...
[INF] Consulta de ventas ejecutada: X registros obtenidos
[INF] Procesando asistencias...
[INF] Consulta de asistencias ejecutada: X registros obtenidos
[INF] Token obtenido/renovado exitosamente
[INF] Ventas enviadas exitosamente
[INF] Asistencias enviadas exitosamente
[INF] Proceso de integración completado exitosamente
```

**NO** deberías ver:
- ❌ `Globalization Invariant Mode is not supported`
- ❌ `No se pudo conectar a SQL Server`
- ❌ `Error al autenticar`
- ❌ Error 203/EXEC en systemd

