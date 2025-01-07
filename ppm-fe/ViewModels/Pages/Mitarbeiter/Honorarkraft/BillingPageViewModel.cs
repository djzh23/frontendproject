using System.Collections.ObjectModel;
using System.Globalization;
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
    public partial class BillingPageViewModel : BaseViewModel
    {
        private readonly IBillingService _billingService;
        private readonly IAuthService _authService;
        private  LocalPathService? _pathService;
        
        public LoadingController LoadingController { get; private set; }

        public BillingPageViewModel(IBillingService billingService, IAuthService authService, IConnectivityService connectivityService)
        {
            _authService = authService;
            _billingService = billingService;
            _pathService = new LocalPathService();
            ConnectivityService = connectivityService;

            LoadingController = new LoadingController();

            DateWorkDay = DateTime.Now; // Set default date to today
            BillingDetails = [];

            // tab 1 commands
            AddBillingDetailCommand = new Command(AddBillingDetail);
            DeleteBillingDetailCommand = new Command<BillingDetail>(DeleteBillingDetail);
            BillCreationCommand = new AsyncRelayCommand(ExecuteFullBillCreationProcess);

            // tab 2 commands
            FetchAllCommand = new AsyncRelayCommand(GetLastTenBills);
            GetLastTenBillsCommand = new AsyncRelayCommand(GetLastTenBills);
            DownloadAndOpenPdfCommand = new AsyncRelayCommand<Billing>(ExecuteDownloadAndOpenPdfAsync);
            LoadMoreCommand = new AsyncRelayCommand(
                async () =>
                {
                    await LoadMoreAsync();
                },
                () =>
                {
                    return HasMoreItems && !IsLoadingMore;
                });

            Today = DateTime.Today;
        }

        [ObservableProperty]
        private DateTime _today;

        #region TAB 1 - Rechnungen erstellen

        #region properties
        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private string? _selectedMonth;

        [ObservableProperty]
        private int _billingNumber;

        [ObservableProperty]
        private DateTime _dateWorkDay;

        [ObservableProperty]
        private string _numberOfHoursText = string.Empty;

        [ObservableProperty]
        private string _stundenlohnText = string.Empty;

        [ObservableProperty]
        private double _workDay;

        [ObservableProperty]
        private BillingDetail? _selectedBillingDetail;
        partial void OnSelectedBillingDetailChanged(BillingDetail? value)
        {
            if (value != null)
            {
                // Update the form fields with the selected billing detail
                DateWorkDay = value.DateWorkDay;
                NumberOfHoursText = value.NumberOfHours.ToString("F2");
                StundenlohnText = value.Stundenlohn.ToString("F2");
                WorkDay = value.WorkDay;
             }
            else
            {
                // Clear the form when deselecting
                ClearForm();
            }
        }

        [ObservableProperty]
        private ObservableCollection<BillingDetail> _billingDetails = [];

        // Keep the double properties for internal use
        public double NumberOfHours => ParseDouble(NumberOfHoursText);
        public double Stundenlohn => ParseDouble(StundenlohnText);
        public double TotalWorkDays => BillingDetails?.Sum(bd => bd.WorkDay) ?? 0;

        public int Id { get; set; }
        #endregion // tab 1 properties

        #region commands
        public ICommand AddBillingDetailCommand { get; }
        public Command<BillingDetail> DeleteBillingDetailCommand { get; }
        public ICommand BillCreationCommand { get; }
        #endregion // tab 1 commands

        #region tasks
        private async void AddBillingDetail()
        {
            CalculateWorkDay();
            if (NumberOfHours == 0 || Stundenlohn == 0 || WorkDay == 0)
            {
                return;
            }

            if(NumberOfHours > 24)
            {
                await DisplayAlertAsync("Warnung", "Der Studenanzahl ist ungültig. Bitte geben Sie einen gültigen Wert ein, der nicht größer als 24h ist.");
                return;
            }

            if (BillingDetails.Any(detail => detail.DateWorkDay.Date == DateWorkDay.Date))
            {
                await DisplayAlertAsync("Warnung", "Es existiert bereits ein Eintrag für dieses Datum.");
                return;
            }
            
            var newDetail = new BillingDetail
            {
                DateWorkDay = DateWorkDay,
                NumberOfHours = NumberOfHours,
                Stundenlohn = Stundenlohn,
                WorkDay = WorkDay
            };

            BillingDetails.Add(newDetail);

            // Reset fields after adding
            NumberOfHoursText = string.Empty;
            StundenlohnText = string.Empty;
            WorkDay = 0;
        }

        private void DeleteBillingDetail(BillingDetail billingDetail)
        {
            if (billingDetail != null)
            {
                BillingDetails.Remove(billingDetail);

                // If the deleted item was the selected item, clear the form
                if (SelectedBillingDetail == billingDetail)
                {
                    ClearForm();
                }
            }
        }

        public async Task ExecuteFullBillCreationProcess()
        {
            try
            {
                // Start Loading
                LoadingController.StartLoading("Validierung von Rechnungsdaten...");

                EnsurePathServiceIsInitialized();

                // step 1: validate required fields
                if (!ValidateBilling())
                {
                    LoadingController.StopLoading();
                    await DisplayAlertAsync("Fehler", "Bitte füllen Sie alle erforderlichen Felder für die Rechnungsstellung aus");
                    return;
                }

                // Step 2: Create Bill
                bool isCreateBillingSuccess = await CreateBilling();
                if (!isCreateBillingSuccess)
                {
                    LoadingController.StopLoading();
                    return;
                }

                LoadingController.StartLoading("Rechnung erfolgreich erstellt...");


                // Step 3: Generate PDF
                LoadingController.StartLoading("PDF wird generiert...");
                bool isCreatePdfSuccess = await ExecuteCreatePdfAsync();
                if (!isCreatePdfSuccess)
                {
                    LoadingController.StopLoading();
                    return;
                }

                // Send message using WeakReferenceMessenger to update cache after creation
                WeakReferenceMessenger.Default.Send(new BillingMessage());
                WeakReferenceMessenger.Default.Send(new HomePageMessage());

                // Notify user
                LoadingController.StopLoading();
                await DisplayAlertAsync("Erfolg", "Rechnungen erstellt und PDF generiert!");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteFullBillCreationProcess), $"An error occurred: {ex.Message}", null, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        public async Task<bool> CreateBilling()
        {
            var billing = new Billing
            {
                Date = Date.ToString("yyyy-MM-dd"),
                Month = SelectedMonth,
                BillingNumber = BillingNumber,
                SommeAll = TotalWorkDays,
                BillingDetails = BillingDetails.ToList()
            };

            try
            {
                var response = await _billingService.CreateBilling(billing);
                if (response != null && response.Success && response.Data != null)
                {
                    Id = response.Data.Id;
                    return true;
                }
                else
                {
                    await DisplayAlertAsync("Fehler", response != null ? response.Message : Properties.UnexpectedError);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(CreateBilling), $"An error occurred: {ex.Message}", billing, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }

            return false;
        }

        private async Task<bool> ExecuteCreatePdfAsync()
        {
            var userInfos = App.UserDetails;
            if (userInfos == null)
            {
                await DisplayAlertAsync("Fehler", "Benutzerdetails sind nicht verfügbar.");
                return false;
            }

            BillingsInfoProfile billingsInfoProfile = new BillingsInfoProfile
            {
                Date = Date.ToString("yyyy-MM-dd"),
                Month = SelectedMonth,
                BillingNumber = BillingNumber,
                SommeAll = TotalWorkDays,
                Firstname = userInfos.Firstname,
                Lastname = userInfos.Lastname,
                UserAddress = userInfos.Street + ", " + userInfos.Pzl + " " + userInfos.City,
                Iban = userInfos.Iban,
                Bic = userInfos.Bic,
                BankName = userInfos.bank_name,
                Steueridentifikationsnummer = userInfos.Steueridentifikationsnummer,
                BillingDetails = BillingDetails.ToList(),
                Role = RoleHelper.GetRoleName(userInfos.Role_ID)
            };

            try
            {
                string formattedDate = Date.ToString("yyyy-MM-dd");
                string fileName = $"rechnung-{BillingNumber}-{SelectedMonth}-{userInfos.Firstname}-{userInfos.Lastname}-{formattedDate}.pdf";

                string destinationFolder = _pathService?.LocalBillsFolder
                    ?? throw new InvalidOperationException("LocalBillsFolder is not set.");

                string destinationPath = Path.Combine(destinationFolder, fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(destinationFolder);
                PdfResult result = await PdfHelper.GenerateBillingPdf(destinationPath, billingsInfoProfile);

                if (result.IsSuccess)
                {
                    byte[] pdfBytes = File.ReadAllBytes(destinationPath);
                    await _billingService.StoreBillingPdfAsync("billing.pdf", pdfBytes, Id);

                    Console.WriteLine("PDF file created successfully. Trying to open...");
                    await OpenPdfAsync(destinationPath);
                    ResetForm();

                    return true;
                }
                else
                {
                    await DisplayAlertAsync("Fehler beim PDF Generieren", result.Properties);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteCreatePdfAsync), $"An error occurred: {ex.Message}", new { billingsInfoProfile }, ex.StackTrace);
            }
            finally
            {
                IsLoading = false;
            }

            return false;
        }

        #endregion //tasks

        #region utils
        private void ClearForm()
        {
            DateWorkDay = DateTime.Now;
            NumberOfHoursText = string.Empty;
            StundenlohnText = string.Empty;
            WorkDay = 0;
        }

        private void CalculateWorkDay()
        {
            WorkDay = NumberOfHours * Stundenlohn;
        }

        private bool ValidateBilling()
        {
            return BillingDetails.Count() != 0 && !string.IsNullOrWhiteSpace(SelectedMonth);
        }

        private void ResetForm()
        {

            Date = DateTime.Today;
            SelectedMonth = null;
            BillingNumber = 0;
            BillingDetails.Clear();
        }

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return 0.0;
        }

        private void EnsurePathServiceIsInitialized()
        {
            _pathService = new LocalPathService();
        }
        #endregion // utils tab 1
        #endregion // tab 1

        #region TAB 2 - Alle Rechnungen

        #region pagination
        private int _currentPage = 1;
        private const int PageSize = 10;

        [ObservableProperty]
        private bool _isLoadingMore;
        partial void OnIsLoadingMoreChanged(bool value)
        {
            (LoadMoreCommand as Command)?.ChangeCanExecute();
        }

        [ObservableProperty]
        private PaginationInfo? _paginationInfo;
        partial void OnPaginationInfoChanged(PaginationInfo? value)
        {
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(HasMoreItems));
            (LoadMoreCommand as Command)?.ChangeCanExecute();
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
        #endregion // tab 2 pagination

        #region properties
        [ObservableProperty]
        private ObservableCollection<Billing>? _billings;

        [ObservableProperty]
        private string? _selectedMonthTwo;
        partial void OnSelectedMonthTwoChanged(string? value)
        {
            Task.Run(async () =>
            {
                await FetchBillingsForMonth();
            }).Wait();
        }
        #endregion // properties tab 2

        #region commands
        public ICommand DownloadAndOpenPdfCommand { get; }
        public ICommand GetLastTenBillsCommand { get; }
        public ICommand FetchAllCommand { get; }
        public ICommand LoadMoreCommand { get; }
        #endregion //commands tab 2

        #region tasks
        public async Task InitializeAsync()
        {
            try
            {
                _currentPage = 1;

                // Get list of billings from cache for current page
                var billings = await CacheService.GetBillingsPageFromCache(_currentPage);

                Billings = new ObservableCollection<Billing>(new List<Billing>());
                Billings.Clear();
                foreach (var billing in billings)
                {
                    Billings.Add(billing);
                }

                // Refrech pagination info
                var result = await _billingService.GetBillings(_currentPage);
                if (result?.Pagination != null)
                {
                    PaginationInfo = result.Pagination;
                    _currentPage++;
                }
                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(InitializeAsync), $"An error occurred: {ex.Message}", null, ex.StackTrace);
            }
        }

        public async Task LoadMoreAsync()
        {
            if (IsLoading || !HasMoreItems) return;

            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");

                var response = new ApiResponse<List<Billing>>();
                if (SelectedMonthTwo != null)
                {
                    response = await _billingService.FetchBillsPerMonth(SelectedMonthTwo, _currentPage);
                }
                else
                {
                    response = await _billingService.GetBillings(_currentPage);
                }

                if (response.Success && response.Data != null)
                {
                    if (Billings == null)
                    {
                        Billings = new ObservableCollection<Billing>();
                    }
                    foreach (var billing in response.Data)
                    {
                        Billings.Add(billing);
                    }

                    // Refresh pagination info
                    if (response?.Pagination != null)
                    {
                        PaginationInfo = response.Pagination;
                        _currentPage++;
                    }
                    OnPropertyChanged(nameof(HasMoreItems));
                    OnPropertyChanged(nameof(TotalItems));
                    (LoadMoreCommand as Command)?.ChangeCanExecute();

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(LoadMoreAsync), $"Error loading more items: {ex.Message}", new { _currentPage }, ex.StackTrace);
            }
            finally
            {
                _currentPage++;
                LoadingController.StopLoading();
            }
        }

        private async Task GetLastTenBills()
        {
            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");
                SelectedMonthTwo = null;
                _currentPage = 1;

                var response = await _billingService.GetBillings(_currentPage);
                if (response.Success)
                {
                    if (response.Data != null)
                    {
                        Billings = new ObservableCollection<Billing>(response.Data);
                    }

                    // Refresh pagination info
                    if (response?.Pagination != null)
                    {
                        PaginationInfo = response.Pagination;
                        _currentPage++;
                    }
                }
                else
                {
                    await DisplayAlertAsync("Fehler", response.Message);
                }

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
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
            if (string.IsNullOrEmpty(SelectedMonthTwo)) return;

            try
            {
                LoadingController.StartLoading("Rechnungen wird geladen...");
                _currentPage = 1;

                // Get list of billings for selected month from cache
                var billings = await CacheService.GetBillingsPerMonthPageFromCache(SelectedMonthTwo, _currentPage);

                Billings = new ObservableCollection<Billing>(new List<Billing>());
                Billings.Clear();
                foreach (var billing in billings)
                {
                    Billings.Add(billing);
                }

                // Refresh Pagination Info
                var result = await _billingService.FetchBillsPerMonth(SelectedMonthTwo, _currentPage);
                if (result?.Pagination != null)
                {
                    _currentPage++;
                }

                PaginationInfo = result != null ? result.Pagination : null;
                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
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

        private async Task ExecuteDownloadAndOpenPdfAsync(Billing? billingItem)
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

                LoadingController.StopLoading();
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        public async Task OpenPdfAsync(string destinationPath)
        {
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

        // Unsubscrive from BillingMessage when existing the page
        public void OnDisappearing()
        {
            WeakReferenceMessenger.Default.Unregister<BillingMessage>(this);
        }
        #endregion // tasks tab 2

        #endregion // tab 2
    }
}
