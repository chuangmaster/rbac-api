using RbacApi.Models.Responses;

namespace RbacApi.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string employeeId, string password);
}
