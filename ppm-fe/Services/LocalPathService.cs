using ppm_fe.Helpers;
using ppm_fe.Models;

namespace ppm_fe.Services
{

    public class LocalPathService : ILocalPathService
    {
        private readonly string? _localDocumentsFolder;
        private readonly string? _localBillsFolder;
        private readonly string? _localWorksFolder;
        private readonly string? _localWorksForAdminFolder;
        private readonly string? _localWorksForOtherUsersFolder;

        public LocalPathService()
        {
            _localDocumentsFolder = "";
            _localBillsFolder = "";
            _localWorksFolder = "";
            _localWorksForAdminFolder = "";
            _localWorksForOtherUsersFolder = "";
#if ANDROID

            var externalStorageDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);

            if (externalStorageDir != null)
            {
                var documentsDirectory = externalStorageDir.AbsolutePath;
                var mainFolder = Path.Combine(documentsDirectory, "PPM");

                if(App.UserDetails.Role_ID == 2)
                {
                    _localWorksFolder = Path.Combine(mainFolder, "Einsätze");
                    Directory.CreateDirectory(_localWorksFolder);
                }

                _localBillsFolder = Path.Combine(mainFolder, "Rechnungen");
                Directory.CreateDirectory(_localBillsFolder);

                _localDocumentsFolder = Path.Combine(documentsDirectory, "PPM");
            }
            else
            {
                // Handle the case where external storage is not available
                Console.WriteLine("Error: External storage not available.");
                LoggerHelper.LogError(this.GetType().Name, nameof(LocalPathService), "Error: External storage not available.", new { externalStorageDir }, null);
            }
#elif IOS

        var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //_localDocumentsFolder = Path.Combine(documentsDirectory, "PPM");

                        // Create the directories
                Directory.CreateDirectory(_localWorksFolder);
                Directory.CreateDirectory(_localBillsFolder);

         var mainFolder = Path.Combine(documentsDirectory, "PPM");
         _localWorksFolder = Path.Combine(mainFolder, "Einsätze");
         _localBillsFolder = Path.Combine(mainFolder, "Rechnungen");
#else
            var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _localDocumentsFolder = Path.Combine(documentsDirectory, "PPM");
#endif
            // create local folders to store documents
            if (!string.IsNullOrWhiteSpace(_localDocumentsFolder))
            {
                if (!Directory.Exists(_localDocumentsFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(_localDocumentsFolder);
                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it appropriately
                        LoggerHelper.LogError(this.GetType().Name, nameof(LocalPathService), $"Error creating directory: {ex.Message}", new { _localDocumentsFolder }, null);
                    }
                }
            }
            else
            {
                // Handle the case where _localDocumentsFolder is null or empty
                LoggerHelper.LogError(this.GetType().Name, nameof(LocalPathService), "Local documents folder path is null or empty.", new { _localDocumentsFolder }, null);
            }

            // create subfolders for user Admin
            if (App.UserDetails.Role_ID == (int)UserRole.Admin)
            {
                // Create subfolders for Bills
                _localBillsFolder = Path.Combine(_localDocumentsFolder, "Rechnungen");
                // Create subfolders if they don't exist
                if (!Directory.Exists(_localBillsFolder))
                {
                    Directory.CreateDirectory(_localBillsFolder);
                }

                // Create subfolders for Works
                _localWorksFolder = Path.Combine(_localDocumentsFolder, "Einsätze");
                // Create subfolders if they don't exist
                if (!Directory.Exists(_localWorksFolder))
                {
                    Directory.CreateDirectory(_localWorksFolder);
                }

                // create separate folders for admin works and all other users works
                _localWorksForAdminFolder = Path.Combine(_localWorksFolder, "Meine Einsätze");
                if (!Directory.Exists(_localWorksForAdminFolder))
                {
                    Directory.CreateDirectory(_localWorksForAdminFolder);
                }
                _localWorksForOtherUsersFolder = Path.Combine(_localWorksFolder, "Einsätze anderer Benutzer");
                if (!Directory.Exists(_localWorksForOtherUsersFolder))
                {
                    Directory.CreateDirectory(_localWorksForOtherUsersFolder);
                }
            }
            else // create only billing folder for other users
            {
                // Create subfolders for Bills
                _localBillsFolder = Path.Combine(_localDocumentsFolder, "Rechnungen");

                // Create subfolders if they don't exist
                if (!Directory.Exists(_localBillsFolder))
                {
                    Directory.CreateDirectory(_localBillsFolder);
                }
            }
        }


        public string LocalDocumentsFolder
        {
            get
            {
                if (_localDocumentsFolder != null)
                {
                    return _localDocumentsFolder;
                }
                else
                {
                    throw new InvalidOperationException("Local documents folder is not available.");
                }
            }
        }
        public string LocalBillsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_localBillsFolder))
                {
                    throw new InvalidOperationException("Local bills folder is not initialized.");
                }
                return _localBillsFolder;
            }
        }

        public string LocalWorksFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_localWorksFolder) && App.UserDetails.Role_ID == 2)
                {
                    throw new InvalidOperationException("Local works folder is not initialized.");
                }
                return _localWorksFolder;
            }
        }
        
        public string LocalWorksForAdminFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_localWorksForAdminFolder) && App.UserDetails.Role_ID == 2)
                {
                    throw new InvalidOperationException("Local admin works folder is not initialized.");
                }
                return _localWorksForAdminFolder;
            }
        }

        public string LocalWorksForOtherUsersFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_localWorksForOtherUsersFolder) && App.UserDetails.Role_ID == 2)
                {
                    throw new InvalidOperationException("Local admin works for other users folder is not initialized.");
                }
                return _localWorksForOtherUsersFolder;
            }
        }
    }
}
