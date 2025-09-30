using IntegracionKoach360.Models;

namespace IntegracionKoach360.Interfaces
{
    public interface IConfigurationService
    {
        Task<ConfiguracionApp?> LoadConfigurationAsync();
        bool ValidateConfiguration(ConfiguracionApp config);
    }
}
