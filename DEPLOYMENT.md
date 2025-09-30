# Guía de Despliegue - IntegracionKoach360

## 📦 Pre-requisitos

- Acceso SSH al servidor Linux (PuTTY)
- Acceso SFTP al servidor Linux (WinSCP)
- Servicio `integracion-koach360` ya configurado en el servidor
- Scripts PHP generando archivos en `/storage/tareas/`

---

## 🚀 Proceso de Despliegue

### Paso 1: Compilar para Linux (Ya realizado)

```bash
dotnet publish -c Release -r linux-x64 --self-contained -o publish
```

✅ **Ejecutable generado:** `publish/IntegracionKoach360`

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

### Paso 3: Hacer Backup del Ejecutable Actual

```bash
# Crear carpeta de backups si no existe
sudo mkdir -p /storage/IntegracionKoach360/backups

# Hacer backup con fecha
sudo cp /storage/IntegracionKoach360/publish/IntegracionKoach360 \
       /storage/IntegracionKoach360/backups/IntegracionKoach360_$(date +%Y%m%d_%H%M%S)

# Listar backups
ls -la /storage/IntegracionKoach360/backups/
```

---

### Paso 4: Copiar el Nuevo Ejecutable

**Usando WinSCP:**

1. Conectarse al servidor
2. Navegar a: `C:\xampp\htdocs\klienteAPI\IntegracionKoach360\publish\` (local)
3. Navegar a: `/storage/IntegracionKoach360/publish/` (remoto)
4. Copiar **solo el archivo**: `IntegracionKoach360`
5. **NO copiar** los archivos `*.json` (el servidor usa los de `/storage/tareas/`)

**O usando SCP desde PowerShell:**

```powershell
# Desde Windows
scp publish/IntegracionKoach360 usuario@servidor:/tmp/IntegracionKoach360_nuevo
```

Luego en **PuTTY**:

```bash
# Mover al destino
sudo mv /tmp/IntegracionKoach360_nuevo /storage/IntegracionKoach360/publish/IntegracionKoach360

# Dar permisos de ejecución
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360
```

---

### Paso 5: Verificar Configuración

```bash
# Verificar que config.json tiene las rutas correctas
cat /storage/IntegracionKoach360/publish/config.json
```

**Debe contener:**

```json
{
  "rutaVentas": "/storage/tareas/ventas.json",
  "rutaAsistencias": "/storage/tareas/asistencias.json",
  ...
}
```

**Si no está correcto, editar:**

```bash
sudo nano /storage/IntegracionKoach360/publish/config.json
```

---

### Paso 6: Verificar que los Scripts PHP Están Generando los JSON

```bash
# Ver contenido de los archivos
ls -la /storage/tareas/*.json

# Ver contenido (primeras líneas)
head -20 /storage/tareas/ventas.json
head -20 /storage/tareas/asistencias.json

# Ver logs de los scripts PHP
tail -20 /storage/tareas/syn_put_ventas.log
tail -20 /storage/tareas/syn_put_asistencias.log
```

---

### Paso 7: Iniciar el Servicio

```bash
# Iniciar el servicio
sudo systemctl start integracion-koach360

# Ver el estado
sudo systemctl status integracion-koach360

# Ver logs en tiempo real
sudo journalctl -u integracion-koach360 -f
```

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

# Restaurar el backup más reciente
sudo cp /storage/IntegracionKoach360/backups/IntegracionKoach360_YYYYMMDD_HHMMSS \
       /storage/IntegracionKoach360/publish/IntegracionKoach360

# Dar permisos
sudo chmod +x /storage/IntegracionKoach360/publish/IntegracionKoach360

# Iniciar el servicio
sudo systemctl start integracion-koach360

# Verificar
sudo systemctl status integracion-koach360
```

---

## 📋 Checklist de Despliegue

- [ ] Compilar para Linux
- [ ] Detener servicio
- [ ] Hacer backup del ejecutable actual
- [ ] Copiar nuevo ejecutable al servidor
- [ ] Dar permisos de ejecución (chmod +x)
- [ ] Verificar config.json
- [ ] Verificar que archivos PHP existen en /storage/tareas/
- [ ] Iniciar servicio
- [ ] Verificar logs (journalctl)
- [ ] Verificar logs de aplicación
- [ ] Monitorear primera ejecución
- [ ] Verificar que se ejecuta cada hora

---

## 🆘 Solución de Problemas

### Problema: "Archivo no encontrado: /storage/tareas/ventas.json"

```bash
# Verificar que el archivo existe
ls -la /storage/tareas/ventas.json

# Si no existe, ejecutar manualmente el script PHP
php /ruta/al/script/syn_put_ventas.php
```

### Problema: "Error al leer archivo"

```bash
# Verificar permisos
ls -la /storage/tareas/*.json

# Dar permisos si es necesario
sudo chmod 644 /storage/tareas/*.json
```

### Problema: "Token obtenido/renovado exitosamente" no aparece

```bash
# Verificar conectividad con la API
curl -X POST https://koach360.kliente.tech:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"rolandpruebas-int","password":"nJ33gzwxC3GL"}'
```

### Problema: El servicio no inicia

```bash
# Ver logs completos del error
sudo journalctl -u integracion-koach360 -xe

# Verificar que el ejecutable es válido
file /storage/IntegracionKoach360/publish/IntegracionKoach360

# Probar ejecutar manualmente
cd /storage/IntegracionKoach360/publish
./IntegracionKoach360
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

