using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using ppm_fe.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ppm_fe.ViewModels.Pages
{
    public partial class CreateWorkPageViewModel : BaseViewModel
    {
        private readonly IWorkService _workService;
        private readonly ICacheService _cacheService;

        public ObservableCollection<string> Teams { get; } = new ObservableCollection<string> { "FF1", "FF2", "FF3", "FF4", "FF5" };
        private CancellationTokenSource tokenSource = new CancellationTokenSource();


        private string _selectedTeam;
        public string SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                _selectedTeam = value;
                OnPropertyChanged();
            }
        }


        private string _newHelper;
        public string NewHelper
        {
            get => _newHelper;
            set
            {
                _newHelper = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<string> _helpers;
        public ObservableCollection<string> Helpers
        {
            get => _helpers;
            set
            {
                _helpers = value;
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


        private TimeSpan _startwork;
        public TimeSpan Startwork
        {
            get => _startwork;
            set
            {
                if (_startwork != value)
                {
                    _startwork = value;
                    OnPropertyChanged(nameof(Startwork));
                }
            }
        }


        private string _ort;
        public string Ort
        {
            get => _ort;
            set
            {
                _ort = value;
                OnPropertyChanged();
            }
        }


        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }


        private string _plan;
        public string Plan
        {
            get => _plan;
            set
            {
                _plan = value;
                OnPropertyChanged();
            }
        }


        private bool _isCreated;
        public bool IsCreated
        {
            get => _isCreated;
            set
            {
                _isCreated = value;
                OnPropertyChanged();
            }
        }


        private Work _currentWork;
        public Work CurrentWork
        {
            get => _currentWork;
            set
            {
                _currentWork = value;
                OnPropertyChanged();
            }
        }


        public ICommand CreateWorkCommand { get; }
        public ICommand AddHelperCommand { get; }
        public ICommand DeleteHelperCommand { get; }
        public ICommand CancelWorkCommand { get; }


        public CreateWorkPageViewModel(IWorkService workService, ICacheService cacheService, IConnectivityService connectivityService)
        {
            _workService = workService;
            _cacheService = cacheService;
            ConnectivityService = connectivityService;

            Helpers = new ObservableCollection<string>();
            CurrentWork = new Work();
            Startwork = new TimeSpan(12, 0, 0);

            CreateWorkCommand = new Command(async () => await ExecuteCreateWorkAsync());
            AddHelperCommand = new Command<string>(ExecuteAddHelper);
            DeleteHelperCommand = new Command<string>(ExecuteDeleteHelper);
            CancelWorkCommand = new Command(ResetForm);
        }


        private async Task ExecuteCreateWorkAsync()
        {
            if (!ValidateWork())
            {
                await DisplayAlertAsync("Fehler", "Bitte füllen Sie alle erforderlichen Felder für den Einsatz aus");
                IsBusy = false;
                return;
            }

            IsBusy = true;
            var work = new Work(
               date: Date,
               status: Status,
               team: SelectedTeam,
               ort: Ort,
               vorort: IsVorOrt,
               plan: Plan,
               startWork: Startwork,
               listofhelpers: Helpers.ToList()
           );

            try
            {
                var result = await _workService.CreateWorkWithoutAgeGroups(work);
                if (result.Success)
                {
                    // Cache direkt aktualisieren
                    await _cacheService.RefreshWorksCache();
                    await _cacheService.RefreshDashboardCache();

                    // Erst Message senden
                    MessagingCenter.Send(this, MessageKeys.WorksChanged);


                    await DisplayAlertAsync("Erfolg", "Einsatz erfolgreich erstellt");

                    ResetForm();
                }
                else
                {
                    await DisplayAlertAsync("Fehler", "Einsatz konnte nicht erstellt werden");
                }
            }
            catch(Exception ex)
            {
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteCreateWorkAsync), ex.Message, new { work }, ex.StackTrace);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecuteAddHelper(string helper)
        {
            if (!string.IsNullOrWhiteSpace(helper) && !Helpers.Contains(helper))
            {
                Helpers.Add(helper);
                NewHelper = string.Empty; // Clear the input field
            }
        }

        private void ExecuteDeleteHelper(string helper)
        {
            if (Helpers.Contains(helper))
            {
                Helpers.Remove(helper);
            }
        }

        private bool ValidateWork()
        {
            return !string.IsNullOrWhiteSpace(SelectedTeam)
                && !string.IsNullOrWhiteSpace(Ort)
                && !string.IsNullOrWhiteSpace(Plan);
        }

        private void ResetForm()
        {
            SelectedTeam = null;
            Helpers.Clear();
            IsVorOrt = false;
            Date = DateTime.Today;
            Ort = string.Empty;
            Plan = string.Empty;
            Startwork = TimeSpan.Zero;
            IsCreated = false;
            CurrentWork = new Work();
        }
    }
}
