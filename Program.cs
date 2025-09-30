using System.Net.Http;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;
using IntegracionKoach360.Services;

namespace IntegracionKoach360
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationService? configurationService = null;
            ILoggingService? loggingService = null;
            IIntegrationService? integrationService = null;

            try
            {
                // Inicializar servicios
                configurationService = new ConfigurationService();
                loggingService = new LoggingService();

                // Cargar configuración
                var config = await configurationService.LoadConfigurationAsync();
                if (config == null)
                {
                    Console.WriteLine("Error: No se pudo cargar la configuración");
                    return;
                }

                // Configurar logging
                loggingService.ConfigureLogging(config.logging);

                loggingService.Information("Iniciando IntegracionKoach360...");
                loggingService.Information("Configuración cargada correctamente");
                loggingService.Information("Intervalo de ejecución: cada {IntervaloHoras} hora(s)", config.intervaloHoras);

                // Crear HttpClient
                using var httpClient = new HttpClient();

                // Inicializar servicios restantes
                var apiService = new ApiService(httpClient, loggingService, config);
                var dataService = new DataService(loggingService, config);
                integrationService = new IntegrationService(apiService, dataService, loggingService, config);

                // Ejecutar integración inmediatamente
                await integrationService.ExecuteIntegrationAsync();

                // Configurar timer para ejecución automática
                integrationService.StartTimer();

                // Detectar si se ejecuta como servicio o en modo interactivo
                bool isServiceMode = IsRunningAsService();
                
                if (isServiceMode)
                {
                    loggingService.Information("Aplicación ejecutándose como servicio...");
                    
                    // Mantener la aplicación ejecutándose (modo servicio)
                    while (true)
                    {
                        await Task.Delay(60000); // Esperar 1 minuto
                        // El timer interno maneja la ejecución cada hora
                    }
                }
                else
                {
                    loggingService.Information("Aplicación ejecutándose en modo interactivo. Presione 'q' para salir...");
                    
                    // Mantener la aplicación ejecutándose (modo interactivo)
                    while (true)
                    {
                        try
                        {
                            var key = Console.ReadKey(true);
                            if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                            {
                                break;
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            // Si no hay consola disponible, cambiar a modo servicio
                            loggingService.Information("Consola no disponible, cambiando a modo servicio...");
                            break;
                        }
                    }
                }

                integrationService.StopTimer();
                loggingService.Information("Aplicación finalizada");
            }
            catch (Exception ex)
            {
                if (loggingService != null)
                {
                    loggingService.Fatal(ex, "Error crítico: {Mensaje}", ex.Message);
                }
                else
                {
                    Console.WriteLine($"Error crítico: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            finally
            {
                loggingService?.CloseAndFlush();
            }
        }

        /// <summary>
        /// Detecta si la aplicación se está ejecutando como servicio
        /// </summary>
        /// <returns>True si se ejecuta como servicio, False si es modo interactivo</returns>
        private static bool IsRunningAsService()
        {
            try
            {
                // Verificar si hay una consola disponible
                Console.WindowHeight = Console.WindowHeight;
                return false; // Hay consola, modo interactivo
            }
            catch
            {
                return true; // No hay consola, modo servicio
            }
        }
    }
}