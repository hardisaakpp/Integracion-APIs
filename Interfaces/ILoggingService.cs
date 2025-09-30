using IntegracionKoach360.Models;

namespace IntegracionKoach360.Interfaces
{
    public interface ILoggingService
    {
        void ConfigureLogging(ConfiguracionLogging config);
        void Information(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(string message, params object[] args);
        void Error(Exception exception, string message, params object[] args);
        void Fatal(Exception exception, string message, params object[] args);
        void Debug(string message, params object[] args);
        void CloseAndFlush();
    }
}
