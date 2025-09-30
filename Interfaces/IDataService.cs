using IntegracionKoach360.Models;

namespace IntegracionKoach360.Interfaces
{
    public interface IDataService
    {
        Task<VentaData[]?> LoadVentasAsync();
        Task<AsistenciaData[]?> LoadAsistenciasAsync();
        Task<VentaData[]> ValidateAndCompleteVentasAsync(VentaData[] ventas);
        Task<AsistenciaData[]> ValidateAndCompleteAsistenciasAsync(AsistenciaData[] asistencias);
    }
}
