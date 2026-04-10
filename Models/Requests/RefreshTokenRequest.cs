using System.ComponentModel.DataAnnotations;

namespace RbacApi.Models.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
