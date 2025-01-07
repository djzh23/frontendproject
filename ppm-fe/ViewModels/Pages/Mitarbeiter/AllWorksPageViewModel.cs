using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Messages;
using ppm_fe.Models;
using ppm_fe.Services;

namespace ppm_fe.ViewModels.Pages
{
    public partial class AllWorksPageViewModel : BaseViewModel
    {
        private readonly IWorkService _workService;
        private readonly ILocalPathService? _pathService;
        public LoadingController LoadingController { get; private set; }

        public AllWorksPageViewModel(IWorkService workService, ILocalPathService pathService, IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;
            _workService = workService;
            _pathService = pathService;
            LoadingController = new LoadingController();

            Works = new ObservableCollection<Work>();
            ListOfHelpers = new ObservableCollection<string>();
            ListOfHelpers.CollectionChanged += ListOfHelpers_CollectionChanged;
            UpdateButtonColors();

            LoadWorksCommand = new Command(async () => await LoadWorksAsync());
            SelectWorkCommand = new Command<Work>(SelectWork);
            AddHelperCommand = new Command(AddHelper);
            RemoveHelperCommand = new Command<string>(RemoveHelper);
            UpdateHelperCommand = new Command<(string oldHelper, string newHelper)>(UpdateHelper);
            UpdateWorkCommand = new Command<Work>(async (work) => await UpdateWork(work));
            CompleteWorkCommand = new Command<Work>(async (work) => await CompleteWork(work));
            UpdateDateCommand = new Command<string>(OnUpdateDate);
            UpdateTeamCommand = new Command<string>(OnUpdateTeam);
            UpdateOrtCommand = new Command<string>(OnUpdateOrt);
            UpdatePlanCommand = new Command<string>(OnUpdatePlan);
            CalculateAllCommand = new Command(CalculateAll);
            CalculateCommand = new Command(CalculateTotals);
            DownloadPdfCommand = new Command(async () => await ExecuteCreatePdfAsync());
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

        [ObservableProperty]
        private PaginationInfo? _paginationInfo;
        partial void OnPaginationInfoChanged(PaginationInfo? value)
        {
            OnPropertyChanged(nameof(TotalItems));
            OnPropertyChanged(nameof(HasMoreItems));
        }

        [ObservableProperty]
        private bool _isLoadingMore;

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
        private Work? _selectedWork;
        partial void OnSelectedWorkChanged(Work? value)
        {
            if (value == null)
            {
                IsWorkSelected = false;
                ClearWorkData();
                return;
            }

            IsWorkSelected = true;
            UpdateSelectedWorkData(value);
        }

        [ObservableProperty]
        private bool _isWorkSelected;

        [ObservableProperty]
        private bool _isWorkDetailsVisible = true;

        [ObservableProperty]
        private DateTime _selectedDate;
        partial void OnSelectedDateChanged(DateTime value)
        {
            if (SelectedWork != null)
            {
                SelectedWork.Date = value.ToString("yyyy-MM-dd"); // Convert DateTime to string
            }
        }

        [ObservableProperty]
        private string? _selectedTeam;
        partial void OnSelectedTeamChanged(string? value)
        {
            if (SelectedWork != null)
            {
                SelectedWork.Team = value;
            }
        }

        [ObservableProperty]
        private bool? ort;

        [ObservableProperty]
        private ObservableCollection<WorkKidsData>? _kidsData;

        [ObservableProperty]
        private ObservableCollection<string> _listOfHelpers;

        [ObservableProperty]
        private bool _isAddingHelper;
        partial void OnIsAddingHelperChanged(bool value)
        {
            OnPropertyChanged(nameof(AddHelperButtonText));
            UpdateButtonColors();
        }

        [ObservableProperty]
        private string? _newHelperName;

        [ObservableProperty]
        private int? totalBoys;

        [ObservableProperty]
        private int? totalGirls;

        [ObservableProperty]
        private int? totalKids;

        [ObservableProperty]
        private DateTime? _date;
        partial void OnDateChanged(DateTime? value)
        {
            OnPropertyChanged(nameof(OrtAndDate));
            OnPropertyChanged(nameof(SelectedTeam));
        }

        [ObservableProperty]
        private string? _updatedAt;

        [ObservableProperty]
        private new bool _isLoading;
        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsNotLoading));
        }

        [ObservableProperty]
        private bool _isExpanderOpen;

        // Wrapper collection to display kids data in a table in the UI
        [ObservableProperty]
        private ObservableCollection<KidsDataWrapper>? _kidsDataUI;
        partial void OnKidsDataUIChanged(ObservableCollection<KidsDataWrapper>? value)
        {
            if (value != KidsDataUI) 
            {
                KidsDataUI = value;
            }
        }

        // Wrapper class for kids data in ViewModel
        public class KidsDataWrapper : ObservableObject
        {
            private readonly WorkKidsData _data;
            public bool _isEmpty;
            public KidsDataWrapper(WorkKidsData data)
            {
                _data = data;
                _isEmpty = ((data.NumberBoys + data.NumberGirls) == 0);
            }

            public string AgeRange => _data?.AgeRange ?? string.Empty;

            public int NumberBoys
            {
                get => _data.NumberBoys;
                set
                {
                    if (_data.NumberBoys != value)
                    {
                        _data.NumberBoys = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(RowTotal));
                    }
                }
            }

            public int NumberGirls
            {
                get => _data.NumberGirls;
                set
                {
                    if (_data.NumberGirls != value)
                    {
                        _data.NumberGirls = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(RowTotal));
                    }
                }
            }

            public int RowTotal => NumberBoys + NumberGirls;

            public bool IsEmpty
            {
                get => _isEmpty;
                private set => _isEmpty = value;
            }
        }

        private Dictionary<string, string> _helperChanges = new Dictionary<string, string>();

        public string AddHelperButtonText => IsAddingHelper ? "Helfer speichern" : "Neuen Helfer einfügen";

        public string OrtAndDate => $"Ort: {Ort}, Date: {Date:yyyy-MM-dd}";

        public string Status => SelectedWork?.Status ?? string.Empty;

        public bool IsComplete => string.Equals(Status, "complete", StringComparison.OrdinalIgnoreCase);

        public bool IsNotLoading => !IsLoading;
        #endregion

        #region commands
        public Command AddHelperCommand { get; private set; }
        public Command<string> RemoveHelperCommand { get; private set; }
        public Command<(string oldHelper, string newHelper)> UpdateHelperCommand { get; private set; }
        public Command LoadWorksCommand { get; private set; }
        public Command<Work> SelectWorkCommand { get; private set; }
        public Command<Work> UpdateWorkCommand { get; private set; }
        public Command<Work> CompleteWorkCommand { get; private set; }
        public Command<string> UpdateDateCommand { get; private set; }
        public Command<string> UpdateTeamCommand { get; private set; }
        public Command<string> UpdateOrtCommand { get; private set; }
        public Command<string> UpdatePlanCommand { get; private set; }
        public Command CalculateAllCommand { get; private set; }
        public Command CalculateCommand { get; private set; }
        public Command DownloadPdfCommand { get; private set; }
        public ICommand LoadMoreCommand { get; }
        #endregion

        #region tasks
        public async Task InitializeAsync()
        {
            try
            {
                _currentPage = 1;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = true;
                    LoadingController.StartLoading("Einsätze wird geladen..");
                });

                // First, reset all Page Data
                await ResetAllDataAsync();

                // Then fetch data from the API
                var result = await _workService.GetAllWorks(_currentPage);
                if (result?.Data != null)
                {
                    foreach (var work in result.Data)
                    {
                        Works.Add(work);
                    }
                }

                // Refresh Pagination Info
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
                LoggerHelper.LogError(GetType().Name, nameof(InitializeAsync), ex.Message, null, ex.StackTrace);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = false;
                    LoadingController.StopLoading();
                });
            }
        }

        private async Task ResetAllDataAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Clear cahe
                    CacheService.ClearCache();

                    // Clear Works and associated data
                    Works.Clear();
                    ListOfHelpers?.Clear();
                    KidsData?.Clear();

                    // SelectedWork zurücksetzen (triggert automatisch ClearWorkData durch OnSelectedWorkChanged)
                    SelectedWork = null;
                    IsWorkSelected = false;

                    // Andere Properties zurücksetzen
                    SelectedDate = DateTime.Now;
                    _currentPage = 1;
                    PaginationInfo = null;
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(ResetAllDataAsync), ex.Message, null, ex.StackTrace);
            }
        }

        private async Task LoadWorksAsync()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = true;
                    LoadingController.StartLoading("Einsätze wird geladen...");
                });

                await ResetAllDataAsync();
                await CacheService.RefreshWorksCache();

                var works = await CacheService.GetWorksPageFromCache(_currentPage);

                if (works == null || !works.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlertAsync("Fehler", "Keine Einsätze gefunden");
                    });
                    return;
                }

                // UI Updates im Main Thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var sortedWorks = SortWorks(new ObservableCollection<Work>(works));
                    Works = new ObservableCollection<Work>(sortedWorks);

                    if (Works.Any())
                    {
                        SelectedWork = Works.First();
                        IsWorkSelected = true;
                    }

                    // Refresh UI
                    OnPropertyChanged(nameof(Works));

                    foreach (var work in Works)
                    {
                        if (work.KidsData == null || !work.KidsData.Any())
                        {
                            InitializeKidsData(work.Id);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    LoggerHelper.LogError(GetType().Name, nameof(LoadWorksAsync), ex.Message, null, ex.StackTrace);
                    await DisplayAlertAsync("Fehler", "Einsätze konnten nicht geladen werden");
                });
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = false;
                    IsLoading = false;
                    LoadingController.StopLoading();
                });
            }
        }

        private async Task LoadMoreAsync()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoadingMore = true;
                });

                var newWorks = await CacheService.GetWorksPageFromCache(_currentPage);

                if (newWorks != null && newWorks.Any())
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var work in newWorks)
                        {
                            Works.Add(work);
                        }
                    });
                }

                // Refresh pagination Info
                var result = await _workService.GetAllWorks(_currentPage);
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
                await DisplayAlertAsync("Error", $"Error loading more items: {ex.Message}");
                LoggerHelper.LogError(GetType().Name, nameof(LoadMoreAsync), ex.Message, null, ex.StackTrace);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoadingMore = false;
                });
            }
        }

        private async Task UpdateWork(Work work)
        {
            if (SelectedWork == null)
            {
                return;
            }

            try
            {
                IsLoading = true;
                LoadingController.StartLoading("Der Vorgang wird durchgeführt...");

                // Prepare work data for update
                PrepareWorkForUpdate(SelectedWork);

                // Update work on the server
                var result = await _workService.UpdateWork(SelectedWork);

                if (result != null && result.Success)
                {
                    var updatedWork = result.Data;
                    
                    // Reload all works
                    await LoadWorksAsync();
                    OnPropertyChanged(nameof(Works));

                    // After reloading, re-select the updated work
                    SelectedWork = Works.FirstOrDefault(w => w.Id == updatedWork?.Id);

                    // Update local data
                    UpdateLocalWork(SelectedWork);

                    // Notify UI of changes
                    RefreshUI();

                    // Cache direkt aktualisieren
                    await CacheService.RefreshHomePageCache();
                    await DisplayAlertAsync("Erfolg", "Einsatz erfolgreich aktualisiert");
                }
                else
                {
                    await DisplayAlertAsync("Fehler", result?.Message);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(CompleteWork), $"Error handling PDF: {ex.Message}", null, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }
            finally
            {
                LoadingController.StopLoading();
                IsLoading = false;
            }
        }

        private async Task CompleteWork(Work work)
        {
            if (SelectedWork == null)
            {
                return;
            }

            if (!ValidateWork(work))
            {
                return;
            }

            try
            {
                // UI-Updates im Main Thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = true;
                    LoadingController.StartLoading("Der Vorgang wird durchgeführt...");
                });

                bool answer = await DisplayAlertWithActionAsync(
                    "Wichtiger Hinweis",
                    "Bitte beachten Sie, dass das Abschließen von diesem Einsatz nicht rückgängig gemacht werden kann. " +
                    "Wenn Sie fortfahren, können Sie die Eingaben nach Abschluss den Einsatz nicht mehr ändern. " +
                    "Bitte überprüfen Sie die Eingaben und klicken Sie auf „Weiter“, wenn Sie den Einsatz abschließen möchten.");

                if (!answer)
                {
                    return;
                }

                // Prepare work data for complete
                var workToUpdate = await PrepareWorkForComplete(SelectedWork);

                // Update work on the server
                var result = await _workService.CompleteWork(SelectedWork, workToUpdate.fileName, workToUpdate.pdfBytes);

                if (result == null || !result.Success)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlertAsync("Fehler", result?.Message);
                    });
                    return;
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await LoadWorksAsync();
                    OnPropertyChanged(nameof(Works));

                    // After reloading, re-select the updated task
                    SelectedWork = Works.FirstOrDefault(t => t.Id == SelectedWork.Id);
                    UpdateLocalWork(SelectedWork);
                    RefreshUI();
                });

                await CacheService.RefreshHomePageCache();

                // PDF-Handling for Admin-User ( Projektkoordinator ) 
                if (App.UserDetails != null && App.UserDetails.Role_ID == (int)UserRole.Admin)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            string destinationFolder = _pathService?.LocalWorksForAdminFolder
                                ?? throw new InvalidOperationException("LocalWorksForAdminFolder is not set.");

                            string destinationPath = Path.Combine(destinationFolder, workToUpdate.fileName);

                            // Ensure the directory exists
                            Directory.CreateDirectory(destinationFolder);

                            // Write the PDF to the file
                            await File.WriteAllBytesAsync(destinationPath, workToUpdate.pdfBytes);

                            // Check if the file exists before attempting to open it
                            if (File.Exists(destinationPath))
                            {
                                var file = new FileResult(destinationPath);
                                var pdfRequest = new OpenFileRequest("Open PDF", file);

                                LoadingController.StopLoading();
                                LoadingController.StartLoading("Einsatz PDF-Download wird geöffnet...");

                                await Task.Delay(2000);
                                await Launcher.OpenAsync(pdfRequest);

                                await DisplayAlertAsync("Erfolg", "Einsatz erfolgreich abgeschlossen");
                            }
                            else
                            {
                                await DisplayAlertAsync("Fehler", "PDF-Datei nach Erstellung nicht gefunden");
                            }
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlertAsync("Fehler", "PDF konnte nicht geöffnet werden");
                            LoggerHelper.LogError(this.GetType().Name, nameof(CompleteWork), $"Error handling PDF: {ex.Message}", null, ex.StackTrace);
                        }
                    });
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await DisplayAlertAsync("Erfolg", "Einsatz erfolgreich aktualisiert");
                    });
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
                });

                LoggerHelper.LogError(GetType().Name, nameof(LoadMoreAsync), ex.Message, null, ex.StackTrace);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingController.StopLoading();
                    IsLoading = false;
                });
            }
        }
        
        private async Task ExecuteCreatePdfAsync()
        {
            if (SelectedWork == null || string.IsNullOrEmpty(SelectedWork.Date))
            {
                await DisplayAlertAsync("Fehler", "Kein Einsatz oder PDF-Pfad verfügbar.");
                return;
            }

            bool isSuccess = false;

            try
            {
                LoadingController.StartLoading("Herunterladen von PDF...");
                string formattedDate = DateTime.Parse(SelectedWork.Date).ToString("yyyy-MM-dd");
                string fileName = $"{formattedDate}-{SelectedWork.Ort}-Einsatz.pdf";

                // Generate PDF and get base64 string
                string base64Pdf = await PdfHelper.GenerateWorkPdf("Plan", SelectedWork);

                // Convert base64 string to byte array
                byte[] pdfBytes = Convert.FromBase64String(base64Pdf);

                // Save PDF to database
                await _workService.SavePdfToDatabaseAsync(fileName, pdfBytes, SelectedWork.Id);

                if (App.UserDetails != null && App.UserDetails.Role_ID != 2)
                {
                    isSuccess = true;
                    LoadingController.StopLoading();
                    await DisplayAlertAsync("Erfolg", "PDF erfolgreich erstellt und gespeichert!");
                    return;
                }

                string destinationFolder = _pathService?.LocalWorksForAdminFolder
                   ?? throw new InvalidOperationException("LocalWorksForAdminFolder is not set.");

                string destinationPath = Path.Combine(destinationFolder, fileName);

                // Ensure the directory exists
                Directory.CreateDirectory(destinationFolder);

                // Write the PDF to the file
                File.WriteAllBytes(destinationPath, pdfBytes);

                isSuccess = true;

                // Check if the file exists before attempting to open it
                if (File.Exists(destinationPath))
                {
                    try
                    {
                        var file = new FileResult(destinationPath);
                        var pdfRequest = new OpenFileRequest("Open PDF", file);
                        await Task.Delay(2000);
                        await Launcher.OpenAsync(pdfRequest);

                        LoadingController.StartLoading("Einsatz PDF-Download initiiert...");
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.LogError(this.GetType().Name, nameof(ExecuteCreatePdfAsync), $"Error opening PDF: {ex.Message}", null, ex.StackTrace);
                        await DisplayAlertAsync("Fehler", "PDF konnte nicht geöffnet werden");
                    }
                }
                else
                {
                    await DisplayAlertAsync("Fehler", "PDF-Datei nach Erstellung nicht gefunden");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(this.GetType().Name, nameof(ExecuteCreatePdfAsync), $"An error occurred: {ex.Message}", null, ex.StackTrace);
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => { IsLoading = false; });

                if (isSuccess)
                {
                    LoadingController.StopLoading();
                    await DisplayAlertAsync("Erfolg", "PDF erfolgreich erstellt und gespeichert");
                }
                else
                {
                    LoadingController.StopLoading();
                    await DisplayAlertAsync("Fehler", "PDF kann nicht erstellt und gespeichert werden");
                }

                LoadingController.StopLoading();
            }
        }

        private void SelectWork(Work work)
        {
            if (work == null) return;

            IsWorkSelected = true;
            SelectedWork = work;
            SelectedTeam = work?.Team;

            KidsData = work?.KidsData != null ? new ObservableCollection<WorkKidsData>(work.KidsData) : new ObservableCollection<WorkKidsData>();

            ListOfHelpers = work?.ListOfHelpers != null ? new ObservableCollection<string>(work.ListOfHelpers) : new ObservableCollection<string>();

            if (work?.KidsData != null && work.KidsData.Any())
            {
                // Merge existing data with default data
                var mergedData = new List<WorkKidsData>();
                foreach (var defaultData in KidsData)
                {
                    var existingData = work.KidsData.FirstOrDefault(k => k.AgeGroupId == defaultData.AgeGroupId);
                    if (existingData != null)
                    {
                        mergedData.Add(existingData);
                    }
                    else
                    {
                        mergedData.Add(defaultData);
                    }
                }
            }
            else
            {
                if (work != null)
                {
                    InitializeKidsData(work.Id);
                } 
            }
            CalculateTotals();

            KidsData = new ObservableCollection<WorkKidsData>(work?.KidsData ?? new List<WorkKidsData>());

            if(work != null)
            {
                work.Reflection = work.Reflection;
                work.Defect = work.Defect;
                work.ParentContact = work.ParentContact;
                work.WellbeingOfChildren = work.WellbeingOfChildren;
                work.Notes = work.Notes;
                work.Wishes = work.Wishes; // Partizipations
                work.EndWork = work.EndWork;
                work.StartWork = work.StartWork;
            }
        }

        private void AddHelper()
        {
            if (!IsAddingHelper)
            {
                IsAddingHelper = true;
                NewHelperName = string.Empty;
            }
            else
            {
                // Store the new helper
                if (!string.IsNullOrWhiteSpace(NewHelperName))
                {
                    ListOfHelpers.Add(NewHelperName);
                    UpdateSelectedWorkHelpers();
                }
                // Reset the adding state
                IsAddingHelper = false;
                NewHelperName = string.Empty;
            }
        }

        private void RemoveHelper(string helper)
        {
            if (ListOfHelpers.Contains(helper))
            {
                ListOfHelpers.Remove(helper);
                if (SelectedWork?.ListOfHelpers != null)
                {
                    SelectedWork.ListOfHelpers.Remove(helper);
                }
            }
        }

        private void UpdateHelper((string oldHelper, string newHelper) helperUpdate)
        {
            if (SelectedWork != null)
            {
                int index = ListOfHelpers.IndexOf(helperUpdate.oldHelper);
                if (index != -1)
                {
                    ListOfHelpers[index] = helperUpdate.newHelper;

                    // Update SelectedWork.Helpers if it exists
                    if (SelectedWork.ListOfHelpers != null && index < SelectedWork.ListOfHelpers.Count)
                    {
                        SelectedWork.ListOfHelpers[index] = helperUpdate.newHelper;
                    }
                    else
                    {
                        // If SelectedWork.Helpers doesn't exist or is too short, create or extend it
                        SelectedWork.ListOfHelpers = new List<string>(ListOfHelpers);
                    }
                }
            }
        }

        private void OnUpdateDate(string newDate)
        {
            if (SelectedWork != null && DateTime.TryParse(newDate, out DateTime date))
            {
                SelectedWork.Date = date.ToString("yyyy-MM-dd");
            }
        }

        private void OnUpdateTeam(string newTeam)
        {
            if (SelectedWork != null)
            {
                SelectedWork.Team = newTeam;
            }
        }

        private void OnUpdateOrt(string newOrt)
        {
            if (SelectedWork != null)
            {
                SelectedWork.Ort = newOrt;
            }
        }

        private void OnUpdatePlan(string newPlan)
        {
            if (SelectedWork != null)
            {
                SelectedWork.Plan = newPlan;
            }
        }

        private void CalculateAll()
        {
            TotalBoys = KidsDataUI?.Sum(d => d.NumberBoys);
            TotalGirls = KidsDataUI?.Sum(d => d.NumberGirls);
            TotalKids = TotalBoys + TotalGirls;

            OnPropertyChanged(nameof(TotalBoys));
            OnPropertyChanged(nameof(TotalGirls));
            OnPropertyChanged(nameof(TotalKids));
        }

        public void CalculateTotals()
        {
            TotalBoys = KidsDataUI?.Sum(k => k.NumberBoys) ?? 0;
            TotalGirls = KidsDataUI?.Sum(k => k.NumberGirls) ?? 0;
            TotalKids = TotalBoys + TotalGirls;
        }
        #endregion

        #region utils
        private bool ValidateWork(Work work)
        {
            if (string.IsNullOrWhiteSpace(work.Ort))
            {
                DisplayValidationError("Bitte geben Sie den Ort der Einsatz ein.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(work.Plan))
            {
                DisplayValidationError("Bitte geben Sie den Plan ein.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(work.Team))
            {
                DisplayValidationError("Bitte wählen Sie ein Team aus.");
                return false;
            }

            if (string.IsNullOrEmpty(work.Reflection))
            {
                DisplayValidationError("Bitte geben Sie die Reflexion ein.");
                return false;
            }



            // Check if at least one age group has children
            bool hasAnyKids = KidsDataUI?.Any(k => k.NumberBoys > 0 || k.NumberGirls > 0) ?? false;
            if (!hasAnyKids)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayValidationError("Bitte geben Sie die Anzahl der Kinder ein."));
                return false;
            }

            if (string.IsNullOrWhiteSpace(work.StartWork) || string.IsNullOrWhiteSpace(work.EndWork))
            {
                DisplayValidationError("Bitte geben Sie sowohl die Start- als auch die Endzeit der Einsatz an.");
                return false;
            }
            return true; // Validation passed
        }

        private void PrepareWorkForUpdate(Work work)
        {
            work.Date = SelectedDate.ToString("yyyy-MM-dd");
            work.StartWork = FormatTimeForServer(work.StartWork);
            work.EndWork = FormatTimeForServer(work.EndWork);

            work.ListOfHelpers = ListOfHelpers.ToList();

            work.Defect = work.Defect ?? string.Empty;
            work.ParentContact = work.ParentContact ?? string.Empty;
            work.WellbeingOfChildren = work.WellbeingOfChildren ?? string.Empty;
            work.Notes = work.Notes ?? string.Empty;
            work.Wishes = work.Wishes ?? string.Empty; // Partizipation
            work.KidsData = KidsData?.ToList() ?? new List<WorkKidsData>();
        }

        private async Task<(string fileName, byte[] pdfBytes)> PrepareWorkForComplete(Work work)
        {
            PrepareWorkForUpdate(work);

            string formattedDate = work.Date != null ? DateTime.Parse(work.Date).ToString("yyyy-MM-dd") : string.Empty;
            string fileName = $"{formattedDate}-{work.Ort}-Einsatz.pdf";

            // Generate PDF and get base64 string
            string base64Pdf = await PdfHelper.GenerateWorkPdf("Plan", work);

            // Convert base64 string to byte array
            byte[] pdfBytes = Convert.FromBase64String(base64Pdf);

            return (fileName, pdfBytes);
        }

        private void UpdateLocalWork(Work? updatedWork)
        {
            if (updatedWork == null) return;

            if (updatedWork.KidsData != null)
            {
                KidsDataUI = new ObservableCollection<KidsDataWrapper>(
                    updatedWork.KidsData.Select(k => new KidsDataWrapper(k))
                );
            }
            else
            {
                // Initialize with empty data if KidsData is null
                EnsureKidsDataInitialized(updatedWork);
            }
        }

        private List<Work> SortWorks(ObservableCollection<Work> works)
        {
            return works
                .OrderBy(j => j.Status == "complete")
                .ThenByDescending(j => j.Status == "inprogress" ? j.UpdatedAt : DateTime.MinValue)
                .ThenByDescending(j => j.Status == "standing" ? j.UpdatedAt : DateTime.MinValue)
                .ThenByDescending(j => j.UpdatedAt)
                .ToList();
        }

        private void ClearWorkData()
        {
            ListOfHelpers = new ObservableCollection<string>();
            SelectedDate = DateTime.Now;
            KidsData?.Clear();
            _helperChanges?.Clear();

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsComplete));
            OnPropertyChanged(nameof(OrtAndDate));
        }
        private void UpdateSelectedWorkData(Work work)
        {
            if (work == null) return;

            SelectedDate = DateTime.TryParse(work.Date, out DateTime parsedDate)
                ? parsedDate
                : DateTime.Now;

            // Initialise List of helpers
            ListOfHelpers = new ObservableCollection<string>(work.ListOfHelpers ?? new List<string>());
            _helperChanges.Clear();

            // Initialise KidsData
            EnsureKidsDataInitialized(work);
            KidsData = new ObservableCollection<WorkKidsData>(work.KidsData ?? new List<WorkKidsData>());

            // UI refresh
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsComplete));
            OnPropertyChanged(nameof(OrtAndDate));
        }

        private void RefreshUI()
        {
            OnPropertyChanged(nameof(Works));
            OnPropertyChanged(nameof(SelectedWork));
            OnPropertyChanged(nameof(KidsData));
            OnPropertyChanged(nameof(KidsDataUI));
            OnPropertyChanged(nameof(ListOfHelpers));
        }

        private string? FormatTimeForServer(string? time)
        {
            if (DateTime.TryParse(time, out var parsedTime))
            {
                return parsedTime.ToString("HH:mm");
            }
            return time;
        }

        private async void DisplayValidationError(string message)
        {
            await DisplayAlertAsync("Validation Error", message);
        }

        private void UpdateButtonColors()
        {
            if (Application.Current != null)
            {

                if (IsAddingHelper)
                {
                    Application.Current.Resources["ButtonBackgroundColor"] = Colors.Green;
                    Application.Current.Resources["ButtonTextColor"] = Colors.White;
                }
                else
                {
                    Application.Current.Resources["ButtonBackgroundColor"] = Colors.Blue;
                    Application.Current.Resources["ButtonTextColor"] = Colors.White;
                }
            }
        }

        private void UpdateSelectedWorkHelpers()
        {
            if (SelectedWork != null)
            {
                SelectedWork.ListOfHelpers = new List<string>(ListOfHelpers);
            }
        }

        private void ListOfHelpers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedWorkHelpers();
        }

        private void EnsureKidsDataInitialized(Work work)
        {
            if (work == null) return;
            if (work.KidsData == null || !work.KidsData.Any())
            {
                work.KidsData = new List<WorkKidsData>
            {
                new WorkKidsData { WorkId = work.Id, AgeRange = "3-5", AgeGroupId = 1, NumberBoys = 0, NumberGirls = 0 },
                new WorkKidsData { WorkId = work.Id, AgeRange = "6-9", AgeGroupId = 2, NumberBoys = 0, NumberGirls = 0 },
                new WorkKidsData { WorkId = work.Id, AgeRange = "10-13", AgeGroupId = 3, NumberBoys = 0, NumberGirls = 0 },
                new WorkKidsData { WorkId = work.Id, AgeRange = "14+", AgeGroupId = 4, NumberBoys = 0, NumberGirls = 0 }
            };
            }

            KidsDataUI = new ObservableCollection<KidsDataWrapper>(
                work.KidsData.Select(k => new KidsDataWrapper(k))
            );
        }

        private string GetAgeRangeForAgeGroupId(int ageGroupId)
        {
            return ageGroupId switch
            {
                1 => "3-5",
                2 => "6-9",
                3 => "10-13",
                4 => "14+",
                _ => "Unknown"
            };
        }

        public void OnExpanderTapped(object sender, EventArgs e)
        {
            if (!IsLoading)
            {
                IsExpanderOpen = !IsExpanderOpen;
            }
        }

        private void InitializeKidsData(int workId)
        {
            KidsData = new ObservableCollection<WorkKidsData>
            {
                new WorkKidsData { WorkId = workId, AgeRange = "3-5", AgeGroupId = 1, NumberBoys = 0, NumberGirls = 0 },
                new WorkKidsData { WorkId = workId, AgeRange = "6-9", AgeGroupId = 2, NumberBoys = 0, NumberGirls = 0 },
                new WorkKidsData { WorkId = workId, AgeRange = "10-13", AgeGroupId = 3, NumberBoys = 0, NumberGirls = 0 },
                new WorkKidsData { WorkId = workId, AgeRange = "14+", AgeGroupId = 4, NumberBoys = 0, NumberGirls = 0 }
            };
        }

        // Unsubscribe from WorkMessage when existing the page
        public void OnDisappearing()
        {
            WeakReferenceMessenger.Default.Unregister<WorkMessage>(this);
        }
        #endregion
    }
}
