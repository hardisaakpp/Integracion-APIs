using System.Text.Json;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;

namespace IntegracionKoach360.Services
{
    public class DataService : IDataService
    {
        private readonly ILoggingService _loggingService;
        private readonly ConfiguracionApp _config;

        public DataService(ILoggingService loggingService, ConfiguracionApp config)
        {
            _loggingService = loggingService;
            _config = config;
        }

        public async Task<VentaData[]?> LoadVentasAsync()
        {
            return await LoadDataFromFile<VentaData[]>(_config.rutaVentas);
        }

        public async Task<AsistenciaData[]?> LoadAsistenciasAsync()
        {
            return await LoadDataFromFile<AsistenciaData[]>(_config.rutaAsistencias);
        }

        public Task<VentaData[]> ValidateAndCompleteVentasAsync(VentaData[] ventas)
        {
            var ventasValidas = new List<VentaData>();

            foreach (var venta in ventas)
            {
                // Completar campos desde configuración si están vacíos
                if (venta.clienteId == 0) venta.clienteId = _config.clienteId;
                if (string.IsNullOrEmpty(venta.usuarioApi)) venta.usuarioApi = _config.usuarioApi;
                if (string.IsNullOrEmpty(venta.claveApi)) venta.claveApi = _config.claveApi;

                // Validar campos requeridos
                if (ValidateVenta(venta))
                {
                    ventasValidas.Add(venta);
                }
                else
                {
                    _loggingService.Warning("Venta inválida - Factura: {FacturaNumero}", venta.facturaNumero);
                }
            }

            return Task.FromResult(ventasValidas.ToArray());
        }

        public Task<AsistenciaData[]> ValidateAndCompleteAsistenciasAsync(AsistenciaData[] asistencias)
        {
            var asistenciasValidas = new List<AsistenciaData>();

            foreach (var asistencia in asistencias)
            {
                // Completar campos desde configuración si están vacíos
                if (asistencia.clienteId == 0) asistencia.clienteId = _config.clienteId;
                if (string.IsNullOrEmpty(asistencia.usuarioApi)) asistencia.usuarioApi = _config.usuarioApi;
                if (string.IsNullOrEmpty(asistencia.claveApi)) asistencia.claveApi = _config.claveApi;

                // Validar campos requeridos
                if (ValidateAsistencia(asistencia))
                {
                    asistenciasValidas.Add(asistencia);
                }
                else
                {
                    _loggingService.Warning("Asistencia inválida - Asesor: {AsesorNombre}", asistencia.asesorNombre);
                }
            }

            return Task.FromResult(asistenciasValidas.ToArray());
        }

        private async Task<T?> LoadDataFromFile<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _loggingService.Warning("Archivo no encontrado: {FilePath}", filePath);
                    return default;
                }

                var contenido = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(contenido))
                {
                    _loggingService.Warning("Archivo vacío: {FilePath}", filePath);
                    return default;
                }

                return JsonSerializer.Deserialize<T>(contenido);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error al leer archivo {FilePath}: {Message}", filePath, ex.Message);
                return default;
            }
        }

        private static bool ValidateVenta(VentaData venta)
        {
            return !string.IsNullOrEmpty(venta.asesorCedula) &&
                   !string.IsNullOrEmpty(venta.asesorNombre) &&
                   !string.IsNullOrEmpty(venta.facturaNumero) &&
                   !string.IsNullOrEmpty(venta.facturaFecha) &&
                   !string.IsNullOrEmpty(venta.localNombre) &&
                   venta.clienteId > 0 &&
                   venta.valorTransaccion > 0;
        }

        private static bool ValidateAsistencia(AsistenciaData asistencia)
        {
            return !string.IsNullOrEmpty(asistencia.asesorCedula) &&
                   !string.IsNullOrEmpty(asistencia.asesorNombre) &&
                   !string.IsNullOrEmpty(asistencia.fecha) &&
                   !string.IsNullOrEmpty(asistencia.hora) &&
                   !string.IsNullOrEmpty(asistencia.localNombre) &&
                   asistencia.clienteId > 0;
        }
    }
}
