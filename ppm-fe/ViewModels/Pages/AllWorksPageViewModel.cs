using CommunityToolkit.Mvvm.ComponentModel;
using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;

namespace ppm_fe.ViewModels.Pages
{
    public partial class AllWorksPageViewModel : BaseViewModel
    {
        private readonly ICacheService _cacheService;
        private readonly IWorkService _workService;
        private readonly ILocalPathService? _pathService;
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

        private bool _hasMoreWorks = true;
        public bool HasMoreWorks
        {
            get => _hasMoreWorks;
            set
            {
                _hasMoreWorks = value;
                OnPropertyChanged(nameof(HasMoreWorks));
            }
        }


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
        public bool IsNotLoading => !IsLoading;


        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isWorkSelected;
        public bool IsWorkSelected
        {
            get => _isWorkSelected;
            set
            {
                if (_isWorkSelected != value)
                {
                    _isWorkSelected = value;
                    OnPropertyChanged();
                }
            }
        }


        private Work _selectedWork;
        public Work SelectedWork
        {
            get => _selectedWork;
            set
            {
                _selectedWork = value;
                OnPropertyChanged();

                if (IsWorkSelected)
                {
                    UpdateSelectedWorkData();
                    OnPropertyChanged(nameof(IsWorkSelected));
                    ListOfHelpers = new ObservableCollection<string>(_selectedWork.ListOfHelpers ?? new List<string>());
                    _helperChanges.Clear();

                    if (DateTime.TryParse(_selectedWork.Date, out DateTime parsedDate))
                    {
                        SelectedDate = parsedDate;
                    }
                    else
                    {
                        SelectedDate = DateTime.Now; // Default to current date if parsing fails
                    }

                    Console.WriteLine($"SelectedWork set: {(_isWorkSelected == null ? "null" : _selectedWork.Status)}");
                    OnPropertyChanged(nameof(SelectedWork));
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(IsComplete));
                }
                else
                {
                    ListOfHelpers = new ObservableCollection<string>();
                    SelectedDate = DateTime.Now; // Reset to current date when no work is selected
                }
            }
        }


        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    if (SelectedWork != null)
                    {
                        SelectedWork.Date = value.ToString("yyyy-MM-dd"); // Convert DateTime to string
                    }
                }
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
                    if (SelectedWork != null)
                    {
                        SelectedWork.Team = value;
                    }
                }
            }
        }


        private string _updatedAt;
        public string UpdatedAt
        {
            get => _updatedAt;
            set
            {
                if (_updatedAt != value)
                {
                    _updatedAt = value;
                    OnPropertyChanged();
                }
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(OrtAndDate)); // Notify that the combined property has changed
                    OnPropertyChanged(nameof(SelectedTeam));
                }
            }
        }

        private ObservableCollection<WorkKidsData> _kidsData;
        public ObservableCollection<WorkKidsData> KidsData
        {
            get => _kidsData;
            set
            {
                _kidsData = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<string> _listOfHelpers;
        public ObservableCollection<string> ListOfHelpers
        {
            get => _listOfHelpers;
            set
            {
                _listOfHelpers = value;
                OnPropertyChanged();
            }
        }

        private bool _isAddingHelper;
        public bool IsAddingHelper
        {
            get => _isAddingHelper;
            set
            {
                _isAddingHelper = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AddHelperButtonText));
                UpdateButtonColors();  // Add this line
            }
        }


        private string _startWork;
        public string StartWork
        {
            get => _startWork;
            set
            {
                if (_startWork != value)
                {
                    _startWork = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartWorkTimeString));
                }
            }
        }


        private string _newHelperName;
        public string NewHelperName
        {
            get => _newHelperName;
            set
            {
                _newHelperName = value;
                OnPropertyChanged();
            }
        }


        private bool _isExpanderOpen;
        public bool IsExpanderOpen
        {
            get => _isExpanderOpen;
            set
            {
                _isExpanderOpen = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private bool? ort;

        [ObservableProperty]
        private List<string>? teamsList;

        [ObservableProperty]
        private int? totalBoys;

        [ObservableProperty]
        private int? totalGirls;

        [ObservableProperty]
        private int? totalKids;

        // Neue Wrapper-Collection für die UI
        private ObservableCollection<KidsDataWrapper> _kidsDataUI;
        public ObservableCollection<KidsDataWrapper> KidsDataUI
        {
            get => _kidsDataUI;
            set => SetProperty(ref _kidsDataUI, value);
        }

        // Wrapper-Klasse (innerhalb des ViewModels)
        public class KidsDataWrapper : ObservableObject
        {
            private readonly WorkKidsData _data;

            public KidsDataWrapper(WorkKidsData data)
            {
                _data = data;
            }
            
            public string AgeRange => _data.AgeRange;

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
        }

    public string OrtAndDate => $"Ort: {Ort}, Date: {Date:yyyy-MM-dd}";

        public string Status => SelectedWork?.Status;

        public bool IsComplete => string.Equals(Status, "complete", StringComparison.OrdinalIgnoreCase);

        private Dictionary<string, string> _helperChanges = new Dictionary<string, string>();

        public string StartWorkTimeString => DateTimeOffset.TryParse(StartWork, out var dto)
            ? dto.LocalDateTime.ToString("HH:mm")
            : string.Empty;

        public string AddHelperButtonText => IsAddingHelper ? "Helfer speichern" : "Neuen Helfer einfügen";

        // Command properties
        public Command AddHelperCommand { get; private set; }
        public Command<string> RemoveHelperCommand { get; private set; }
        public Command<(string oldHelper, string newHelper)> UpdateHelperCommand { get; private set; }
        public Command LoadWorksCommand { get; private set; }
        public Command FetchMoreWorksCommand { get; private set; }
        public Command<Work> SelectWorkCommand { get; private set; }
        public Command<Work> UpdateWorkCommand { get; private set; }
        public Command<string> UpdateDateCommand { get; private set; }
        public Command<string> UpdateTeamCommand { get; private set; }
        public Command<string> UpdateOrtCommand { get; private set; }
        public Command<string> UpdatePlanCommand { get; private set; }
        public Command CalculateAllCommand { get; private set; }
        public Command CalculateCommand { get; private set; }
        public Command DownloadPdfCommand { get; private set; }
        public ICommand LoadMoreCommand { get; }

        public AllWorksPageViewModel(ICacheService cacheService, IWorkService workService, ILocalPathService pathService, IConnectivityService connectivityService)
        {
            ConnectivityService = connectivityService;
            _cacheService = cacheService;
            _workService = workService;
            _pathService = pathService;

            LoadingController = new LoadingController();

            InitializeCollections();
            InitializeCommands();
            UpdateButtonColors();

            ListOfHelpers.CollectionChanged += ListOfHelpers_CollectionChanged;

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

        private async Task LoadWorksAsync()  
        {
            IsLoading = true;
            Works.Clear();

            // Reset selected work ( sonst wird der alte Work von den vorherigen eingeloggten User angezeigt )
            IsWorkSelected = false;
            SelectedWork = null;
            try
            {
                _currentPage = 1;

                var result = await _workService.GetAllWorks();

                Works = new ObservableCollection<Work>(result.Data);
                if(Works.Count == 0)
                {
                    await DisplayAlertAsync("Fehler", "Keine Einsätze gefundend");
                    return;

                }
                if(Works.Any())
                {
                    IsWorkSelected = true;
                    SelectedWork = Works.First();
                }

                var sortedWorks = SortWorks(result.Data);

                Works = new ObservableCollection<Work>(sortedWorks);
                OnPropertyChanged(nameof(Works));


                // Ensure KidsData is initialized for each work
                foreach (var work in Works)
                {
                    if (work.KidsData == null || !work.KidsData.Any())
                    {
                        InitializeKidsData(work.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(LoadWorksAsync), $"An error occurred: {ex.Message}", null, ex.StackTrace);
                await DisplayAlertAsync("Fehler", "Einsätze konnten nicht geladen werden");
            }
            finally
            {
                IsBusy = false;
                IsLoading = false;
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
                string base64Pdf = await PdfHelper.GeneratePdfWithTable("Plan", SelectedWork);

                // Convert base64 string to byte array
                byte[] pdfBytes = Convert.FromBase64String(base64Pdf);

                // Save PDF to database
                await _workService.SavePdfToDatabaseAsync(fileName, pdfBytes, SelectedWork.Id);

                if (App.UserDetails.Role_ID != 2)
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
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => { IsBusy = false; });

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

        private async Task UpdateWorkAsync(Work work)
        {
            if (SelectedWork == null)
            {
                return;
            }

            if (!ValidateWork(work))
            {
                return; // Validation failed, so exit early
            }

            try
            {
                IsBusy = true;

                // Prepare work data for update
                PrepareWorkForUpdate(SelectedWork);

                // Update work on the server
                var result = await _workService.UpdateWork(SelectedWork);
                if (result != null && result.Success)
                {
                    var updatedWork = result.Data;


                    // Update local data
                    UpdateLocalWork(updatedWork);

                    // Log the server response
                    LogServerResponse(updatedWork);

                    // Notify UI of changes
                    RefreshUI();

                    // Reload all works
                    LoadWorksCommand.Execute(null);

                    // After reloading, re-select the updated work
                    SelectedWork = Works.FirstOrDefault(j => j.Id == updatedWork.Id);


                    // Cache direkt aktualisieren
                    await _cacheService.RefreshDashboardCache();

                    // Erst Message senden
                    MessagingCenter.Send(this, MessageKeys.WorksChanged);

                    await DisplayAlertAsync("Erfolg", "Einsatz erfolgreich aktualisiert");
                }
                else
                {
                    await DisplayAlertAsync("Fehler", "Aktualisierung der Einsatz fehlgeschlagen");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Fehler", ErrorMessage.UnexpectedError);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<Work> SortWorks(List<Work> works)
        {
            return works
                .OrderBy(j => j.Status == "complete")
                .ThenByDescending(j => j.Status == "standing" ? j.UpdatedAt : DateTime.MinValue)
                .ThenByDescending(j => j.UpdatedAt)
                .ToList();
        }

        private void SelectWork(Work work)
        {
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
                InitializeKidsData(work.Id);
            }
            CalculateTotals();

            KidsData = new ObservableCollection<WorkKidsData>(work.KidsData);

            work.Reflection = work.Reflection;
            work.Defect = work.Defect;
            work.ParentContact = work.ParentContact;
            work.WellbeingOfChildren = work.WellbeingOfChildren;
            work.Notes = work.Notes;
            work.Wishes = work.Wishes;
            work.EndWork = work.EndWork;
            work.StartWork = work.StartWork;
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

        public void CalculateTotals()
        {
            TotalBoys = KidsDataUI?.Sum(k => k.NumberBoys) ?? 0;
            TotalGirls = KidsDataUI?.Sum(k => k.NumberGirls) ?? 0;
            TotalKids = TotalBoys + TotalGirls;
        }

        private void CalculateAll()
        {
            TotalBoys = KidsDataUI.Sum(d => d.NumberBoys);
            TotalGirls = KidsDataUI.Sum(d => d.NumberGirls);
            TotalKids = TotalBoys + TotalGirls;

            OnPropertyChanged(nameof(TotalBoys));
            OnPropertyChanged(nameof(TotalGirls));
            OnPropertyChanged(nameof(TotalKids));
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
            work.Wishes = work.Wishes ?? string.Empty;

            work.KidsData = KidsData.ToList();
        }

        private void UpdateLocalWork(Work updatedWork)
        {
            var index = Works.IndexOf(Works.FirstOrDefault(j => j.Id == updatedWork.Id));
            if (index != -1)
            {
                Works[index] = updatedWork;
            }
            SelectedWork = updatedWork;
        }

        private void LogServerResponse(Work updatedWork)
        {
            Console.WriteLine($"Server response: {JsonSerializer.Serialize(updatedWork, new JsonSerializerOptions { WriteIndented = true })}");
        }

        private void RefreshUI()
        {
            OnPropertyChanged(nameof(Works));
            OnPropertyChanged(nameof(SelectedWork));
            OnPropertyChanged(nameof(KidsData));
            OnPropertyChanged(nameof(ListOfHelpers));
        }

        private string FormatTimeForServer(string time)
        {
            if (DateTime.TryParse(time, out var parsedTime))
            {
                return parsedTime.ToString("HH:mm");
            }
            return time; // Return original if parsing fails
        }
        
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

            if(string.IsNullOrEmpty(work.Reflection))
            {
                DisplayValidationError("Bitte geben Sie die Reflexion ein.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(work.StartWork) || string.IsNullOrWhiteSpace(work.EndWork))
            {
                DisplayValidationError("Bitte geben Sie sowohl die Start- als auch die Endzeit der Einsatz an.");
                return false;
            }

            return true; // Validation passed
        }

        private async void DisplayValidationError(string message)
        {
           await DisplayAlertAsync("Validation Error", message);
        }

        private void UpdateButtonColors()
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

        private void AddHelper()
        {
            if (!IsAddingHelper)
            {
                // Start adding a new helper
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

        private void UpdateSelectedWorkHelpers()
        {
            if (SelectedWork != null)
            {
                SelectedWork.ListOfHelpers = new List<string>(ListOfHelpers);
            }
        }

        private void ListOfHelpers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedWorkHelpers();
        }

        private void UpdateSelectedWorkData()
        {

            if (DateTime.TryParse(_selectedWork.Date, out DateTime parsedDate))
            {
                SelectedDate = parsedDate;
            }
            else
            {
                SelectedDate = DateTime.Now; // Default to current date if parsing fails
            }


            ListOfHelpers = new ObservableCollection<string>(SelectedWork.ListOfHelpers ?? new List<string>());
            _helperChanges.Clear();

            EnsureKidsDataInitialized(SelectedWork);
            KidsData = new ObservableCollection<WorkKidsData>(SelectedWork.KidsData);

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsComplete));
        }

        private void EnsureKidsDataInitialized(Work work)
        {

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

            // Wrapper-Objekte erstellen
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
            if (!IsLoading) // Only toggle if not loading
            {
                IsExpanderOpen = !IsExpanderOpen;
            }
        }

        private void InitializeCollections()
        {
            Works = new ObservableCollection<Work>();
            TeamsList = new List<string> { "FF1", "FF2", "FF3", "FF4", "FF5", "FF6" };
            ListOfHelpers = new ObservableCollection<string>();
        }

        private void InitializeCommands()
        {
            AddHelperCommand = new Command(AddHelper);
            RemoveHelperCommand = new Command<string>(RemoveHelper);
            UpdateHelperCommand = new Command<(string oldHelper, string newHelper)>(UpdateHelper);
            LoadWorksCommand = new Command(async () => await LoadWorksAsync());
            UpdateWorkCommand = new Command<Work>(async (work) => await UpdateWorkAsync(work));
            SelectWorkCommand = new Command<Work>(SelectWork);
            UpdateDateCommand = new Command<string>(OnUpdateDate);
            UpdateTeamCommand = new Command<string>(OnUpdateTeam);
            UpdateOrtCommand = new Command<string>(OnUpdateOrt);
            UpdatePlanCommand = new Command<string>(OnUpdatePlan);
            CalculateAllCommand = new Command(CalculateAll);
            CalculateCommand = new Command(CalculateTotals);
            DownloadPdfCommand = new Command(async () => await ExecuteCreatePdfAsync());
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
            MessagingCenter.Unsubscribe<AllWorksPageViewModel>(this, MessageKeys.WorksChanged);
        }

        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                _currentPage = 1;

                // Hole nur die erste Seite
                var works = await _cacheService.GetWorksPageFromCache(_currentPage);

                Works.Clear();
                foreach (var work in works)
                {
                    Works.Add(work);
                }

                // Pagination Info aktualisieren
                var result = await _workService.GetAllWorks(_currentPage);
                if (result?.Pagination != null)
                {
                    _paginationInfo = result.Pagination;
                    _currentPage++;
                }

                OnPropertyChanged(nameof(HasMoreItems));
                OnPropertyChanged(nameof(TotalItems));
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
            if (IsBusy || !HasMoreItems) return;

            try
            {
                IsBusy = true;

                var nextPageWorks = await _cacheService.GetWorksPageFromCache(_currentPage);

                foreach (var work in nextPageWorks)
                {
                    Works.Add(work);
                }

                _currentPage++;
                OnPropertyChanged(nameof(HasMoreItems));
                (LoadMoreCommand as Command)?.ChangeCanExecute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading more items: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
