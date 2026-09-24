using System.ComponentModel.DataAnnotations;

namespace AbsoluteCinema.API.DTOs.Requests;

public class ForgetPasswordRequest
{
    [Required]
    [Display(Name = "Email Or UserName")]
    public string EmailOrUserName { get; set; } = string.Empty;
}
