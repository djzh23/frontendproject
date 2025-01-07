using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;

namespace ppm_fe.ViewModels.Startup;

public partial class RegisterPageViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    public RegisterPageViewModel(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        RegisterCommand = new AsyncRelayCommand(Register);
    }

    #region properties
    [ObservableProperty]
    private bool _isRegistrationEnabled = true;

    [ObservableProperty]
    private string? _firstname;

    [ObservableProperty]
    private string? _lastname;

    [ObservableProperty]
    private string? _number;

    [ObservableProperty]
    private string? _street;

    [ObservableProperty]
    private string? _plz;

    [ObservableProperty]
    private string? _city;

    [ObservableProperty]
    private string? _country;

    [ObservableProperty]
    private string? _steueridentifikationnummer;

    [ObservableProperty]
    private string? _bankname;

    [ObservableProperty]
    private string? _bic;

    [ObservableProperty]
    private string? _iban;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _passwordrepeat;
    #endregion

    #region Commands
    public IAsyncRelayCommand RegisterCommand { get; }
    #endregion

    #region tasks
    private async Task Register()
    {
        IsLoading = true;
        IsRegistrationEnabled = false;

        if (
            string.IsNullOrWhiteSpace(Firstname)
            || string.IsNullOrWhiteSpace(Lastname)
            || string.IsNullOrWhiteSpace(Street)
            || string.IsNullOrWhiteSpace(Plz)
            || string.IsNullOrWhiteSpace(Country)
            || string.IsNullOrWhiteSpace(Number)
            || string.IsNullOrWhiteSpace(City)
            || string.IsNullOrWhiteSpace(Steueridentifikationnummer)
            || string.IsNullOrWhiteSpace(Bankname)
            || string.IsNullOrWhiteSpace(Iban)
            || string.IsNullOrWhiteSpace(Email)
            || string.IsNullOrWhiteSpace(Password)
            || string.IsNullOrWhiteSpace(Passwordrepeat)
            )
        {
            IsLoading = false;
            await DisplayAlertAsync("Fehler", "Alle Felder sind erforderlich");

            return;
        }

        if (Password != Passwordrepeat)
        {
            IsLoading = false;
            await DisplayAlertAsync("Fehler", "Passwort und Passwortwiederholung müssen gleich sein");
            return;
        }

        var user = new User
        {
            Firstname = InputCleaner.SanitizeInput(Firstname),
            Lastname = InputCleaner.SanitizeInput(Lastname),
            Email = InputCleaner.CleanEmail(Email),
            Password = Password,
            Street = Street,
            Pzl = Plz,
            City = City,
            Country = Country,
            Steueridentifikationsnummer = Steueridentifikationnummer,
            bank_name = Bankname,
            Number = FormatPhoneNumber(),
            Iban = InputCleaner.CleanIBAN(Iban ?? string.Empty) ?? Iban,
            Bic = InputCleaner.CleanBIC(Bic ?? string.Empty) ?? Bic,
            Approved = false
        };

        var result = await _authService.Register(user);
        if (!result.Success)
        {
            IsLoading = false;
            await DisplayAlertAsync("Fehler", $"{result.Message}");
        }
        else
        {
            IsLoading = false;
            ResetForm();

            await DisplayAlertAsync("Registrierung", "Konto erfolgreich erstellt! Willkommen im Board!");
        }

        IsRegistrationEnabled = true;
    }
    #endregion

    #region utils
    private void ResetForm()
    {
        Firstname = null;
        Lastname = null;
        Number = null;
        Street = null;
        Plz = null;
        City = null;
        Country = null;
        Steueridentifikationnummer = null;
        Bankname = null;
        Bic = null;
        Iban = null;
        Email = null;
        Password = null;
        Passwordrepeat = null;
    }

    private string FormatPhoneNumber()
    {
        if (string.IsNullOrWhiteSpace(Number))
            return string.Empty;

        // Remove any non-digit characters
        string digitsOnly = new string(Number.Where(char.IsDigit).ToArray());

        // Add the "+" prefix if it's not already there
        if (!digitsOnly.StartsWith("+"))
        {
            return "+" + digitsOnly;
        }

        return digitsOnly;
    }
    #endregion
}
