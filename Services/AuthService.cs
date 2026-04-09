using RbacApi.Models.Responses;
using RbacApi.Repositories.Interfaces;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> LoginAsync(string employeeId, string password)
    {
        var user = await _userRepository.GetByEmployeeIdAsync(employeeId);
        if (user is null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var permissions = await _userRepository.GetPermissionsByUserIdAsync(user.Id);

        var accessToken = _jwtService.GenerateToken(
            subject: user.EmployeeId,
            name: user.Name,
            permissions: permissions);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = _jwtService.GenerateRefreshToken(),
            ExpiresIn = 1800, // 30 minutes
            Permissions = permissions,
        };
    }
}
