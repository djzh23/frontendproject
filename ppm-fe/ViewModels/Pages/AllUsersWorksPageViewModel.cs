using CommunityToolkit.Mvvm.Input;
using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ppm_fe.ViewModels.Pages
{
    public partial class AllUsersWorksPageViewModel : BaseViewModel
    {
        private readonly IWorkService _workService;
        private readonly ICacheService _cacheService;
        private readonly IAuthService _authService;
        private readonly ILocalPathService _pathService;
        public LoadingController LoadingController { get; private set; }

        private int _currentPage = 1;
        private const int PageSize = 10;

        private bool _isLoadingMore;
        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set
            {
                SetProperty(ref _isLoadingMore, value);
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
        }

        private PaginationInfo _paginationInfo;
        public PaginationInfo PaginationInfo
        {
            get => _paginationInfo;
            private set
            {
                if (_paginationInfo != value)
                {
                    _paginationInfo = value;
                    OnPropertyChanged(nameof(PaginationInfo));
                    OnPropertyChanged(nameof(TotalItems));
                    OnPropertyChanged(nameof(HasMoreItems));
                    (LoadMoreCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _date;
        public int Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }


        private List<string> _teams;
        public List<string> Teams
        {
            get => _teams;
            set
            {
                _teams = value;
                OnPropertyChanged(nameof(Teams));
            }
        }


        private string creator_name;
        public string CreatorName
        {
            get => creator_name;
            set
            {
                creator_name = value;
                OnPropertyChanged(nameof(CreatorName));
            }
        }


        private string _selectedTeam;
        public string SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                if (_selectedTeam != value)
                {
                    _selectedTeam = value;
                    OnPropertyChanged();
                    GetWorksPerTeam();
                }
            }
        }


        private int _team;
        public int Team
        {
            get => _team;
            set
            {
                _team = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<Work> _works;
        public ObservableCollection<Work> Works
        {
            get => _works;
            set
            {
                _works = value;
                OnPropertyChanged();
            }
        }


        public Command DownloadAndOpenPdfCommand { get; }
        public ICommand FetchAllLastWorksCommand { get; }
        public ICommand FetchDataCommand => new Command(async () =>
        {
            try
            {
                IsLoading = true;
                await GetAllUsersWorks();
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(FetchDataCommand), $"Error fetching data: {ex.Message}", null, ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
            }
        });
        public ICommand LoadMoreCommand { get; }

        public AllUsersWorksPageViewModel(IWorkService workService, IAuthService authService, ILocalPathService pathService, IConnectivityService connectivityService, ICacheService cacheService)
        {
            ConnectivityService = connectivityService;
            _workService = workService;
            _authService = authService;
            _pathService = pathService;
            _cacheService = cacheService;

            LoadingController = new LoadingController();

            InitializeTeams();
            Works = new ObservableCollection<Work>();

            FetchAllLastWorksCommand = new AsyncRelayCommand(GetAllUsersWorks);
            DownloadAndOpenPdfCommand = new Command<Work>(async (work) => await ExecuteDownloadAndOpenPdfAsync(work));

            LoadMoreCommand = new Command(
                async () =>
                {
                    Debug.WriteLine("LoadMore Command executed");
                    await LoadMoreAsync();
                },
                () =>
                {
                    Debug.WriteLine($"CanExecute: HasMoreItems={HasMoreItems}, !IsLoadingMore={!IsLoadingMore}");
                    return HasMoreItems && !IsLoadingMore;
                });
        }

        private void InitializeTeams()
        {
            Teams = new List<string> { "FF1", "FF2", "FF3", "FF4", "FF5" };
        }

        public async Task GetAllUsersWorks()
        {
            IsLoading = true;
            SelectedTeam = null;
            _currentPage = 1; // Reset page number

            //var res = await _workService.GetAllUsersWorks(_currentPage);
            var res = await _workService.GetAllUsersWorks(_currentPage);
            if (res != null && res.Success)
            {
                Works.Clear(); // Clear existing items
                foreach (var work in res.Data)
                {
                    Works.Add(work);
                }

                if (res.Pagination != null)
                {
                    PaginationInfo = res.Pagination;
                    _currentPage++;
                }

                OnPropertyChanged(nameof(HasMoreItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
            else
            {
                await DisplayAlertAsync("Fehler", "Einsätze konnten nicht geladen werden");
            }

            IsLoading = false;
        }


        private async Task GetWorksPerTeam()
        {
            IsLoading = true;
            _currentPage = 1; // Reset page number

            if (string.IsNullOrEmpty(SelectedTeam)) return;

            IsLoading = true;
            var res = await _workService.FetchWorksPerTeam(SelectedTeam, _currentPage);
            if (res != null && res.Success)
            {
                IsLoading = false;
                Works = new ObservableCollection<Work>(res.Data ?? Enumerable.Empty<Work>());

                PaginationInfo = res.Pagination;
                _currentPage++;

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
            else
            {
                await DisplayAlertAsync("Fehler", $"Einsätze konnten für Team {SelectedTeam} nicht geladen werden");
                IsLoading = false;
            }
        }


        private async Task ExecuteDownloadAndOpenPdfAsync(Work work)
        {
            if (work == null || string.IsNullOrEmpty(work.PdfFile))
            {
                await DisplayAlertAsync("Fehler", "Kein Einsatz oder PDF-Pfad verfügbar.");
                return;
            }

            try
            {
                IsBusy = true;
                LoadingController.StartLoading("Herunterladen von PDF...");

                string path = "";
                if (work.CreatorId == App.UserDetails?.Id)
                {
                    path = _pathService.LocalWorksForAdminFolder;
                }
                else
                {
                    path = _pathService.LocalWorksForOtherUsersFolder;
                }
                string localFilePath = await _workService.DownloadAndOpenPdfAsync(work.Id, work.PdfFile, path);

                // Wait for a short time to allow the download to start
                await Task.Delay(2000);
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(localFilePath)
                });

                LoadingController.StartLoading("Einsatz PDF Download eingeleitet...");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteDownloadAndOpenPdfAsync), $"An error occurred: {ex.Message}", null, ex.StackTrace);
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
                IsBusy = false;
            }
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
                var hasMore = PaginationInfo != null &&
                             Works.Count < PaginationInfo.Total &&
                             _currentPage <= PaginationInfo.LastPage;
                Debug.WriteLine($"HasMoreItems: {hasMore}, CurrentItems: {Works.Count}, Total: {PaginationInfo?.Total}, CurrentPage: {_currentPage}, LastPage: {PaginationInfo?.LastPage}");
                return hasMore;
            }
        }

        // Wichtig: Cleanup beim Verlassen der Seite
        public void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<AllUsersWorksPageViewModel>(this, MessageKeys.WorksChanged);
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                _currentPage = 1;

                // Hole nur die erste Seite
                var works = await _cacheService.GetUsersWorksPageFromCache(_currentPage);

                Works.Clear();
                foreach (var work in works)
                {
                    Works.Add(work);
                }

                // Pagination Info aktualisieren
                var result = await _workService.GetAllUsersWorks(_currentPage);
                if (result?.Pagination != null)
                {
                    _paginationInfo = result.Pagination;
                    _currentPage++;
                }

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
                OnPropertyChanged(nameof(Works));
                (LoadMoreCommand as Command)?.ChangeCanExecute();

                Debug.WriteLine($"Initialize completed. Page: {_currentPage - 1}, Items Loaded: {Works.Count}");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(InitializeAsync), $"An error occurred: {ex.Message}", null, ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadMoreAsync()
        {
            Debug.WriteLine($"LoadMoreAsync called - IsBusy: {IsBusy}, HasMoreItems: {HasMoreItems}");

            if (IsBusy || !HasMoreItems)
            {
                Debug.WriteLine("LoadMore skipped due to busy state or no more items");
                return;
            }

            try
            {
                IsBusy = true;
                IsLoadingMore = true;
                Debug.WriteLine($"Loading page {_currentPage}");

                ApiResponse<List<Work>> result = null;
                if(SelectedTeam != null)
                {
                    result = await _workService.GetAllUsersWorks(_currentPage);
                }
                else
                {
                    result = await _workService.GetAllUsersWorks(_currentPage);
                }
                

                if (result?.Data != null && result.Data.Any())
                {
                    foreach (var work in result.Data)
                    {
                        Works.Add(work);
                    }
                }
                else
                {
                    Debug.WriteLine("No more items received from server");
                    PaginationInfo = new PaginationInfo { LastPage = _currentPage - 1 };
                }

                PaginationInfo = result.Pagination;
                _currentPage++;
                OnPropertyChanged(nameof(HasMoreItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadMoreAsync: {ex.Message}");
                await DisplayAlertAsync("Fehler", "Fehler beim Laden weiterer Einträge");
            }
            finally
            {
                IsBusy = false;
                IsLoadingMore = false;
            }
        }
    }
}
