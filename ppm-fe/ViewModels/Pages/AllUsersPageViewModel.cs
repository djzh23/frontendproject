using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ppm_fe.ViewModels.Pages;

public partial class AllUsersPageViewModel : BaseViewModel, INotifyPropertyChanged
{
    private readonly IAuthService _authService;

    public int Id { get; set; }

    public string? Name { get; set; }


    private bool _approved;
    public bool Approved
    {
        get => _approved;
        set
        {
            if (_approved != value)
            {
                _approved = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<UserSummary> Users { get; } = [];

    private bool _isApprovedAndHasRole;
    public bool IsApprovedAndHasRole
    {
        get => _isApprovedAndHasRole;
        set
        {
            if (_isApprovedAndHasRole != value)
            {
                _isApprovedAndHasRole = value;
                OnPropertyChanged(nameof(IsApprovedAndHasRole));
            }
        }
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ICommand? RefreshUsersCommand { get; }
    public ICommand? UpdateUserRoleCommand { get; }
    public ICommand? ToggleApprovalCommand { get; }

    public ICommand? ApproveCommand { get; }
    public ICommand? DisapproveCommand { get; }


    public AllUsersPageViewModel(IAuthService authService, IConnectivityService connectivityService)
    {
        ConnectivityService = connectivityService;

        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        RefreshUsersCommand = new Command(async () => await LoadUsersAsync());

        UpdateUserRoleCommand = new Command<UserSummary>(async (user) => await UpdateUserRole(user));
        DisapproveCommand = new Command<UserSummary>(DisapproveUser);

        Task.Run(async () => await LoadUsersAsync());
    }

    private async void DisapproveUser(UserSummary user)
    {
        if (IsBusy) return;
        IsBusy = true;
        IsRefreshing = true;

        try
        {
            var result = await _authService.DisapproveUser(user.Id);
            if (result.Success)
            {
                user.Approved = !user.Approved;
                string message = user.Approved ? "Das Konto wurde erfolgreich genehmigt!" : "Das Konto wurde erfolgreich abgelehnt!";
                await DisplayAlertAsync("Confirmation", message);
            }
            else
            {
                string message = $"Benutzer kann nicht {(user.Approved ? "abgelehnt" : "genehmigt")} werden.";
                await DisplayAlertAsync("Fehler", message);
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(DisapproveUser), ex.Message, new { user.Id }, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.ErrorMessage.UnexpectedError);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
            await LoadUsersAsync();
        }
    }

    private async Task UpdateUserRole(UserSummary user)
    {
        if (IsBusy) return;
        IsBusy = true;
        IsRefreshing = true;

        var roleId = RoleHelper.GetRoleId(user.Role?.Name);

        try
        {
            // Check if the selected role is 5
            if (roleId == 5)
            {
                await DisplayAlertAsync(
                    "Ungültige Rollenauswahl",
                    "Bitte wählen Sie für die Genehmigung eine andere Rolle als Unbekannt."
                );
                return;
            }

            var response = await _authService.ApproveRole(user.Id, roleId);
            if (response.Success)
            {
                await DisplayAlertAsync("Confirmation", "Die Benutzerrolle wurde erfolgreich geändert!");
            }
            else
            {
                await DisplayAlertAsync("Fehler", "Die Benutzerrolle kann nicht aktualisiert werden!");
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(UpdateUserRole), ex.Message, new { user.Id, roleId }, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.ErrorMessage.UnexpectedError);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
            await LoadUsersAsync();

        }
    }

    public async Task LoadUsersAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        IsRefreshing = true;
        Users.Clear();

        try
        {
            var usersResponse = await _authService.GetAllUsers();

            if (usersResponse.Success)
            {
                if (usersResponse?.Data != null && usersResponse.Data.Count > 0)
                {
                    var user_data = await SecureStorage.GetAsync("user_data");
                    var userId = JsonConvert.DeserializeObject<User>(user_data)?.Id;

                    foreach (var user in usersResponse.Data)
                    {
                        if (user.Id != userId && user.Role != null)
                        {
                            user.Role.Name = RoleHelper.GetRoleName(user.Role.Id);
                            Users.Add(user);
                        }
                    }
                }
                else
                {
                    await DisplayAlertAsync("Fehler", "Es wurden keine Benutzer gefunden oder es konnten keine Benutzer geladen werden.");
                }
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(LoadUsersAsync), ex.Message, null, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.ErrorMessage.UnexpectedError);
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}
