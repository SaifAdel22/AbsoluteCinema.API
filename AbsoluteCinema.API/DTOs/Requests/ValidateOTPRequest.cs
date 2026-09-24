using System.ComponentModel.DataAnnotations;

namespace AbsoluteCinema.API.DTOs.Requests
{

    public class ValidateOTPRequest
    {
        [Required]
        public string OTP { get; set; } = string.Empty;
    }
}
