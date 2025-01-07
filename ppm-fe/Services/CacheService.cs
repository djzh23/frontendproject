using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Helpers;
using ppm_fe.Messages;
using ppm_fe.Models;
using ppm_fe.Services.Interfaces;

namespace ppm_fe.Services
{
    public class CacheService : ICacheService
    {
        private readonly IWorkService _workService;
        private readonly IBillingService _billingService;

        public Cache? _cache;
        private DateTime _lastCacheUpdateAt;
        private const int CACHE_EXPIRATION_MINUTES = 15; // Cache expiration time

        public CacheService(IWorkService workService, IBillingService billingService)
        {
            _workService = workService;
            _billingService = billingService;

            if(_cache == null)
            {
                _cache = new Cache();
            }

            RegisterChangeMessages();
        }

        #region Fetching Caches
        // Meine Einsatze
        public async Task<List<Work>> GetWorksPageFromCache(int page, int perPage = 10)
        {
            try
            {
                if(_cache == null)
                {
                    return new List<Work>();
                }

                if (!_cache.IsHonorarkraft && !_cache.IsFestMitarbeiter && !_cache.IsAdmin)
                {
                    return new List<Work>();
                }

                // Check if the page exists in cache and still not expired
                if (_cache._cacheDetails.ContainsKey(page) &&
                    DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page] != null && _cache._cacheDetails[page].Works != null)
                    {
                        return _cache._cacheDetails[page].Works ?? new List<Work>();
                    }
                }

