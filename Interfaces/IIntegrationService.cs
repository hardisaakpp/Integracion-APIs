namespace IntegracionKoach360.Interfaces
{
    public interface IIntegrationService
    {
        Task ExecuteIntegrationAsync();
        void StartTimer();
        void StopTimer();
    }
}
