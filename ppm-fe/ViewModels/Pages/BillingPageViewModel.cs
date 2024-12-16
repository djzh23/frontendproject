using CommunityToolkit.Mvvm.Input;
using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Input;

namespace ppm_fe.ViewModels.Pages
{
    public partial class BillingPageViewModel : BaseViewModel
    {
        private readonly IBillingService _billingService;
        private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;
        private readonly LocalPathService? _pathService;

        public LoadingController LoadingController { get; private set; }

        // Constructor
        public BillingPageViewModel(IBillingService billingService, IAuthService authService, ICacheService cacheService, IConnectivityService connectivityService)
        {
            _authService = authService;
            _billingService = billingService;
            _cacheService = cacheService;
            _pathService = new LocalPathService();
            ConnectivityService = connectivityService;

            LoadingController = new LoadingController();

            Title = "Rechnungen";
            IsCreationSucceed = false;
            DateWorkDay = DateTime.Now; // Set default date to today
            BillingDetails = [];
            PdfUrls = new ObservableCollection<string>();

            // tab 1 commands
            AddBillingDetailCommand = new Command(AddBillingDetail);
            DeleteBillingDetailCommand = new Command<BillingDetail>(DeleteBillingDetail);
            CreateBillingCommand = new AsyncRelayCommand(CreateBilling);
            PreviewCreateBillingCommand = new AsyncRelayCommand(PreviewCreateBilling);
            BillCreationCommand = new AsyncRelayCommand(async () => await ExecuteFullBillCreationProcess());

            // tab 2 commands
            FetchAllCommand = new AsyncRelayCommand(GetLastTenBills);
            SelectMonthCommand = new Command<DateTime>(SelectMonth);
            GetLastTenBillsCommand = new AsyncRelayCommand(GetLastTenBills);
            DownloadAndOpenPdfCommand = new Command<Billing>(async (billingItem) => await ExecuteDownloadAndOpenPdfAsync(billingItem));
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

            FetchPdfUrls();
        }

        private bool _isCreationSucceed;
        public bool IsCreationSucceed
        {
            get => _isCreationSucceed;
            set
            {
                if (_isCreationSucceed != value)
                {
                    _isCreationSucceed = value;
                    OnPropertyChanged();
                    (DownloadAndOpenPdfCommand as Command)?.ChangeCanExecute();
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


        #region TAB 1 - Rechnungen erstellen
        #region properties
        private DateTime _date = DateTime.Today;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        private string? _selectedMonth;
        public string? SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
            }
        }


        private int _billingNumber;
        public int BillingNumber
        {
            get => _billingNumber;
            set
            {
                _billingNumber = value;
                OnPropertyChanged();
            }
        }


        private string? _selectedTeam;
        public string? SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                _selectedTeam = value;
                OnPropertyChanged();
            }
        }


        private bool _isVorOrt;
        public bool IsVorOrt
        {
            get => _isVorOrt;
            set
            {
                _isVorOrt = value;
                OnPropertyChanged();
            }
        }


        private DateTime _dateWorkDay;
        public DateTime DateWorkDay
        {
            get => _dateWorkDay;
            set => SetProperty(ref _dateWorkDay, value);
        }


        private string _numberOfHoursText = string.Empty;
        public string NumberOfHoursText
        {
            get => _numberOfHoursText;
            set => SetProperty(ref _numberOfHoursText, value);
        }


        private string _stundenlohnText = string.Empty;
        public string StundenlohnText
        {
            get => _stundenlohnText;
            set => SetProperty(ref _stundenlohnText, value);
        }


        private double _workDay;
        public double WorkDay
        {
            get => _workDay;
            set => SetProperty(ref _workDay, value);
        }


        private BillingDetail? _selectedBillingDetail;
        public BillingDetail? SelectedBillingDetail
        {
            get => _selectedBillingDetail;
            set
            {
                if (SetProperty(ref _selectedBillingDetail, value))
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
            }
        }


        private ObservableCollection<BillingDetail> _billingDetails = [];
        public ObservableCollection<BillingDetail> BillingDetails
        {
            get => _billingDetails;
            set => SetProperty(ref _billingDetails, value);
        }

