using IntegracionKoach360.Models;

namespace IntegracionKoach360.Interfaces
{
    public interface IApiService
    {
        Task<string?> GetTokenAsync();
        Task<bool> SendVentasAsync(VentaData[] ventas);
        Task<bool> SendAsistenciasAsync(AsistenciaData[] asistencias);
        Task<bool> RenewTokenIfNeededAsync();
    }
}
