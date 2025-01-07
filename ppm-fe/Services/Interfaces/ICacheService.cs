using ppm_fe.Models;

namespace ppm_fe.Services.Interfaces
{
    public interface ICacheService
    {
        Task<List<Work>> GetWorksPageFromCache(int page, int perPage = 10);

        Task<List<Work>> GetAdminWorksPageFromCache(int page, int perPage = 10);

        Task<List<Work>> GetWorksPerTeamPageFromCache(string team, int page, int perPage = 10);

        Task<List<Billing>> GetBillingsPageFromCache(int page, int perPage = 10);

        Task<List<Billing>> GetBillingsPerMonthPageFromCache(string month, int page, int perPage = 10);

        Task<List<Billing>> GetAdminBillingsPageFromCache(int page, int perPage = 10);

        Task<List<Billing>> GetAdminBillingsPerMonthPageFromCache(string month, int page, int perPage = 10);

        Task<Dictionary<string, int>> GetAdminHomePageCounts();

        Task RefreshWorksCache();

        Task RefreshBillingsCache();

        Task RefreshHomePageCache();

        void ClearCache();

        void RegisterChangeMessages();
    }
}
