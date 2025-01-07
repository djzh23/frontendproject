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
    public partial class CreateWorkPageViewModel : BaseViewModel
    {
        private readonly IWorkService _workService;
        public LoadingController LoadingController { get; private set; }

        public CreateWorkPageViewModel(IWorkService workService, IConnectivityService connectivityService)
        {
            _workService = workService;
            ConnectivityService = connectivityService;
            LoadingController = new LoadingController();

            Helpers = new ObservableCollection<string>();
            Startwork = new TimeSpan(12, 0, 0);

            CreateWorkCommand = new Command(async () => await ExecuteCreateWorkAsync());
            AddHelperCommand = new Command<string>(ExecuteAddHelper);
            DeleteHelperCommand = new Command<string>(ExecuteDeleteHelper);
            CancelWorkCommand = new Command(ResetForm);
        }

        #region properties
        [ObservableProperty]
        private DateTime _date = DateTime.Today;

        [ObservableProperty]
        private bool _isVorOrt;

        [ObservableProperty]
        private string? _selectedTeam;

        [ObservableProperty]
        private string? _ort;

        [ObservableProperty]
        private TimeSpan _startwork;

        [ObservableProperty]
        private ObservableCollection<string> _helpers;

        [ObservableProperty]
        private string? _newHelper;

        [ObservableProperty]
        private string? _plan;
        #endregion

        #region commands
        public ICommand CreateWorkCommand { get; }
        public ICommand AddHelperCommand { get; }
        public ICommand DeleteHelperCommand { get; }
        public ICommand CancelWorkCommand { get; }
        #endregion

        #region tasks
        private async Task ExecuteCreateWorkAsync()
        {
            if (!ValidateWork())
            {
                await DisplayAlertAsync("Fehler", "Bitte füllen Sie alle erforderlichen Felder für den Einsatz aus");
                return;
            }

            LoadingController.StartLoading("Einsatz wird erstellt");

            var work = new Work(
               date: Date,
               status: null,
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
                    // Send message using WeakReferenceMessenger to update cache after creation
                    WeakReferenceMessenger.Default.Send(new WorkMessage());
                    WeakReferenceMessenger.Default.Send(new HomePageMessage());

                    ResetForm();
                    LoadingController.StopLoading();

                    await DisplayAlertAsync("Erfolg", "Einsatz erfolgreich erstellt");
                }
                else
                {
                    await DisplayAlertAsync("Fehler", result.Message);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
                LoggerHelper.LogError(GetType().Name, nameof(ExecuteCreateWorkAsync), ex.Message, new { work }, ex.StackTrace);
            }
            finally
            {
                LoadingController.StopLoading();
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

        private void ResetForm()
        {
            SelectedTeam = null;
            Helpers.Clear();
            IsVorOrt = false;
            Date = DateTime.Today;
            Ort = string.Empty;
            Plan = string.Empty;
            Startwork = TimeSpan.Zero;
        }
        #endregion

        #region utils
        private bool ValidateWork()
        {
            return !string.IsNullOrWhiteSpace(SelectedTeam)
                && !string.IsNullOrWhiteSpace(Ort)
                && !string.IsNullOrWhiteSpace(Plan);
        }
        #endregion
    }
}
