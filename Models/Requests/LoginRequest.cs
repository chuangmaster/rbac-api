using System.ComponentModel.DataAnnotations;

namespace RbacApi.Models.Requests;

public class LoginRequest
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
