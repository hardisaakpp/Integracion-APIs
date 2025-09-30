using Serilog;
using Serilog.Events;
using IntegracionKoach360.Interfaces;
using IntegracionKoach360.Models;

namespace IntegracionKoach360.Services
{
    public class LoggingService : ILoggingService
    {
        public void ConfigureLogging(ConfiguracionLogging config)
        {
            // Convertir nivel de log
            var nivelMinimo = config.nivelMinimo.ToLower() switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(nivelMinimo)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // Agregar sink de archivo si está habilitado
            if (config.guardarEnArchivo)
            {
                // Crear directorio de logs si no existe
                if (!Directory.Exists(config.rutaArchivoLogs))
                {
                    Directory.CreateDirectory(config.rutaArchivoLogs);
                }

                var rutaArchivo = Path.Combine(config.rutaArchivoLogs, config.nombreArchivo);
                
                loggerConfig.WriteTo.File(
                    rutaArchivo,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: ParseFileSize(config.tamañoMaximoArchivo),
                    retainedFileCountLimit: config.archivosRetenidos,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }

            Log.Logger = loggerConfig.CreateLogger();
        }

        public void Information(string message, params object[] args)
        {
            Log.Information(message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Log.Warning(message, args);
        }

        public void Error(string message, params object[] args)
        {
            Log.Error(message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            Log.Error(exception, message, args);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            Log.Fatal(exception, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            Log.Debug(message, args);
        }

        public void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }

        private static long ParseFileSize(string sizeString)
        {
            if (string.IsNullOrEmpty(sizeString)) return 10 * 1024 * 1024; // 10MB por defecto

            var size = sizeString.ToUpper();
            if (size.EndsWith("KB"))
                return long.Parse(size.Replace("KB", "")) * 1024;
            if (size.EndsWith("MB"))
                return long.Parse(size.Replace("MB", "")) * 1024 * 1024;
            if (size.EndsWith("GB"))
                return long.Parse(size.Replace("GB", "")) * 1024 * 1024 * 1024;
            
            return long.Parse(size);
        }
    }
}