        // Keep the double properties for internal use
        public double NumberOfHours => ParseDouble(NumberOfHoursText);
        public double Stundenlohn => ParseDouble(StundenlohnText);
        public double TotalWorkDays => BillingDetails?.Sum(bd => bd.WorkDay) ?? 0;

        public ObservableCollection<string> PdfUrls { get; set; }

        public ObservableCollection<string> Months { get; } = new ObservableCollection<string> { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" };

        public ObservableCollection<string> Teams { get; } = new ObservableCollection<string> { "FF1", "FF2", "FF3", "FF4", "FF5", "FF6" };
        
        public int Id { get; set; }
        #endregion // tab 1 properties

        #region commands
        public Command AddBillingDetailCommand { get; }
        public Command<BillingDetail> DeleteBillingDetailCommand { get; }

        public ICommand PreviewCreateBillingCommand { get; }
        public ICommand CreateBillingCommand { get; }
        public ICommand BillCreationCommand { get; }
        public ICommand SelectMonthCommand { get; }
        #endregion // tab 1 commands

        #region tasks
        private void AddBillingDetail()
        {
            CalculateWorkDay();
            if (NumberOfHours == 0 || Stundenlohn == 0 || WorkDay == 0)
            {
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

                // Validate required fields
                if (!ValidateBilling())
                {
                    await DisplayAlertAsync("Fehler", "Bitte füllen Sie alle erforderlichen Felder für die Rechnungsstellung aus");
                    LoadingController.StopLoading();
                    return;
                }

                // Validate preview generation, entered billing number must not be taken, validation is done at the API level
                bool isPreviewBillingSuccess = await PreviewCreateBilling();
                if (!isPreviewBillingSuccess)
                {
                    LoadingController.StopLoading();
                    return;
                }
                LoadingController.StartLoading("Rechnungsvorschau erfolgreich...");

                // Step 2: Create Bill
                LoadingController.StartLoading("Erstellen von Rechnungen...");
                bool isCreateBillingSuccess = await CreateBilling();
                if (!isCreateBillingSuccess)
                {
                    LoadingController.StopLoading();
                    return;
                }
                LoadingController.StartLoading("Rechnung erfolgreich erstellt...");

                // Step 3: Generate PDF
                LoadingController.StartLoading("PDF generieren...");
                bool isCreatePdfSuccess = await ExecuteCreatePdfAsync();
                if (!isCreatePdfSuccess)
                {
                    LoadingController.StopLoading();
                    return;
                }

                // Cache direkt aktualisieren
                await _cacheService.RefreshBillingsCache();
                await _cacheService.RefreshDashboardCache();

                // Erst Message senden
                MessagingCenter.Send(this, MessageKeys.BillingsChanged);

                // Notify user
                await DisplayAlertAsync("Erfolg", "Rechnungen erstellt und PDF generiert!");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteFullBillCreationProcess), $"An error occurred: {ex.Message}", null, ex.StackTrace);
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
            }
        }

        public async Task<bool> PreviewCreateBilling()
        {
            var billing = new Billing
            {
                IsVorOrt = IsVorOrt,
                Date = Date.ToString("yyyy-MM-dd"),
                Month = SelectedMonth,
                BillingNumber = BillingNumber,
                Team = SelectedTeam,
                SommeAll = TotalWorkDays,
                BillingDetails = BillingDetails.ToList()
            };

            try
            {
                var response = await _billingService.PreviewCreateBilling(billing);
                if (response.Success)
                {
                    return true;
                }
                else
                {
                    await DisplayAlertAsync("Fehler", response.Message);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(PreviewCreateBilling), $"An error occurred: {ex.Message}", billing, ex.StackTrace);
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
            }

            return false;
        }

        public async Task<bool> CreateBilling()
        {
            var billing = new Billing
            {
                IsVorOrt = IsVorOrt,
                Date = Date.ToString("yyyy-MM-dd"),
                Month = SelectedMonth,
                BillingNumber = BillingNumber,
                Team = SelectedTeam,
                SommeAll = TotalWorkDays,
                BillingDetails = BillingDetails.ToList()
            };

            try
            {
                var response = await _billingService.CreateBilling(billing);
                if (response.Success)
                {
                    Id = response.Data.Id;
                    return true;
                }
                else
                {
                    await DisplayAlertAsync("Fehler", "Erstellen der Rechnung fehlgeschlagen");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(CreateBilling), $"An error occurred: {ex.Message}", billing, ex.StackTrace);
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
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
                IsVorOrt = IsVorOrt,
                Date = Date.ToString("yyyy-MM-dd"),
                Month = SelectedMonth,
                BillingNumber = BillingNumber,
                Team = SelectedTeam,
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

                string fileName = IsVorOrt
                    ? $"{formattedDate}-{SelectedMonth}-rechnung-{BillingNumber}.pdf"
                    : $"{formattedDate}-{SelectedMonth}-ausflug-rechnung-{BillingNumber}.pdf";

                string destinationFolder = _pathService?.LocalBillsFolder
                    ?? throw new InvalidOperationException("LocalBillsFolder is not set.");

                string destinationPath = Path.Combine(destinationFolder, fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(destinationFolder);
                PdfResult result = await PdfHelper.GeneratePdfBilling(destinationPath, billingsInfoProfile);

                if (result.IsSuccess)
                {
                    byte[] pdfBytes = File.ReadAllBytes(destinationPath);
                    await _billingService.UploadBillingPdfAsync("billing.pdf", pdfBytes, Id);

                    Console.WriteLine("PDF file created successfully. Trying to open...");
                    await OpenPdfAsync(destinationPath);
                    ResetForm();

                    return true;
                }
                else
                {
                    await DisplayAlertAsync("Fehler beim PDF Generieren", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteCreatePdfAsync), $"An error occurred: {ex.Message}", new { billingsInfoProfile }, ex.StackTrace);
                IsCreationSucceed = false;
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
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
            return !string.IsNullOrWhiteSpace(SelectedTeam) && BillingDetails.Count() != 0;
        }

        private void ResetForm()
        {

            Date = DateTime.Today;
            SelectedMonth = null;
            BillingNumber = 0;
            SelectedTeam = null;
            IsVorOrt = false;
            _billingDetails.Clear();
        }

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return 0.0;
        }
        #endregion // utils tab 1
        #endregion // tab 1

        #region TAB 2 - Alle Rechnungen

        #region properties
        private string _selectedMonthTwo;
        public string SelectedMonthTwo
        {
            get => _selectedMonthTwo;
            set
            {
                if (_selectedMonthTwo != value)
                {
                    _selectedMonthTwo = value;
                    OnPropertyChanged();
                    // You can add any logic here that should run when a new month is selected
                    FetchBillingsForMonth();
                }
            }
        }

        private ObservableCollection<Billing> _billings;
        public ObservableCollection<Billing> Billings
        {
            get => _billings;
            set
            {
                _billings = value;
                OnPropertyChanged();
            }
        }
        #endregion // properties tab 2

        #region commands
        public Command DownloadAndOpenPdfCommand { get; }

        public AsyncRelayCommand GetLastTenBillsCommand { get; }

        public ICommand FetchAllCommand { get; }

        public ICommand LoadMoreCommand { get; }
        #endregion //commands tab 2

        #region tasks
        private void SelectMonth(DateTime month)
        {
            IsLoading = true;
        }

        private async Task GetLastTenBills()
        {
            IsLoading = true;
            _currentPage = 1;

            var res = await _billingService.GetBillings(_currentPage);
            if (res.Success)
            {
                IsLoading = false;
                SelectedMonthTwo = null;
                if (res.Data != null)
                {
                    Billings = new ObservableCollection<Billing>(res.Data);
                }

                _currentPage++;
                _paginationInfo = res.Pagination;
            }
            else
            {
                await DisplayAlertAsync("Fehler", res.Message);
                IsLoading = false;
            }

            OnPropertyChanged(nameof(HasMoreItems));
            OnPropertyChanged(nameof(TotalItems));
            (LoadMoreCommand as Command)?.ChangeCanExecute();
            IsLoading = false;
        }

        private async Task FetchBillingsForMonth()
        {
            if (string.IsNullOrEmpty(SelectedMonthTwo)) return;

            IsLoading = true;
            _currentPage = 1;

            // Hole nur die erste Seite
            var billings = await _cacheService.GetBillingsPerMonthPageFromCache(SelectedMonthTwo, _currentPage);


            Billings = new ObservableCollection<Billing>(new List<Billing>());
            Billings.Clear();
            foreach (var billing in billings)
            {
                Billings.Add(billing);
            }

            // Pagination Info aktualisieren
            var result = await _billingService.FetchBillsPerMonth(SelectedMonthTwo, _currentPage);
            if (result?.Pagination != null)
            {
                _currentPage++;
            }

            _paginationInfo = result.Pagination;
            OnPropertyChanged(nameof(HasMoreItems));
            OnPropertyChanged(nameof(TotalItems));
            (LoadMoreCommand as Command)?.ChangeCanExecute();
            IsLoading = false;

            Debug.WriteLine($"Initialize completed. Page: {_currentPage - 1}, Items Loaded: {Billings.Count}");

            

            //IsLoading = true;
            //var res = await _billingService.FetchBillsPerMonth(SelectedMonthTwo);
            //if (res.Success)
            //{
            //    IsLoading = false;
            //    Billings = new ObservableCollection<Billing>(res.Data ?? new List<Billing>());
            //}
            //else
            //{
            //    await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
            //    IsLoading = false;
            //}
        }

        private async Task ExecuteDownloadAndOpenPdfAsync(Billing billingItem)
        {
            if (billingItem == null || string.IsNullOrEmpty(billingItem.PdfFileBilling))
            {
                await DisplayAlertAsync("Fehler", "Kein Rechnungsposten oder PDF-Pfad verfügbar.");
                return;
            }

            var tcs = new TaskCompletionSource<bool>();

            try
            {
                LoadingController.StartLoading("Herunterladen von PDF...");

                string localFilePath = await _billingService.DownloadAndOpenPdfAsync(billingItem.BillingNumber, billingItem.PdfFileBilling, _pathService.LocalBillsFolder);


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
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
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

                    // Launch the PDF file using Xamarin.Essentials' Launcher
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

        #endregion // tasks tab 2


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
                Debug.WriteLine($"HasMoreItems: {hasMore}, CurrentPage: {_currentPage}, LastPage: {PaginationInfo?.LastPage}");
                return hasMore;
            }
        }

        // Wichtig: Cleanup beim Verlassen der Seite
        public void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<BillingPageViewModel>(this, MessageKeys.BillingsChanged);
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                _currentPage = 1;

                // Hole nur die erste Seite
                var billings = await _cacheService.GetBillingsPageFromCache(_currentPage);


                Billings = new ObservableCollection<Billing>(new List<Billing>());
                Billings.Clear();
                foreach (var billing in billings)
                {
                    Billings.Add(billing);
                }

                // Pagination Info aktualisieren
                var result = await _billingService.GetBillings(_currentPage);
                if (result?.Pagination != null)
                {
                    _paginationInfo = result.Pagination;
                    _currentPage++;
                }

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();

                Debug.WriteLine($"Initialize completed. Page: {_currentPage - 1}, Items Loaded: {Billings.Count}");
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
            if (IsBusy || !HasMoreItems) return;

            try
            {
                IsBusy = true;

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
                    foreach (var billing in response.Data)
                    {
                        Billings.Add(billing);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading more items: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                SelectedMonthTwo = null;
                _currentPage++;
                OnPropertyChanged(nameof(HasMoreItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
        }

        #endregion // tab 2

        private async void FetchPdfUrls()
        {
            var urls = await _billingService.GetPdfUrls();

            if (urls.Success && urls.Data != null)
            {
                PdfUrls.Clear();
                foreach (var url in urls.Data)
                {
                    PdfUrls.Add(url);
                }
            }
        }

    }
}
