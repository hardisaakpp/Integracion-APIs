using System.Text.Json;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;

namespace IntegracionKoach360.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public async Task<ConfiguracionApp?> LoadConfigurationAsync()
        {
            try
            {
                if (!File.Exists("config.json"))
                {
                    Console.WriteLine("Error: Archivo config.json no encontrado");
                    return null;
                }

                var contenido = await File.ReadAllTextAsync("config.json");
                var config = JsonSerializer.Deserialize<ConfiguracionApp>(contenido);
                
                if (config == null)
                {
                    Console.WriteLine("Error: No se pudo deserializar la configuración");
                    return null;
                }

                if (!ValidateConfiguration(config))
                {
                    Console.WriteLine("Error: Configuración inválida");
                    return null;
                }

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar configuración: {ex.Message}");
                return null;
            }
        }

        public bool ValidateConfiguration(ConfiguracionApp config)
        {
            return !string.IsNullOrEmpty(config.usuario) && 
                   !string.IsNullOrEmpty(config.password) && 
                   !string.IsNullOrEmpty(config.baseUrl) &&
                   config.clienteId > 0;
        }
    }
}
