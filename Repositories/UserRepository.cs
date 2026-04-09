using Dapper;
using RbacApi.Infrastructure;
using RbacApi.Models.Entities;
using RbacApi.Repositories.Interfaces;

namespace RbacApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _db;

    public UserRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmployeeIdAsync(string employeeId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<User>(
            """
            SELECT id, employee_id AS EmployeeId, name, password_hash AS PasswordHash,
                   is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM users
            WHERE employee_id = @EmployeeId AND is_active = TRUE
            """,
            new { EmployeeId = employeeId });
    }

    public async Task<List<string>> GetPermissionsByUserIdAsync(int userId)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryAsync<string>(
            """
            SELECT DISTINCT p.code
            FROM permissions p
            INNER JOIN role_permissions rp ON rp.permission_id = p.id
            INNER JOIN user_roles ur ON ur.role_id = rp.role_id
            WHERE ur.user_id = @UserId
            ORDER BY p.code
            """,
            new { UserId = userId });
        return result.ToList();
    }
}
