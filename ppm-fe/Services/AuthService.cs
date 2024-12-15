
using ppm_fe.Constants;
using ppm_fe.Helpers;
using ppm_fe.Models;
using ppm_fe.ViewModels;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace ppm_fe.Services;

public class AuthService : BaseViewModel, IAuthService
{
    private const string TokenKey = "token";
    
    public string? _storedToken;

    private User? _myUser;
    public User? MyUser
    {
        get => _myUser;
        set
        {
            _myUser = value;
            OnPropertyChanged();
        }
    }

    private HttpClient _client = new()
    {
        BaseAddress = new Uri(RouteHelper.BaseUrl),
    };


    //REGISTER - SERVICE FUNCTION
    public async Task<ApiResponse<User>> Register(User user)
    {
        string route = RouteHelper.GetFullUrl("/register");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{route}"),
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(user), Encoding.UTF8, "application/json")
        };

        // Set the Accept header to application/json
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<User>>(jsonResponse);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new ApiResponse<User>
                {
                    Success = false,
                    Message = responseResult?.Message,
                    Data = null
                };
                
            }
            if (responseResult != null)
            {
                return responseResult;
            }
        }
        catch (Exception ex)
        {
            // Log the error
            LoggerHelper.LogError(GetType().Name, nameof(Register), ex.Message, new { user }, ex.StackTrace);
        }

        return new ApiResponse<User>
        {
            Success = false,
            Message = ErrorMessage.UnexpectedError,
            Data = null
        };
    }


    //LOGIN - SERVICE FUNCTION
    public async Task<ApiResponse<object>> Login(string email, string password)
    {

        string route = RouteHelper.GetFullUrl("/login");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{route}"),
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { email, password }), Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(jsonResponse);

            if (responseResult != null && responseResult.Success)
            {
                if (responseResult?.Data?.User != null && responseResult.Data.Token != null)
                {
                    _myUser = responseResult.Data.User;
                    _storedToken = responseResult.Data.Token;

                    await SetStoredToken(_storedToken);
                    await SecureStorage.SetAsync("hasAuth", "true");
                    await SaveUser();

                    return new ApiResponse<object>
                    {
                        Success = true,
                        Message = responseResult.Message,
                        Data = responseResult.Data
                    }; 
                }
                else
                {
                    LoggerHelper.LogError(this.GetType().Name, nameof(Login), "Login response or token is null", new { email }, string.Empty);
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Anmeldeantwort oder Token ist ungültig.",
                        Data = null
                    };
                }
            }
            if (responseResult != null && !responseResult.Success)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = responseResult.Message,
                    Data = null
                };
            }
        }
        catch (HttpRequestException ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(Login), $"Network error: {ex.Message}", new { email }, ex.StackTrace);

        }
        catch (JsonException ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(Login), $"Error parsing response: {ex.Message}", new { email }, ex.StackTrace);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(this.GetType().Name, nameof(Login), ex.Message, new { email }, ex.StackTrace);
        }

        return new ApiResponse<object>
        {
            Success = false,
            Message = ErrorMessage.UnexpectedError,
            Data = null
        };
    }


    //FORGETPASSWORD - SERVICE FUNCTION
    public async Task<bool> ForgetPassword(string email)
    {
        string route = RouteHelper.GetFullUrl("/forgot-password");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{route}"),
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { email }), Encoding.UTF8, "application/json")
        };
      
        try
        {
            var response = await _client.SendAsync(request);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var forgetPasswordResponse = JsonConvert.DeserializeObject<ApiResponse<ForgetPasswordResponse>>(jsonResponse);

            if (forgetPasswordResponse != null)
            {
                var message = forgetPasswordResponse.Message ?? "Unexpected error during login.";
                if (forgetPasswordResponse.Success)
                {
                    
                    await DisplayAlertAsync("Success", message);
                    return true;
                }
                else
                {
                    await DisplayAlertAsync("error", message);
                }
            }
            else
            {
                await DisplayAlertAsync("error", "Invalid response.");
            }
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlertAsync("Error", $"Network error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            await DisplayAlertAsync("Error", $"Error parsing response: {ex.Message}");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"An unexpected error occurred: {ex.Message}");
        }

        return false;
    }


    //LOGOUT - SERVICE FUNCTION
    public async Task<ApiResponse<object>> Logout()
    {
        var user_data = await SecureStorage.GetAsync("user_data");
        var user = !string.IsNullOrEmpty(user_data) ? JsonConvert.DeserializeObject<User>(user_data) : null;

        string route = RouteHelper.GetFullUrl("/logout");
        var request = new HttpRequestMessage(HttpMethod.Post, route);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("token"));

        try
        {
            //-----------------This is needed when desktop not logout successfully ..TO CHECK
            //Preferences.Clear(); 
            var response = await _client.SendAsync(request);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<object>>(jsonResponse);

            if (responseResult != null && responseResult.Success)
            {
                _storedToken = null;
                await SecureStorage.SetAsync("hasAuth", "false");
                SecureStorage.Remove("token");
                SecureStorage.Remove("user_data");
                return responseResult;
            }
            else
            {
                LoggerHelper.LogError(GetType().Name, nameof(Logout), jsonResponse, new { user }, string.Empty);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ErrorMessage.UnexpectedError,
                    Data = null
                };
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(Logout), ex.Message, new { user }, ex.StackTrace);
            return new ApiResponse<object>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }
    }

    //GET ALL USERS - SERVICE FUNCTION
    public async Task<ApiResponse<List<UserSummary>>> GetAllUsers()
    {
        var user_data = await SecureStorage.GetAsync("user_data");
        var user = !string.IsNullOrEmpty(user_data) ? JsonConvert.DeserializeObject<User>(user_data) : null;

        string route = RouteHelper.GetFullUrl("/users");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{route}"),
        };

        try
        {
            var token = await GetStoredToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<List<UserSummary>>>(jsonResponse);


            if (responseResult != null && responseResult.Success)
            {
                return responseResult;
            }
        }

        catch (JsonException ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(GetAllUsers), $"Error parsing response: {ex.Message}", new { user }, ex.StackTrace);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(GetAllUsers), $"Error fetching user data: {ex.Message}", new { user }, ex.StackTrace);
        }

        return new ApiResponse<List<UserSummary>>
        {
            Success = false,
            Message = ErrorMessage.UnexpectedError,
            Data = null
        };
    }


    //UPDATE USER ROLE - SERVICE FUNCTION
    public async Task<ApiResponse<object>> ApproveRole(int userId, int? roleId)
    {
        string route = RouteHelper.GetFullUrl($"/approve/{userId}");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{route}"),
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { approved = 1, role_id = roleId }), Encoding.UTF8, "application/json")
        };

        try
        {
            string? token = await GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No authentication token found. Please log in.");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<object>>(jsonResponse);

            if (responseResult != null && responseResult.Success)
            {
                return responseResult;
            }
            else
            {
                LoggerHelper.LogError(GetType().Name, nameof(ApproveRole), $"Failed to update user role. Status code: {response.StatusCode}", new { userId, roleId }, string.Empty);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ErrorMessage.UnexpectedError,
                    Data = null
                };
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(ApproveRole), ex.Message, new { userId, roleId }, ex.StackTrace);
            return new ApiResponse<object>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(ApproveRole), ex.Message, new { userId, roleId }, ex.StackTrace);
            return new ApiResponse<object>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }
    }


    //DISAPPROVE USER - SERVICE FUNCTION
    public async Task<ApiResponse<object>> DisapproveUser(int userId)
    {
        string route = RouteHelper.GetFullUrl($"/disapprove/{userId}");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{route}"),
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(new { userId }), Encoding.UTF8, "application/json")
        };

        try
        {
            string? token = await GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No authentication token found.");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<object>>(jsonResponse);

            if (responseResult != null && responseResult.Success)
            {
                return responseResult;
            }
            else
            {
                LoggerHelper.LogError(GetType().Name, nameof(DisapproveUser), $"Failed to update user approval status. Status code: {response.StatusCode}", new { userId }, string.Empty);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = ErrorMessage.UnexpectedError,
                    Data = null
                };
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(DisapproveUser), ex.Message, new { userId }, ex.StackTrace);
            return new ApiResponse<object>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(DisapproveUser), ex.Message, new { userId }, ex.StackTrace);
            return new ApiResponse<object>
            {
                Success = false,
                Message = ErrorMessage.UnexpectedError,
                Data = null
            };
        }
    }

    //GET PROFILE USER - SERVICE FUNCTION
    public async Task<ApiResponse<User>> GetUserProfile()
    {
        var user_data = await SecureStorage.GetAsync("user_data");
        var user = !string.IsNullOrEmpty(user_data) ? JsonConvert.DeserializeObject<User>(user_data) : null;

        string route = RouteHelper.GetFullUrl("/user");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{route}"),
        };

        try
        {
            string? token = await GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No authentication token found.");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<User>>(jsonResponse);

            if (responseResult != null && responseResult.Success)
            {
                return responseResult;
            }
            else
            {
                LoggerHelper.LogError(GetType().Name, nameof(GetUserProfile), $"Failed to load user profile. Status code: {response.StatusCode}", new { user }, string.Empty);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(GetUserProfile), ex.Message, new { user }, ex.StackTrace);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(GetUserProfile), ex.Message, new { user }, ex.StackTrace);
        }

        return new ApiResponse<User>
        {
            Success = false,
            Message = ErrorMessage.UnexpectedError,
            Data = null
        };
    }

    //UPDATE USER PROFILE - SERVICE FUNCTION
    public async Task<ApiResponse<User>> UpdateProfile(User user)
    {
        string route = RouteHelper.GetFullUrl("/user");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"{route}"),
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(user), Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _client.SendAsync(request);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            var responseResult = JsonConvert.DeserializeObject<ApiResponse<User>>(jsonResponse);

            if (responseResult != null && responseResult.Success)
            {
                return responseResult;
            }
            if (responseResult != null && !responseResult.Success)
            {
                LoggerHelper.LogError(GetType().Name, nameof(UpdateProfile), $"Failed to update user profile. Status code: {response.StatusCode}", new { user.Email }, string.Empty);
                return new ApiResponse<User>
                {
                    Success = false,
                    Message = responseResult.Message,
                    Data = null
                };
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(UpdateProfile), ex.Message, new { user.Email }, ex.StackTrace);
        }

        return new ApiResponse<User>
        {
            Success = false,
            Message = ErrorMessage.UnexpectedError,
            Data = null
        };
    }

    private async Task<string?> GetStoredToken()
    {
        try
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return token;
        }
        catch (Exception ex)
        {
            // Log the error
            LoggerHelper.LogError(GetType().Name, nameof(SetStoredToken), ex.Message, new { this.MyUser }, ex.StackTrace);
        }

        return null;
    }

    private async Task SetStoredToken(string token)
    {
        try
        {
            await SecureStorage.SetAsync(TokenKey, token);
        }
        catch (Exception ex)
        {
            // Log the error
            LoggerHelper.LogError(GetType().Name, nameof(SetStoredToken), ex.Message, new { this.MyUser }, ex.StackTrace);
        }
    }

    public async Task SaveUser()
    {
        try
        {
            var userDataJson = JsonConvert.SerializeObject(_myUser);
            await SecureStorage.SetAsync("user_data", userDataJson);
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError(GetType().Name, nameof(SaveUser), ex.Message, new { this.MyUser }, ex.StackTrace);
        }
    }

    public User GetLoggedIntUser()
    {
        var t = Preferences.Get("token", string.Empty);
        _storedToken = t;

        if (_myUser != null && _storedToken != null)
        {
            return _myUser;
        }
        else
        {
            throw new InvalidOperationException("User is not logged in.");
        }
    }
}
