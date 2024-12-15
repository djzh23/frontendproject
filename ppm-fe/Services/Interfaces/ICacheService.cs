using ppm_fe.Models;

namespace ppm_fe.Services.Interfaces
{
    public interface ICacheService
    {
        //event Action WorksChanged;
        event Action DataChanged;


        Task InitializeCacheAsync();

        Task<List<Billing>> GetBillingsPageFromCache(int page, int perPage = 10);

        Task<List<Billing>> GetBillingsPerMonthPageFromCache(string month, int page, int perPage = 10);

        Task<Dictionary<string, int>> GetAdminDashboardStats();

        Task RefreshBillingsCache();

        Task RefreshDashboardCache();

        bool IsCacheValid();

        void ClearCache();
    }
}
