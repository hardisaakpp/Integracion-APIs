using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IntegracionKoach360.Models
{
    public class ConfiguracionApp
    {
        public string usuario { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int clienteId { get; set; }
        public string usuarioApi { get; set; } = string.Empty;
        public string claveApi { get; set; } = string.Empty;
        public int intervaloHoras { get; set; } = 1;
        public string baseUrl { get; set; } = string.Empty;
        
        public ConfiguracionDatabase database { get; set; } = new ConfiguracionDatabase();
        public ConfiguracionLogging logging { get; set; } = new ConfiguracionLogging();
    }

    public class ConfiguracionDatabase
    {
        public string connectionString { get; set; } = string.Empty;
        public int commandTimeout { get; set; } = 120;
    }

    public class ConfiguracionLogging
    {
        public string nivelMinimo { get; set; } = "Information";
        public bool guardarEnArchivo { get; set; } = true;
        public string rutaArchivoLogs { get; set; } = "logs";
        public string nombreArchivo { get; set; } = "integracion-koach360-{Date}.log";
        public string tamañoMaximoArchivo { get; set; } = "10MB";
        public int archivosRetenidos { get; set; } = 30;
        public bool mostrarEnConsola { get; set; } = true;
    }
}
