namespace ppm_fe.Services
{
    public interface ILocalPathService
    {
        string LocalDocumentsFolder { get; }
        string LocalBillsFolder { get; }
        string LocalWorksFolder { get; }
        string LocalWorksForAdminFolder { get; }
        string LocalWorksForOtherUsersFolder { get; }
    }

}
