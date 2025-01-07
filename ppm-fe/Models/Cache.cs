namespace ppm_fe.Models
{
    public class Cache
    {
        public User? _currentUser { get; private set; }
        public Dictionary<int, CacheDetail> _cacheDetails;
        public Dictionary<string, int> _homePageCounts;

        public Cache()
        {
            // intialize properties to be filled later in the CacheService
            _cacheDetails = new Dictionary<int, CacheDetail>();
            _homePageCounts = new Dictionary<string, int>();
        }

        public async Task SetCurrentUser(User? user)
        {
            await Task.Run(() =>
            {
                _currentUser = user;

                // Clear cache when user changes
                _cacheDetails.Clear();
                _homePageCounts.Clear();

            });
        }

        public bool IsSuperAdmin => _currentUser?.Role_ID == (int)UserRole.SuperAdmin;
        public bool IsAdmin => _currentUser?.Role_ID == (int)UserRole.Admin;
        public bool IsFestMitarbeiter => _currentUser?.Role_ID == (int)UserRole.FestMitarbeiter;
        public bool IsHonorarkraft => _currentUser?.Role_ID == (int)UserRole.Honorarkraft;
    }

    public class CacheDetail
    {
        public List<Work>? Works { get; set; }
        public List<Work>? AdminWorks { get; set; }
        public List<Billing>? Billings { get; set; }
        public Dictionary<string, List<Billing>> BillingsPerMonth { get; set; } = new Dictionary<string, List<Billing>>();
        public Dictionary<string, List<Work>> AdminWorksPerTeam { get; set; } = new Dictionary<string, List<Work>>();
        public List<Billing>? AdminBillings { get; set; }
        public Dictionary<string, List<Billing>> AdminBillingsPerMonth { get; set; } = new Dictionary<string, List<Billing>>();
    }
}