                // If not existing in cache, then fetching from the API
                var result = await _workService.GetAllWorks(page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].Works = result.Data;
                    }
                    else
                    {
                        CacheDetail cacheDetail = new CacheDetail { Works = result.Data };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetWorksPageFromCache), $"Fetching works from cache failed: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new List<Work>();
        }

        public async Task<List<Work>> GetAdminWorksPageFromCache(int page, int perPage = 10)
        {
            try
            {
                if (_cache == null)
                {
                    return new List<Work>();
                }

                if (!_cache.IsAdmin)
                {
                    return new List<Work>();
                }

                if (_cache._cacheDetails.ContainsKey(page) && DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page].AdminWorks != null)
                    {
                        return _cache._cacheDetails[page].AdminWorks ?? new List<Work>();
                    }
                }

                var result = await _workService.GetAllUsersWorks(page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].AdminWorks = result.Data;
                    }
                    else
                    {
                        CacheDetail cacheDetail = new CacheDetail { AdminWorks = result.Data };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetAdminWorksPageFromCache), $"Fetching admin works failed: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new List<Work>();
        }

        public async Task<List<Work>> GetWorksPerTeamPageFromCache(string team, int page, int perPage = 10)
        {
            try
            {
                if (_cache == null)
                {
                    return new List<Work>();
                }

                if (!_cache.IsHonorarkraft && !_cache.IsFestMitarbeiter && !_cache.IsAdmin)
                {
                    return new List<Work>();
                }

                if (_cache._cacheDetails.ContainsKey(page) &&
                DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page].AdminWorksPerTeam != null && _cache._cacheDetails[page].AdminWorksPerTeam.ContainsKey(team))
                    {
                        return _cache._cacheDetails[page].AdminWorksPerTeam[team];
                    }
                }

                var result = await _workService.FetchWorksPerTeam(team, page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].AdminWorksPerTeam[team] = result.Data;
                    }
                    else
                    {
                        List<Work> items = new List<Work>();
                        if (result?.Data != null)
                        {
                            items = result.Data;
                        }

                        CacheDetail cacheDetail = new CacheDetail { };
                        cacheDetail.AdminWorksPerTeam = new Dictionary<string, List<Work>>
                        {
                            { team, new List<Work>(items) }
                        };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result?.Data ?? new List<Work>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetWorksPerTeamPageFromCache), $"Fetching works per team failed: {ex.Message}", new { Team = team, Page = page, PerPage = perPage }, ex.StackTrace);
            }


            return new List<Work>();
        }

        public async Task<List<Billing>> GetBillingsPageFromCache(int page, int perPage = 10)
        {
            try
            {
                if (_cache == null)
                {
                    return new List<Billing>();
                }

                if (!_cache.IsHonorarkraft)
                {
                    return new List<Billing>();
                }

                if (_cache._cacheDetails.ContainsKey(page) &&
                    DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page].Billings != null)
                    {
                        return _cache._cacheDetails[page].Billings ?? new List<Billing>();
                    }
                }

                var result = await _billingService.GetBillings(page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].Billings = result.Data;
                    }
                    else
                    {

                        CacheDetail cacheDetail = new CacheDetail { Billings = result.Data };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetBillingsPageFromCache), $"Fetching billings failed: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new List<Billing>();
        }

        public async Task<List<Billing>> GetBillingsPerMonthPageFromCache(string month, int page, int perPage = 10)
        {
            try
            {
                if (_cache == null)
                {
                    return new List<Billing>();
                }

                if (!_cache.IsHonorarkraft)
                {
                    return new List<Billing>();
                }

                if (_cache._cacheDetails.ContainsKey(page) &&
                    DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page].BillingsPerMonth != null && _cache._cacheDetails[page].BillingsPerMonth.ContainsKey(month))
                    {
                        return _cache._cacheDetails[page].BillingsPerMonth[month];
                    }
                }

                var result = await _billingService.FetchBillsPerMonth(month, page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].BillingsPerMonth[month] = result.Data;
                    }
                    else
                    {
                        List<Billing> items = new List<Billing>();
                        if (result?.Data != null)
                        {
                            items = result.Data;
                        }

                        CacheDetail cacheDetail = new CacheDetail { };
                        cacheDetail.BillingsPerMonth = new Dictionary<string, List<Billing>>
                        {
                            { month, new List<Billing>(items) }
                        };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result?.Data ?? new List<Billing>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetBillingsPerMonthPageFromCache), $"Fetching billings per month failed: {ex.Message}", new { Month = month, Page = page, PerPage = perPage }, ex.StackTrace);
            }

            return new List<Billing>();
        }

        public async Task<List<Billing>> GetAdminBillingsPageFromCache(int page, int perPage = 10)
        {
            try
            {
                if (_cache == null)
                {
                    return new List<Billing>();
                }

                if (!_cache.IsAdmin)
                {
                    return new List<Billing>();
                }

                if (_cache._cacheDetails.ContainsKey(page) &&
                    DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page].AdminBillings != null)
                    {
                        return _cache._cacheDetails[page].AdminBillings ?? new List<Billing>();
                    }
                }

                // Wenn nicht im Cache, von API holen
                var result = await _billingService.GetAdminBillings(page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].AdminBillings = result.Data;
                    }
                    else
                    {

                        CacheDetail cacheDetail = new CacheDetail { AdminBillings = result.Data };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result.Data;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetAdminBillingsPageFromCache), $"Fetching admin billings failed: {ex.Message}", new { Page = page, PerPage = perPage }, ex.StackTrace);
            }


            return new List<Billing>();
        }

        public async Task<List<Billing>> GetAdminBillingsPerMonthPageFromCache(string month, int page, int perPage = 10)
        {
            try
            {
                if (_cache == null)
                {
                    return new List<Billing>();
                }

                if (!_cache.IsAdmin)
                {
                    return new List<Billing>();
                }

                if (_cache._cacheDetails.ContainsKey(page) &&
                    DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    if (_cache._cacheDetails[page].AdminBillingsPerMonth != null && _cache._cacheDetails[page].AdminBillingsPerMonth.ContainsKey(month))
                    {
                        return _cache._cacheDetails[page].AdminBillingsPerMonth[month];
                    }
                }

                var result = await _billingService.FetchAdminBillsPerMonth(month, page, perPage);
                if (result?.Data != null)
                {
                    if (_cache._cacheDetails.ContainsKey(page) && _cache._cacheDetails[page] != null)
                    {
                        _cache._cacheDetails[page].AdminBillingsPerMonth[month] = result.Data;
                    }
                    else
                    {
                        List<Billing> items = new List<Billing>();
                        if (result?.Data != null)
                        {
                            items = result.Data;
                        }

                        CacheDetail cacheDetail = new CacheDetail { };
                        cacheDetail.AdminBillingsPerMonth = new Dictionary<string, List<Billing>>
                        {
                            { month, new List<Billing>(items) }
                        };
                        _cache._cacheDetails[page] = cacheDetail;
                    }

                    _lastCacheUpdateAt = DateTime.Now;
                    return result?.Data ?? new List<Billing>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetAdminBillingsPerMonthPageFromCache), $"Fetching admin billings per month failed: {ex.Message}", new { Month = month, Page = page, PerPage = perPage }, ex.StackTrace);
            }


            return new List<Billing>();
        }

        public async Task<Dictionary<string, int>> GetAdminHomePageCounts()
        {
            try
            {
                if (_cache == null)
                {
                    return new Dictionary<string, int>();
                }

                if (_cache._homePageCounts.ContainsKey("totalWorks")
                    && _cache._homePageCounts.ContainsKey("incompleteWorks")
                    && _cache._homePageCounts.ContainsKey("totalBillings")
                    && DateTime.Now.Subtract(_lastCacheUpdateAt).TotalMinutes <= CACHE_EXPIRATION_MINUTES)
                {
                    return _cache._homePageCounts;
                }

                if (_cache.IsHonorarkraft)
                {
                    var totalBillingsTask = _billingService.GetNumberOfBills();
                    var totalBillingsResponse = await totalBillingsTask; 
                    if (totalBillingsResponse.Success)
                    {
                        _cache._homePageCounts["totalBillings"] = totalBillingsResponse.Data;
                        _lastCacheUpdateAt = DateTime.Now;
                    }
                }

                if (_cache.IsHonorarkraft || _cache.IsFestMitarbeiter || _cache.IsAdmin)
                {
                    var totalWorksTask = _workService.GetNumberOfWorks();
                    var totaIncompleteWorksTask = _workService.GetNumberOfIncompleteWorks();

                    await Task.WhenAll(totalWorksTask, totaIncompleteWorksTask);

                    var totalWorksResponse = await totalWorksTask;
                    var totalIncompleteWorksResponse = await totaIncompleteWorksTask;

                    if (totalWorksResponse.Success && totalIncompleteWorksResponse.Success)
                    {
                        _cache._homePageCounts["totalWorks"] = totalWorksResponse.Data;
                        _cache._homePageCounts["incompleteWorks"] = totalIncompleteWorksResponse.Data;
                        _lastCacheUpdateAt = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetAdminHomePageCounts), $"Fetching admin home page counts failed: {ex.Message}", new { }, ex.StackTrace);
            }

            return _cache?._homePageCounts ?? new Dictionary<string, int>(); ;
        }

        #endregion

        #region refresh caches
        public async Task RefreshWorksCache()
        {
            try
            {
                if (_cache == null) 
                {
                    return;
                }

                if (!_cache.IsHonorarkraft && !_cache.IsFestMitarbeiter && !_cache.IsAdmin)
                {
                    return;
                }

                // Refresh works by getting the list of 10 works for page 1
                var firstPageResult = await _workService.GetAllWorks(1, 10);

                if (_cache._cacheDetails.ContainsKey(1) && _cache._cacheDetails[1] != null)
                {
                    _cache._cacheDetails[1].Works = firstPageResult.Data;
                }
                else
                {
                    CacheDetail cacheDetail = new CacheDetail { Works = firstPageResult.Data };
                    _cache._cacheDetails[1] = cacheDetail;
                }

                if (firstPageResult?.Pagination != null)
                {
                    for (int i = 2; i <= firstPageResult?.Pagination.LastPage; i++)
                    {
                        var result = await _workService.GetAllWorks(i, 10);
                        if (_cache._cacheDetails.ContainsKey(i) && _cache._cacheDetails[i] != null)
                        {
                            _cache._cacheDetails[i].Works = result.Data;
                        }
                        else
                        {
                            CacheDetail cacheDetail = new CacheDetail { Works = result.Data };
                            _cache._cacheDetails[i] = cacheDetail;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(RefreshWorksCache), $"Error refreshing cache: {ex.Message}", new { }, ex.StackTrace);
            }
        }

        public async Task RefreshBillingsCache()
        {
            try
            {
                if (_cache == null)
                {
                    return;
                }

                if (!_cache.IsHonorarkraft)
                {
                    return;
                }

                var billingsResponse = await _billingService.GetBillings();
                if (billingsResponse.Success)
                {
                    if (_cache._cacheDetails.ContainsKey(1) && _cache._cacheDetails[1] != null)
                    {
                        _cache._cacheDetails[1].Billings = billingsResponse.Data;
                    }
                    else
                    {

                        CacheDetail cacheDetail = new CacheDetail { Billings = billingsResponse.Data };
                        _cache._cacheDetails[1] = cacheDetail;
                    }
                }
                else
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(RefreshBillingsCache), "Failed to refresh billings cache", new {  }, string.Empty);
                }

                foreach (var CacheData in _cache._cacheDetails.Values)
                {
                    CacheData.BillingsPerMonth = new Dictionary<string, List<Billing>>();
                    CacheData.AdminBillingsPerMonth = new Dictionary<string, List<Billing>>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(RefreshBillingsCache), $"Error refreshing billings cache: {ex.Message}", new {  }, ex.StackTrace);
            }
        }

        public async Task RefreshHomePageCache()
        {
            try
            {
                if (_cache == null)
                {
                    return;
                }

                if (_cache.IsHonorarkraft)
                {
                    var totalBillingsTask = _billingService.GetNumberOfBills();
                    var totalBillingsResponse = await totalBillingsTask;
                    if (totalBillingsResponse.Success)
                    {
                        _cache._homePageCounts["totalBillings"] = totalBillingsResponse.Data;
                        _lastCacheUpdateAt = DateTime.Now;
                    }
                }

                if (_cache.IsHonorarkraft || _cache.IsFestMitarbeiter || _cache.IsAdmin)
                {
                    var totalWorksTask = _workService.GetNumberOfWorks();
                    var totaIncompleteWorksTask = _workService.GetNumberOfIncompleteWorks();

                    await Task.WhenAll(totalWorksTask, totaIncompleteWorksTask);

                    var totalWorksResponse = await totalWorksTask;
                    var totalIncompleteWorksResponse = await totaIncompleteWorksTask;

                    if (totalWorksResponse.Success && totalIncompleteWorksResponse.Success)
                    {
                        _cache._homePageCounts["totalWorks"] = totalWorksResponse.Data;
                        _cache._homePageCounts["incompleteWorks"] = totalIncompleteWorksResponse.Data;
                        _lastCacheUpdateAt = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(RefreshHomePageCache), $"Error refreshing billings cache: {ex.Message}", new { }, ex.StackTrace);
                throw;
            }
        }
        #endregion

        public void ClearCache()
        {
            if (_cache != null)
            {
                _cache._cacheDetails.Clear();
                _cache._homePageCounts.Clear();
                _lastCacheUpdateAt = DateTime.MinValue;
            }
        }

        public void RegisterChangeMessages()
        {
            // Register to receive BillingMessages
            WeakReferenceMessenger.Default.Register<BillingMessage>(this, async (r, message) =>
            {
                if (message.RefreshCache)
                {
                    await RefreshBillingsCache();
                }
            });

            // Register to receive WorkMessages
            WeakReferenceMessenger.Default.Register<WorkMessage>(this, async (r, message) =>
            {
                if (message.RefreshCache)
                {
                    await RefreshWorksCache();
                }
            });

            // Register to receive HomePageMessages
            WeakReferenceMessenger.Default.Register<HomePageMessage>(this, async (r, message) =>
            {
                if (message.RefreshCache)
                {
                    await RefreshHomePageCache();
                }
            });

            // Register to receive UserMessages
            WeakReferenceMessenger.Default.Register<UserMessage>(this, async (r, message) =>
            {
                if(_cache != null)
                {
                    await _cache.SetCurrentUser(message.User);
                }
                else
                {
                    _cache = new Cache();
                    await _cache.SetCurrentUser(message.User);
                }
                
            });
        }
    }
}
