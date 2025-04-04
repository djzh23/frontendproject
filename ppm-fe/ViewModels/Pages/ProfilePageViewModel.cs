﻿using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ppm_fe.Constants;
using ppm_fe.Controls;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.Services;

namespace ppm_fe.ViewModels.Pages;

public partial class ProfilePageViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    public LoadingController LoadingController { get; private set; }

    public ProfilePageViewModel(IAuthService authService, IConnectivityService connectivityService)
    {
        ConnectivityService = connectivityService;
        _authService = authService;
        LoadingController = new LoadingController();

        RefreshProfileCommand = new Command(async () => await FetchUserData());
        UpdateCommand = new Command(async () => await UpdateUserData());
    }

    #region properties
    [ObservableProperty]
    private User? _user;
    #endregion

    #region commands
    public ICommand UpdateCommand { get; }
    public ICommand RefreshProfileCommand { get; }
    #endregion

    #region tasks
    public async Task FetchUserData()
    {
        if (IsLoading) return;

        try
        {
            LoadingController.StartLoading("Profile informationen wird geladen...");

            var response = await _authService.GetUserProfile();
            if (response.Success)
            {
                if (response.Data != null)
                {
                    User = new User
                    {
                        Firstname = response.Data.Firstname,
                        Lastname = response.Data.Lastname,
                        Email = response.Data.Email,
                        Id = response.Data.Id,
                        Steueridentifikationsnummer = response.Data.Steueridentifikationsnummer,
                        Street = response.Data.Street,
                        Number = response.Data.Number,
                        Pzl = response.Data.Pzl,
                        City = response.Data.City,
                        Country = response.Data.Country,
                        bank_name = response.Data.bank_name,
                        Iban = response.Data.Iban,
                        Bic = response.Data.Bic
                    };
                }
                else
                {
                    await DisplayAlertAsync("Fehler", "Kein Benutzerprofil gefunden oder Profil kann nicht geladen werden.");
                }
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(FetchUserData), ex.Message, null, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
        }
        finally
        {
            LoadingController.StopLoading();
        }
    }

    private async Task UpdateUserData()
    {
        if (IsLoading) return;

        try
        {
            LoadingController.StartLoading("Profile informationen wird erstellt...");

            if (User != null)
            {
                var user = new User
                {
                    Firstname = User.Firstname,
                    Lastname = User.Lastname,
                    Email = User.Email,
                    Id = User.Id,
                    Steueridentifikationsnummer = User.Steueridentifikationsnummer,
                    Street = User.Street,
                    Number = User.Number,
                    Pzl = User.Pzl,
                    City = User.City,
                    Country = User.Country,
                    bank_name = User.bank_name,
                    Iban = User.Iban,
                    Bic = User.Bic
                };

                // Update user data
                var response = await _authService.UpdateUserProfile(user);
                if (response.Success)
                {
                    await DisplayAlertAsync("Konfirmation", "Das Profil wurde erfolgreich geändert!!");
                    await FetchUserData();
                }
                else
                {
                    await DisplayAlertAsync("Fehler", response.Message);
                }
            }
            else
            {
                await DisplayAlertAsync("Fehler", "Keine zu aktualisierenden Benutzerdaten gefunden.");
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(FetchUserData), ex.Message, User, ex.StackTrace);
            await DisplayAlertAsync("Fehler", Properties.UnexpectedError);
        }
        finally
        {
            LoadingController.StopLoading();
        }
    }
    #endregion
}
