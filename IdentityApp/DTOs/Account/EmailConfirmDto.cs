using System.ComponentModel.DataAnnotations;

namespace IdentityApp.DTOs.Account
{
    public class EmailConfirmDto
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [RegularExpression("^[\\w-\\.]+@[\\w-\\.]+\\.[a-z]{2,4}$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
    }
}
