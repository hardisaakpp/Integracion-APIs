using System.Timers;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;

namespace IntegracionKoach360.Services
{
    public class IntegrationService : IIntegrationService
    {
        private readonly IApiService _apiService;
        private readonly IDataService _dataService;
        private readonly ILoggingService _loggingService;
        private readonly ConfiguracionApp _config;
        private System.Timers.Timer? _timer;

        public IntegrationService(
            IApiService apiService, 
            IDataService dataService, 
            ILoggingService loggingService, 
            ConfiguracionApp config)
        {
            _apiService = apiService;
            _dataService = dataService;
            _loggingService = loggingService;
            _config = config;
        }

        public async Task ExecuteIntegrationAsync()
        {
            try
            {
                _loggingService.Information("========================================");
                _loggingService.Information("Iniciando proceso de integración...");

                // Procesar ventas
                await ProcessVentasAsync();

                // Procesar asistencias
                await ProcessAsistenciasAsync();

                _loggingService.Information("Proceso de integración completado exitosamente");
                _loggingService.Information("========================================");
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error en integración: {Message}", ex.Message);
            }
        }

        public void StartTimer()
        {
            if (_config == null) return;

            // Convertir horas a milisegundos
            double intervaloMs = _config.intervaloHoras * 60 * 60 * 1000;
            
            _timer = new System.Timers.Timer(intervaloMs);
            _timer.Elapsed += async (sender, e) => await ExecuteIntegrationAsync();
            _timer.AutoReset = true;
            _timer.Enabled = true;

            _loggingService.Information("Timer configurado para ejecutar cada {IntervaloHoras} hora(s)", _config.intervaloHoras);
        }

        public void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        private async Task ProcessVentasAsync()
        {
            try
            {
                _loggingService.Information("Procesando ventas...");
                
                var ventas = await _dataService.LoadVentasAsync();
                if (ventas == null || ventas.Length == 0)
                {
                    _loggingService.Information("No se encontraron datos de ventas");
                    return;
                }

                // Validar y completar datos
                var ventasValidas = await _dataService.ValidateAndCompleteVentasAsync(ventas);
                if (ventasValidas.Length == 0)
                {
                    _loggingService.Information("No hay ventas válidas para procesar");
                    return;
                }

                _loggingService.Information("Enviando {Count} venta(s)...", ventasValidas.Length);
                await _apiService.SendVentasAsync(ventasValidas);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error procesando ventas: {Message}", ex.Message);
            }
        }

        private async Task ProcessAsistenciasAsync()
        {
            try
            {
                _loggingService.Information("Procesando asistencias...");
                
                var asistencias = await _dataService.LoadAsistenciasAsync();
                if (asistencias == null || asistencias.Length == 0)
                {
                    _loggingService.Information("No se encontraron datos de asistencias");
                    return;
                }

                // Validar y completar datos
                var asistenciasValidas = await _dataService.ValidateAndCompleteAsistenciasAsync(asistencias);
                if (asistenciasValidas.Length == 0)
                {
                    _loggingService.Information("No hay asistencias válidas para procesar");
                    return;
                }

                _loggingService.Information("Enviando {Count} asistencia(s)...", asistenciasValidas.Length);
                await _apiService.SendAsistenciasAsync(asistenciasValidas);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error procesando asistencias: {Message}", ex.Message);
            }
        }
    }
}
