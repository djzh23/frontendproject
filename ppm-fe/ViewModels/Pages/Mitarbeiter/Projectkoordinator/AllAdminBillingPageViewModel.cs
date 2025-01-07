using System.Collections.ObjectModel;
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
    public partial class AllAdminBillingPageViewModel : BaseViewModel
    {
        private readonly IBillingService _billingService;
        private readonly IAuthService _authService;
        private LocalPathService? _pathService;

        public LoadingController LoadingController { get; private set; }

        public AllAdminBillingPageViewModel(IBillingService billingService, IAuthService authService, IConnectivityService connectivityService)
        {
            _authService = authService;
            _billingService = billingService;
            ConnectivityService = connectivityService;
            _pathService = new LocalPathService();
            LoadingController = new LoadingController();

            FetchAllCommand = new AsyncRelayCommand(GetLastTenBills);
            GetLastTenBillsCommand = new AsyncRelayCommand(GetLastTenBills);
            DownloadAndOpenPdfCommand = new Command<Billing>(async (billingItem) => await ExecuteDownloadAndOpenPdfAsync(billingItem));
            LoadMoreCommand = new Command(
                async () =>
                {
                    await LoadMoreAsync();
                },
                () =>
                {
                    return HasMoreItems && !IsLoadingMore;
                });
        }



        #region pagination
        private int _currentPage = 1;
        private const int PageSize = 10;

        private bool _isLoadingMore;
        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set
            {
                SetProperty(ref _isLoadingMore, value);
            }
        }

        private PaginationInfo? _paginationInfo;
        public PaginationInfo? PaginationInfo
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
                }
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
                var hasMore = PaginationInfo != null && _currentPage <= PaginationInfo.LastPage;
                return hasMore;
            }
        }
        #endregion

        #region properties
        [ObservableProperty]
        private string? _selectedMonth;
        partial void OnSelectedMonthChanged(string? value)
        {
            Task.Run(async () =>
            {
                await FetchBillingsForMonth();
            }).Wait();
        }

        [ObservableProperty]
        private ObservableCollection<Billing>? _billings;
        #endregion

        #region commands
        public Command DownloadAndOpenPdfCommand { get; }

        public AsyncRelayCommand GetLastTenBillsCommand { get; }

        public ICommand FetchAllCommand { get; }

        public ICommand LoadMoreCommand { get; }
        #endregion

        #region tasks
        public async Task InitializeAsync()
        {
            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");
                _currentPage = 1;

                // Get list of billings for all users from cache for current page
                var billings = await CacheService.GetAdminBillingsPageFromCache(_currentPage);

                Billings = new ObservableCollection<Billing>(new List<Billing>());
                Billings.Clear();
                foreach (var billing in billings)
                {
                    Billings.Add(billing);
                }

                // PRefresh pagination info 
                var result = await _billingService.GetAdminBillings(_currentPage);
                if (result?.Pagination != null)
                {
                    _paginationInfo = result.Pagination;
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

        public async Task LoadMoreAsync()
        {
            if (IsLoading || !HasMoreItems) return;

            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");

                var response = new ApiResponse<List<Billing>>();
                if (SelectedMonth != null)
                {
                    response = await _billingService.FetchAdminBillsPerMonth(SelectedMonth, _currentPage);
                }
                else
                {
                    response = await _billingService.GetAdminBillings(_currentPage);
                }

                if (response.Success && response.Data != null)
                {
                    foreach (var billing in response.Data)
                    {
                        Billings?.Add(billing);
                    }

                    // Refresh pagination info
                    if (response?.Pagination != null)
                    {
                        _paginationInfo = response.Pagination;
                        _currentPage++;
                    }
                    OnPropertyChanged(nameof(HasMoreItems));
                    OnPropertyChanged(nameof(TotalItems));

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(LoadMoreAsync), $"Error loading more items: {ex.Message}", new { _currentPage }, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        private async Task GetLastTenBills()
        {
            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");
                SelectedMonth = null;
                _currentPage = 1;

                var response = await _billingService.GetAdminBillings(_currentPage);
                if (response.Success)
                {
                    if (response.Data != null)
                    {
                        Billings = new ObservableCollection<Billing>(response.Data);
                    }

                    // Refresh pagination info
                    if (response?.Pagination != null)
                    {
                        _paginationInfo = response.Pagination;
                        _currentPage++;
                    }
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
                LoggerHelper.LogError(GetType().Name, nameof(GetLastTenBills), $"An error occured: {ex.Message}", new { _currentPage }, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
            }

        }

        private async Task FetchBillingsForMonth()
        {
            if (string.IsNullOrEmpty(SelectedMonth)) return;

            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");
                _currentPage = 1;

                // Get list of all users billings for selected month from cache
                var billings = await CacheService.GetAdminBillingsPerMonthPageFromCache(SelectedMonth, _currentPage);

                Billings = new ObservableCollection<Billing>(new List<Billing>());
                Billings.Clear();
                foreach (var billing in billings)
                {
                    Billings.Add(billing);
                }

                // Pagination Info aktualisieren
                var result = await _billingService.FetchAdminBillsPerMonth(SelectedMonth, _currentPage);
                if (result?.Pagination != null)
                {
                    _currentPage++;
                }

                PaginationInfo = result?.Pagination;
                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(FetchBillingsForMonth), $"An error occured: {ex.Message}", new { SelectedMonth, _currentPage }, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        private async Task ExecuteDownloadAndOpenPdfAsync(Billing billingItem)
        {
            if (billingItem == null || string.IsNullOrEmpty(billingItem.PdfFileBilling))
            {
                await DisplayAlertAsync("Fehler", "Kein Rechnungsposten oder PDF-Pfad verfügbar.");
                return;
            }

            try
            {
                LoadingController.StartLoading("Herunterladen von PDF...");

                EnsurePathServiceIsInitialized();

                if(_pathService?.LocalBillsFolder == null)
                {
                    await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
                    return;
                }

                var tcs = new TaskCompletionSource<bool>();
                string localFilePath = await _billingService.DownloadAndOpenPdfAsync(billingItem.Id, billingItem.PdfFileBilling, _pathService.LocalBillsFolder);

                // Wait for a short time to allow the download to start
                await Task.Delay(2000);
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(localFilePath)
                });
                LoadingController.StopLoading();
                await DisplayAlertAsync("Erfolg", "Rechnungs-PDF-Download eingeleitet!");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(ExecuteDownloadAndOpenPdfAsync), $"Error initiating PDF download: {ex.Message}", new { billingItem }, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        public async Task OpenPdfAsync(string destinationPath)
        {
            EnsurePathServiceIsInitialized();

            if (File.Exists(destinationPath))
            {
                try
                {
                    var pdfFile = new FileInfo(destinationPath);

                    // Launch the PDF file
                    var file = new FileResult(destinationPath);
                    var pdfRequest = new OpenFileRequest("Open PDF", file);

                    await Launcher.OpenAsync(pdfRequest);
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Fehler", "Die PDF-Datei konnte nicht geöffnet werde");
                    LoggerHelper.LogError(GetType().Name, nameof(OpenPdfAsync), $"Failed to open the PDF: {ex.Message}", new { destinationPath }, ex.StackTrace);
                }
            }
            else
            {
                await DisplayAlertAsync("Fehler", "PDF-Datei nicht gefunden!");
            }
        }

        private void EnsurePathServiceIsInitialized()
        {
            _pathService = new LocalPathService();
        }
        public void OnDisappearing()
        {
            // Unsubscrive from BillingMessage when existing the page
            WeakReferenceMessenger.Default.Unregister<BillingMessage>(this);
        }
        #endregion
    }
}
