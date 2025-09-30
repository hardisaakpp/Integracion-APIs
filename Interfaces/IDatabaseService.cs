using IntegracionKoach360.Models;

namespace IntegracionKoach360.Interfaces
{
    public interface IDatabaseService
    {
        Task<VentaData[]> GetVentasAsync();
        Task<AsistenciaData[]> GetAsistenciasAsync();
    }
}