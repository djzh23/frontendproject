using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Messages;
using ppm_fe.Models;
using ppm_fe.Services;

namespace ppm_fe.ViewModels.Pages
{
    public partial class AllAdminWorksPageViewModel : BaseViewModel
    {
        private readonly IWorkService _workService;
        private ILocalPathService _pathService;
        public LoadingController LoadingController { get; private set; }

        public AllAdminWorksPageViewModel(IWorkService workService, ILocalPathService pathService, IConnectivityService connectivityService)
        {
            _workService = workService;
            _pathService = pathService;
            ConnectivityService = connectivityService;
            LoadingController = new LoadingController();

            Works = new ObservableCollection<Work>();

            GetLastTenWorksCommand = new AsyncRelayCommand(GetLastTenWorks);
            DownloadAndOpenPdfCommand = new Command<Work>(async (workItem) => await ExecuteDownloadAndOpenPdfAsync(workItem));
            LoadMoreCommand = new Command(
                async () => await LoadMoreAsync(),
                () => HasMoreItems && !IsLoadingMore
            );
        }

        #region pagination
        private int _currentPage = 1;
        private const int PageSize = 10;

        [ObservableProperty]
        private bool _isLoadingMore;

        [ObservableProperty]
        private PaginationInfo? _paginationInfo;
        partial void OnPaginationInfoChanged(PaginationInfo? value)
        {
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(HasMoreItems));
        }

        public int TotalItems
        {
            get => PaginationInfo?.Total ?? 0;
            private set
            {
                if (PaginationInfo != null)
                {
                    PaginationInfo.Total = value;
                    OnPropertyChanged(nameof(TotalItems));
                }
            }
        }

        public bool HasMoreItems
        {
            get
            {
                var hasMore = PaginationInfo != null && _currentPage <= PaginationInfo.LastPage;
                return hasMore;
            }
        }
        #endregion

        #region properties
        [ObservableProperty]
        private ObservableCollection<Work> _works;

        [ObservableProperty]
        private string? _selectedTeam;
        partial void OnSelectedTeamChanged(string? value)
        {
            Task.Run(async () =>
            {
                await FetchWorksPerTeam();
            }).Wait();
        }

        [ObservableProperty]
        private List<string>? _team;

        [ObservableProperty]
        private string? _date;

        [ObservableProperty]
        private string? _creatorName;
        #endregion

        #region commands
        public AsyncRelayCommand GetLastTenWorksCommand { get; }
        public Command LoadMoreCommand { get; private set; }
        public Command DownloadAndOpenPdfCommand { get; }
        #endregion

        #region tasks
        public async Task InitializeAsync()
        {
            try
            {
                LoadingController.StartLoading("Einsätze wird geladen...");
                _currentPage = 1;

                // Fetch the list of first 10 all users works from cache
                var works = await CacheService.GetAdminWorksPageFromCache(_currentPage);

                Works = new ObservableCollection<Work>(new List<Work>());
                Works.Clear();
                foreach (var work in works)
                {
                    Works.Add(work);
                }

                // Refresh Pagination Info 
                var result = await _workService.GetAllUsersWorks(_currentPage);
                if (result?.Pagination != null)
                {
                    PaginationInfo = result.Pagination;
                    _currentPage++;
                }

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(InitializeAsync), $"An error occurred: {ex.Message}", null, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        private async Task GetLastTenWorks()
        {
            try
            {
                LoadingController.StartLoading("Einsätze wird geladen...");

                _currentPage = 1;
                SelectedTeam = null;

                var response = await _workService.GetAllUsersWorks(_currentPage);
                if (response.Success && response.Data != null)
                {
                    Works.Clear();
                    foreach (var work in response.Data)
                    {
                        Works.Add(work);
                    }

                    // Refresh Pagination Info 
                    if (response?.Pagination != null)
                    {
                        _currentPage++;
                    }

                    PaginationInfo = response?.Pagination;
                    OnPropertyChanged(nameof(HasMoreItems));
                    OnPropertyChanged(nameof(TotalItems));
                }
                else
                {
                    await DisplayAlertAsync("Fehler", response.Message);
                }

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(GetLastTenWorks), ex.Message, new { SelectedTeam, _currentPage }, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        private async Task FetchWorksPerTeam()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTeam)) return;

                LoadingController.StartLoading("Einsätze wird geladen...");

                _currentPage = 1;

                // Get the list of all users works per team from cache for page 1
                var works = await CacheService.GetWorksPerTeamPageFromCache(SelectedTeam, _currentPage);

                Works.Clear();
                foreach (var work in works)
                {
                    Works.Add(work);
                }

                // Refresh Pagination Info 
                var result = await _workService.FetchWorksPerTeam(SelectedTeam, _currentPage);
                if (result?.Pagination != null)
                {
                    _currentPage++;
                }

                PaginationInfo = result?.Pagination;
                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));

            }
            catch(Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(FetchWorksPerTeam), ex.Message, new { SelectedTeam }, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
            }
            
        }

        public async Task LoadMoreAsync()
        {
            if (!HasMoreItems) return;

            try
            {
                LoadingController.StartLoading("Einsätze wird geladen...");

                var response = new ApiResponse<List<Work>>();
                if (SelectedTeam != null)
                {
                    response = await _workService.FetchWorksPerTeam(SelectedTeam, _currentPage);
                }
                else
                {
                    response = await _workService.GetAllUsersWorks(_currentPage);
                }

                if (response.Success && response.Data != null)
                {
                    foreach (var work in response.Data)
                    {
                        Works.Add(work);
                    }
                    
                    // Refresh Pagination Info 
                    if (response?.Pagination != null)
                    {
                        _currentPage++;
                    }

                    PaginationInfo = response?.Pagination;
                    OnPropertyChanged(nameof(HasMoreItems));
                    OnPropertyChanged(nameof(TotalItems));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(LoadMoreAsync), ex.Message, new { SelectedTeam, _currentPage }, ex.StackTrace);
            }
            finally
            {
                _currentPage++;
                LoadingController.StopLoading();
            }
        }

        private async Task ExecuteDownloadAndOpenPdfAsync(Work workItem)
        {
            if (workItem == null || string.IsNullOrEmpty(workItem.PdfFile))
            {
                await DisplayAlertAsync("Fehler", "Kein Einsatz oder PDF-Pfad verfügbar.");
                return;
            }

            LoadingController.StartLoading("Herunterladen von PDF...");

            try
            {
                EnsurePathServiceIsInitialized();

                var tcs = new TaskCompletionSource<bool>();

                string path = "";
                if (workItem.CreatorId == App.UserDetails?.Id)
                {
                    path = _pathService.LocalWorksForAdminFolder;
                }
                else
                {
                    path = _pathService.LocalWorksForOtherUsersFolder;
                }
                string localFilePath = await _workService.DownloadAndOpenPdfAsync(workItem.Id, workItem.PdfFile, path);

                // Wait for a short time to allow the download to start
                await Task.Delay(2000);
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(localFilePath)
                });
                
                await DisplayAlertAsync("Erfolg", "Einsatz PDF Download eingeleitet...");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(ExecuteDownloadAndOpenPdfAsync), $"Error initiating PDF download: {ex.Message}", new { workItem }, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        private void EnsurePathServiceIsInitialized()
        {
            _pathService = new LocalPathService();
        }

        public void OnDisappearing()
        {
            SelectedTeam = null;

            // Unsubscrive from WorkMessage when existing the page
            WeakReferenceMessenger.Default.Unregister<WorkMessage>(this);
        }
        #endregion
    }
}
