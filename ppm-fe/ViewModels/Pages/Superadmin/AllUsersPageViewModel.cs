using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Newtonsoft.Json;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;

namespace ppm_fe.ViewModels.Pages;

public partial class AllUsersPageViewModel : BaseViewModel, INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    public LoadingController LoadingController { get; private set; }

    public AllUsersPageViewModel(IAuthService authService, IConnectivityService connectivityService)
    {
        _authService = authService;
        ConnectivityService = connectivityService;
        LoadingController = new LoadingController();

        RefreshUsersCommand = new Command(async () => await LoadUsersAsync());
        ApproveUserCommand = new Command<UserSummary>(async (user) => await ApproveUser(user));
        DisapproveUserCommand = new Command<UserSummary>(DisapproveUser);
        ChangeUserCommand = new Command<UserSummary>(async (user) => await ChangeUser(user));

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadUsersAsync();
        });
    }

    #region properties
    public ObservableCollection<UserSummary> Users { get; } = [];
    #endregion

    #region commands
    public ICommand? RefreshUsersCommand { get; }
    public ICommand? ApproveUserCommand { get; }
    public ICommand? DisapproveUserCommand { get; }
    public ICommand? ChangeUserCommand { get; }
    #endregion

    #region tasks
    private async void DisapproveUser(UserSummary user)
    {
        if (IsLoading) return;

        var roleId = RoleHelper.GetRoleId(user.Role?.Name);

        try
        {
            LoadingController.StartLoading("Benuterkonto wird deaktiviert...");

            var response = await _authService.DisapproveUser(user.Id);
            user.Approved = response.Success ? !user.Approved : user.Approved;

            var alertType = response.Success ? "Erfolg" : "Fehler";
            LoadingController.StopLoading();
            await DisplayAlertAsync(alertType, response.Message);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(DisapproveUser), ex.Message, new { user.Id, roleId }, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.Properties.UnexpectedError);
        }
        finally
        {
            await LoadUsersAsync();
            LoadingController.StopLoading();
        }
    }

    private async Task ApproveUser(UserSummary user)
    {
        if (IsLoading) return;

        var roleId = RoleHelper.GetRoleId(user.Role?.Name);

        // Check if the selected role is unknown (5)
        if (roleId == (int)UserRole.NoRole)
        {
            await DisplayAlertAsync(
                "Ungültige Rollenauswahl",
                "Bitte wählen Sie für die Genehmigung eine andere Rolle als Unbekannt."
            );
            return;
        }

        try
        {
            LoadingController.StartLoading("Benuterkonto wird aktiviert...");

            var response = await _authService.ApproveUser(user.Id, roleId);
            var alertType = response.Success ? "Erfolg" : "Fehler";
            LoadingController.StopLoading();
            await DisplayAlertAsync(alertType, response.Message);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(ApproveUser), ex.Message, new { user.Id, roleId }, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.Properties.UnexpectedError);
        }
        finally
        {
            await LoadUsersAsync();
            LoadingController.StopLoading();
        }
    }

    private async Task ChangeUser(UserSummary user)
    {
        if (IsLoading) return;

        var roleId = RoleHelper.GetRoleId(user.Role?.Name);

        // Check if the selected role is 5
        if (roleId == (int)UserRole.NoRole)
        {
            await DisplayAlertAsync(
                "Ungültige Rollenauswahl",
                "Bitte wählen Sie für die Genehmigung eine andere Rolle als Unbekannt."
            );
            return;
        }

        try
        {
            LoadingController.StartLoading("Benuterkonto wird geändert...");

            var response = await _authService.ChangeUserRole(user.Id, roleId);
            var alertType = response.Success ? "Erfolg" : "Fehler";
            LoadingController.StopLoading();
            await DisplayAlertAsync(alertType, response.Message);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(ChangeUser), ex.Message, new { user.Id, roleId }, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.Properties.UnexpectedError);
        }
        finally
        {
            await LoadUsersAsync();
            LoadingController.StopLoading();
        }
    }

    public async Task LoadUsersAsync()
    {
        if (IsLoading) return;

        try
        {
            LoadingController.StartLoading("Benutzern wird geladen");

            MainThread.BeginInvokeOnMainThread(() => Users.Clear());

            var response = await _authService.GetAllUsers();

            if (response?.Success == true)
            {
                if (response.Data?.Count > 0)
                {
                    var user_data = await SecureStorage.GetAsync("user_data");
                    var userId = !string.IsNullOrEmpty(user_data)
                        ? JsonConvert.DeserializeObject<User>(user_data)?.Id
                        : null;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        foreach (var user in response.Data)
                        {
                            if (user.Id != userId && user.Role != null)
                            {
                                user.Role.Name = RoleHelper.GetRoleName(user.Role.Id);
                                Users.Add(user);
                            }
                        }
                    });
                }
                else
                {
                    await DisplayAlertAsync("Hinweis", "Keine Benutzer gefunden.");
                }
            }
            else
            {
                await DisplayAlertAsync("Fehler", "Fehler beim Laden der Benutzer.");
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(LoadUsersAsync), ex.Message, null, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Constants.Properties.UnexpectedError);
        }
        finally
        {
            LoadingController.StopLoading();
        }
    }
    #endregion
}
