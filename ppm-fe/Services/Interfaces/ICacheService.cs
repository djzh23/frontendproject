using ppm_fe.Models;

namespace ppm_fe.Services.Interfaces
{
    public interface ICacheService
    {
        //event Action WorksChanged;
        event Action DataChanged;


        Task InitializeCacheAsync();

        Task<List<Work>> GetWorksPageFromCache(int page, int perPage = 10);

        Task<List<Work>> GetUsersWorksPageFromCache(int page, int perPage = 10);

        Task<List<Billing>> GetBillingsPageFromCache(int page, int perPage = 10);

        Task<List<Billing>> GetBillingsPerMonthPageFromCache(string month, int page, int perPage = 10);

        Task<Dictionary<string, int>> GetAdminDashboardStats();

        Task RefreshWorksCache();

        Task RefreshBillingsCache();

        Task RefreshDashboardCache();

        bool IsCacheValid();

        void ClearCache();




        //List<ApiResponse<List<Work>>> GetCachedWorks();

        //Task<List<Work>> GetCachedWorksAsync();


        //bool IsCacheValid();
        //List<ApiResponse<List<Billing>>> GetCachedBillings();

        //// Neue Methoden
        //int GetNumberOfWorks();
        //int GetNumberOfStandingWorks();

        //void ClearCache();
        //Task RefreshWorksCache();

        //int GetTotalNumberOfWorks();
        //int GetTotalNumberOfStandingWorks();
        //Task RefreshBillingsCache();
    }
}
