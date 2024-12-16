using ppm_fe.Models;
using ppm_fe.Services.Interfaces;
using System.Diagnostics;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace ppm_fe.Services
{
    public class CacheService : ICacheService
    {
        // Event für Datenänderungen
        public event Action DataChanged;

        private readonly IWorkService _workService;
        private readonly IBillingService _billingService;

        private Dictionary<int, CacheData> _pageCache; // Cache für jede Seite

        private List<ApiResponse<List<Work>>> _cachedWorks;
        private List<ApiResponse<List<Work>>> _cachedUsersWorks;
        private List<ApiResponse<List<Billing>>> _cachedBillings;
        public Dictionary<string, int> _dashboardStats = new Dictionary<string, int>();

        private DateTime _lastCacheUpdate;
        private const int CACHE_EXPIRATION_MINUTES = 15; // Cache für 15 Minuten gültig

        public CacheService(IWorkService workService, IBillingService billingService)
        {
            _workService = workService;
            _billingService = billingService;

            _cachedWorks = new List<ApiResponse<List<Work>>>();
            _cachedUsersWorks = new List<ApiResponse<List<Work>>>();

            _cachedBillings = new List<ApiResponse<List<Billing>>>();

            _dashboardStats = new Dictionary<string, int>();

            _pageCache = new Dictionary<int, CacheData>();
        }

        public async Task InitializeCacheAsync()
        {
            try
            {
                var worksTask = _workService.GetAllWorks();
                var usersWorksTask = _workService.GetAllUsersWorks();
                var billingsTask = _billingService.GetBillings();
                var totalWorksTask = _workService.GetNumberOfWorks();
                var totalStandingWorksTask = _workService.GetNumberOfStandingWorks();
                var totalBillingsTask = _billingService.GetNumberOfBills();

                // Parallel laden für bessere Performance
                await Task.WhenAll(worksTask, billingsTask, totalWorksTask, totalStandingWorksTask, totalBillingsTask);

                // Cache die paginierten Daten
                var worksResponse = await worksTask;
                if (worksResponse.Success)
                {
                    _cachedWorks = new List<ApiResponse<List<Work>>> { worksResponse };
                }

                var usersWorksResponse = await usersWorksTask;
                if (usersWorksResponse.Success)
                {
                    _cachedUsersWorks = new List<ApiResponse<List<Work>>> { usersWorksResponse };
                }

                var billingsResponse = await billingsTask;
                if (billingsResponse.Success)
                {
                    _cachedBillings = new List<ApiResponse<List<Billing>>> { billingsResponse };
                }

                var totalWorksResponse = await totalWorksTask;
                var totalStandingWorksResponse = await totalStandingWorksTask;
                var totalBillingsResponse = await totalBillingsTask;
                if (totalWorksResponse.Success && totalStandingWorksResponse.Success && totalBillingsResponse.Success)
                {
                    _dashboardStats["totalWorks"] = totalWorksResponse.Data;
                    _dashboardStats["standingWorks"] = totalStandingWorksResponse.Data;
                    _dashboardStats["totalBillings"] = totalBillingsResponse.Data;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cache initialization failed: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Work>> GetWorksPageFromCache(int page, int perPage = 10)
        {
            // Prüfe ob die Seite im Cache ist und noch gültig
            if (_pageCache.ContainsKey(page) &&
                DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
            {
                if (_pageCache[page].works != null)
                {
                    return _pageCache[page].works;
                }
            }

            // Wenn nicht im Cache, von API holen
            var result = await _workService.GetAllWorks(page, perPage);
            if (result?.Data != null)
            {
                if (_pageCache.ContainsKey(page) && _pageCache[page] != null)
                {
                    _pageCache[page].works = result.Data;
                }
                else
                {

                    CacheData cache = new CacheData { works = result.Data };
                    _pageCache[page] = cache;
                }

                _lastCacheUpdate = DateTime.Now;
                return result.Data;
            }

            return new List<Work>();
        }

        public async Task<List<Work>> GetUsersWorksPageFromCache(int page, int perPage = 10)
        {
            // Prüfe ob die Seite im Cache ist und noch gültig
            if (_pageCache.ContainsKey(page) &&
                DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
            {
                if (_pageCache[page].usersWorks != null)
                {
                    return _pageCache[page].usersWorks;
                }
            }

            // Wenn nicht im Cache, von API holen
            var result = await _workService.GetAllUsersWorks(page, perPage);
            if (result?.Data != null)
            {
                if (_pageCache.ContainsKey(page) && _pageCache[page] != null)
                {
                    _pageCache[page].usersWorks = result.Data;
                }
                else
                {

                    CacheData cache = new CacheData { usersWorks = result.Data };
                    _pageCache[page] = cache;
                }

                _lastCacheUpdate = DateTime.Now;
                return result.Data;
            }

            return new List<Work>();
        }

        public async Task<List<Billing>> GetBillingsPageFromCache(int page, int perPage = 10)
        {
            // Prüfe ob die Seite im Cache ist und noch gültig
            if (_pageCache.ContainsKey(page) &&
                DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
            {
                if (_pageCache[page].billings != null)
                {
                    return _pageCache[page].billings;
                }
            }

            // Wenn nicht im Cache, von API holen
            var result = await _billingService.GetBillings(page, perPage);
            if (result?.Data != null)
            {
                if (_pageCache.ContainsKey(page) && _pageCache[page] != null)
                {
                    _pageCache[page].billings = result.Data;
                }
                else
                {

                    CacheData cache = new CacheData { billings = result.Data };
                    _pageCache[page] = cache;
                }

                _lastCacheUpdate = DateTime.Now;
                return result.Data;
            }

            return new List<Billing>();
        }

        public async Task<List<Billing>> GetBillingsPerMonthPageFromCache(string month, int page, int perPage = 10)
        {
            // Prüfe ob die Seite im Cache ist und noch gültig
            if (_pageCache.ContainsKey(page) &&
                DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
            {
                if (_pageCache[page].billingsPerMonth != null && _pageCache[page].billingsPerMonth.ContainsKey(month))
                {
                    return _pageCache[page].billingsPerMonth[month];
                }
            }

            // Wenn nicht im Cache, von API holen
            var result = await _billingService.FetchBillsPerMonth(month, page, perPage);
            if (result?.Data != null)
            {
                if (_pageCache.ContainsKey(page) && _pageCache[page] != null)
                {
                    _pageCache[page].billingsPerMonth[month] = result.Data;
                }
                else
                {
                    CacheData cache = new CacheData { };
                    cache.billingsPerMonth = new Dictionary<string, List<Billing>>
                    {
                        { month, new List<Billing>(result?.Data) }
                    };
                    _pageCache[page] = cache;
                }

                _lastCacheUpdate = DateTime.Now;
                return result.Data;
            }

            return new List<Billing>();
        }

        public async Task<Dictionary<string, int>> GetAdminDashboardStats()
        {
            // Prüfe ob die Seite im Cache ist und noch gültig
            if (_dashboardStats.ContainsKey("totalWorks")
                && _dashboardStats.ContainsKey("standingWorks")
                && _dashboardStats.ContainsKey("totalBillings")
                && DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
            {
                return _dashboardStats;
            }

            var totalWorksTask = _workService.GetNumberOfWorks();
            var totalStandingWorksTask = _workService.GetNumberOfStandingWorks();
            var totalBillingsTask = _billingService.GetNumberOfBills();

            // Parallel laden für bessere Performance
            await Task.WhenAll(totalWorksTask, totalStandingWorksTask);

            // Cache die  Daten
            var totalWorksResponse = await totalWorksTask;
            var totalStandingWorksResponse = await totalStandingWorksTask;
            var totalBillingsResponse = await totalBillingsTask;

            if (totalWorksResponse.Success && totalStandingWorksResponse.Success && totalBillingsResponse.Success)
            {
                _dashboardStats["totalWorks"] = totalWorksResponse.Data;
                _dashboardStats["standingWorks"] = totalStandingWorksResponse.Data;
                _dashboardStats["totalBillings"] = totalBillingsResponse.Data;
                _lastCacheUpdate = DateTime.Now;
            }
            return _dashboardStats;
        }

        public async Task RefreshWorksCache()
        {
            try
            {
                // Erste Seite abrufen
                var firstPageResult = await _workService.GetAllWorks(1, 10);

                if (_pageCache.ContainsKey(1) && _pageCache[1] != null)
                {
                    _pageCache[1].works = firstPageResult.Data;
                }
                else
                {

                    CacheData cache = new CacheData { works = firstPageResult.Data };
                    _pageCache[1] = cache;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing cache: {ex.Message}");
                throw;
            }
        }

        public async Task RefreshBillingsCache()
        {
            try
            {
                var billingsResponse = await _billingService.GetBillings();
                if (billingsResponse.Success)
                {
                    if (_pageCache.ContainsKey(1) && _pageCache[1] != null)
                    {
                        _pageCache[1].billings = billingsResponse.Data;
                    }
                    else
                    {

                        CacheData cache = new CacheData { billings = billingsResponse.Data };
                        _pageCache[1] = cache;
                    }
                }
                else
                {
                    throw new Exception("Failed to refresh billings cache");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing billings cache: {ex.Message}");
                throw;
            }
        }

        public async Task RefreshDashboardCache()
        {
            try
            {
                var worksCountResponse = await _workService.GetNumberOfWorks();
                var standingWorksCountResponse = await _workService.GetNumberOfStandingWorks();
                var billingsCountResponse = await _billingService.GetNumberOfBills();

                if (worksCountResponse.Success && standingWorksCountResponse.Success && billingsCountResponse.Success)
                {
                    _dashboardStats["totalWorks"] = worksCountResponse.Data;
                    _dashboardStats["standingWorks"] = standingWorksCountResponse.Data;
                    _dashboardStats["totalBillings"] = billingsCountResponse.Data;
                    _lastCacheUpdate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing billings cache: {ex.Message}");
                throw;
            }
        }

        public bool IsCacheValid()
        {
            return _cachedWorks != null && _cachedWorks.Any() &&
                   _cachedUsersWorks != null && _cachedUsersWorks.Any() &&
                   _cachedBillings != null && _cachedBillings.Any() &&
                   _dashboardStats != null && _dashboardStats.Any() &&
                   DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES;
        }

        public void ClearCache()
        {
            _cachedWorks = new List<ApiResponse<List<Work>>>();
            _cachedUsersWorks = new List<ApiResponse<List<Work>>>();
            _cachedBillings = new List<ApiResponse<List<Billing>>>();
            _dashboardStats.Clear();
            _pageCache.Clear();
            _lastCacheUpdate = DateTime.MinValue;
        }
    }

    public static class MessageKeys
    {
        public const string WorksChanged = "WorksChanged";
        public const string BillingsChanged = "BillingsChanged";
    }
}
