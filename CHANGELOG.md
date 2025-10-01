# Changelog - IntegracionKoach360

## [1.2.0] - 2025-10-01

### 🔄 Changed
- **BREAKING CHANGE**: Consulta de ventas ahora retorna solo el día actual hasta la hora de ejecución
  - **Antes:** `V.Fecha >= DATEADD(DAY, -8, CAST(GETDATE() AS DATE)) AND V.Fecha < CAST(GETDATE() AS DATE)`
  - **Ahora:** `V.Fecha = CAST(GETDATE() AS DATE) AND V.Hora <= CONVERT(TIME, GETDATE())`
- Comportamiento optimizado para envío horario de ventas del día en curso

### 📚 Documentation
- Documentado comportamiento de DELETE selectivo de la API Koach360
- Agregada explicación de por qué se envía el día completo en cada ejecución
- Actualizada arquitectura con detalles de sincronización
- Documentado que la API elimina solo fechas/locales específicos del payload

### 🎯 Rationale
La API de Koach360 implementa DELETE selectivo:
```sql
DELETE FROM ventas 
WHERE cliente_id = X 
  AND fecha IN (fechas del payload) 
  AND local IN (locales del payload)
```

Esto significa:
- ✅ Datos históricos (días anteriores) permanecen intactos
- ✅ Cada ejecución debe enviar el día completo (00:00 hasta hora actual)
- ✅ NO se debe enviar solo la última hora (eliminaría ventas de horas anteriores)
- ✅ Diseño actual es correcto y necesario

---

## [1.1.0] - 2025-09-30

### ✨ Added
- Soporte para lectura de archivos JSON con formato snake_case (generados por scripts PHP)
- Configuración de rutas personalizables para archivos de ventas y asistencias en `config.json`
- Atributos `[JsonPropertyName]` en modelos para mapear campos PHP → .NET
- Documentación completa de despliegue en `DEPLOYMENT.md`

### 🔧 Changed
- **BREAKING CHANGE**: `DataModels.cs` ahora usa atributos `[JsonPropertyName]` para mapear snake_case
- Configuración de rutas de archivos JSON ahora es configurable vía `config.json`:
  - `rutaVentas`: Ruta al archivo de ventas generado por PHP
  - `rutaAsistencias`: Ruta al archivo de asistencias generado por PHP

### 📝 Technical Details

#### Mapeo de Campos PHP → .NET

**Ventas:**
- `asesor_cedula` → `asesorCedula`
- `asesor_correo` → `asesorCorreo`
- `asesor_nombre` → `asesorNombre`
- `factura_numero` → `facturaNumero`
- `factura_fecha` → `facturaFecha`
- `factura_hora` → `facturaHora`
- `factura_origen` → `facturaOrigen`
- `lider_cedula` → `liderCedula`
- `lider_correo` → `liderCorreo`
- `lider_nombre` → `liderNombre`
- `local_nombre` → `localNombre`
- `valor_transaccion` → `valorTransaccion`
- `cantidad_unidades` → `cantidadUnidades`
- `cliente_id` → `clienteId`
- `usuario_api` → `usuarioApi`
- `clave_api` → `claveApi`

**Asistencias:**
- `asesor_nombre` → `asesorNombre`
- `asesor_cedula` → `asesorCedula`
- `asesor_cargo` → `asesorCargo`
- `asesor_correo` → `asesorCorreo`
- `local_nombre` → `localNombre`
- `cliente_id` → `clienteId`
- `usuario_api` → `usuarioApi`
- `clave_api` → `claveApi`

### 🔗 Integration

Esta versión está diseñada para trabajar con los scripts PHP existentes:
- `/storage/tareas/syn_put_ventas.php`
- `/storage/tareas/syn_put_asistencias.php`

Los scripts PHP generan archivos JSON en formato snake_case que la aplicación .NET ahora puede leer correctamente.

### 📦 Deployment

Ver `DEPLOYMENT.md` para instrucciones completas de despliegue.

**Configuración requerida en `config.json`:**

```json
{
  "rutaVentas": "/storage/tareas/ventas.json",
  "rutaAsistencias": "/storage/tareas/asistencias.json",
  ...
}
```

---

## [1.0.0] - 2025-09-29

### Initial Release
- Sistema de integración automática con API Koach360
- Envío de ventas cada hora
- Envío de asistencias cada hora
- Autenticación automática con renovación de token cada 50 minutos
- Logging con Serilog (archivo + consola)
- Ejecución como servicio en Linux
- Validación y completado automático de campos requeridos

