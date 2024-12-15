using ppm_fe.Models;

namespace ppm_fe.Services;
public interface IAuthService
{
    Task<ApiResponse<User>> Register(User user);

    Task<ApiResponse<object>> Login(string email, string password);

    Task<ApiResponse<object>> Logout();

    Task<bool> ForgetPassword(string email);

    User GetLoggedIntUser();

    Task<ApiResponse<List<UserSummary>>> GetAllUsers();

    Task<ApiResponse<object>> ApproveRole(int userId, int? roleId);

    Task<ApiResponse<object>> DisapproveUser(int userId);

    Task<ApiResponse<User>> GetUserProfile();

    Task<ApiResponse<User>> UpdateProfile(User user);
}
