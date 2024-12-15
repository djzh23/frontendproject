using ppm_fe.Models;
using ppm_fe.Services.Interfaces;
using System.Diagnostics;

namespace ppm_fe.Services
{
    public class CacheService : ICacheService
    {
        // Event für Datenänderungen
        public event Action DataChanged;

        private readonly IBillingService _billingService;

        private Dictionary<int, CacheData> _pageCache; // Cache für jede Seite
        private List<ApiResponse<List<Billing>>> _cachedBillings;
        public Dictionary<string, int> _dashboardStats = new Dictionary<string, int>();

        private DateTime _lastCacheUpdate;
        private const int CACHE_EXPIRATION_MINUTES = 15; // Cache für 15 Minuten gültig

        public CacheService(IBillingService billingService)
        {
            _billingService = billingService;

            _cachedBillings = new List<ApiResponse<List<Billing>>>();

            _dashboardStats = new Dictionary<string, int>();

            _pageCache = new Dictionary<int, CacheData>();
        }

        public async Task InitializeCacheAsync()
        {
            try
            {
                var billingsTask = _billingService.GetBillings();
                var totalBillingsTask = _billingService.GetNumberOfBills();

                // Parallel laden für bessere Performance
                await Task.WhenAll(billingsTask, totalBillingsTask);

                // Cache die paginierten Daten
                var billingsResponse = await billingsTask;
                if (billingsResponse.Success)
                {
                    _cachedBillings = new List<ApiResponse<List<Billing>>> { billingsResponse };
                }

                var totalBillingsResponse = await totalBillingsTask;
                if (totalBillingsResponse.Success)
                {
                    _dashboardStats["totalBillings"] = totalBillingsResponse.Data;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cache initialization failed: {ex.Message}");
                throw;
            }
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
            if (_dashboardStats.ContainsKey("totalBillings")
                && DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
            {
                return _dashboardStats;
            }

            var totalBillingsTask = _billingService.GetNumberOfBills();

            // Parallel laden für bessere Performance
            await Task.WhenAll(totalBillingsTask);

            // Cache die  Daten
            var totalBillingsResponse = await totalBillingsTask;

            if (totalBillingsResponse.Success)
            {
                _dashboardStats["totalBillings"] = totalBillingsResponse.Data;
                _lastCacheUpdate = DateTime.Now;
            }
            return _dashboardStats;
        }

        public async Task RefreshBillingsCache()
        {
            try
            {
                var billingsResponse = await _billingService.GetBillings();
                if (billingsResponse.Success)
                {
                    _cachedBillings = new List<ApiResponse<List<Billing>>> { billingsResponse };
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
                var billingsCountResponse = await _billingService.GetNumberOfBills();

                if (billingsCountResponse.Success)
                {
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
            return  _cachedBillings != null && _cachedBillings.Any() &&
                   _dashboardStats != null && _dashboardStats.Any() &&
                   DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes <= CACHE_EXPIRATION_MINUTES;
        }

        public void ClearCache()
        {
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
