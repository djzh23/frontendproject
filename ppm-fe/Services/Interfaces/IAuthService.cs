﻿using ppm_fe.Models;

namespace ppm_fe.Services;
public interface IAuthService
{
    User GetLoggedIntUser();

    Task<ApiResponse<User>> Register(User user);

    Task<ApiResponse<object>> Login(string email, string password);

    Task<ApiResponse<object>> Logout();

    Task<ApiResponse<object>> ForgetPassword(string email);

    Task<ApiResponse<List<UserSummary>>> GetAllUsers();

    Task<ApiResponse<object>> ApproveUser(int userId, int? roleId);

    Task<ApiResponse<object>> DisapproveUser(int userId);

    Task<ApiResponse<object>> ChangeUserRole(int userId, int? roleId);

    Task<ApiResponse<User>> GetUserProfile();

    Task<ApiResponse<User>> UpdateUserProfile(User user);
}
