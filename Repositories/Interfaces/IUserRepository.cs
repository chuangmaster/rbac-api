using RbacApi.Models.Entities;

namespace RbacApi.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmployeeIdAsync(string employeeId);
    Task<List<string>> GetPermissionsByUserIdAsync(int userId);
}
