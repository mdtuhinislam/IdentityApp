using System.ComponentModel.DataAnnotations;

namespace IdentityApp.DTOs.Account
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage ="Maximum {1} and minimum {2} charecter ")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Maximum {1} and minimum {2} charecter ")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression("^[\\w-\\.]+@[\\w-\\.]+\\.[a-z]{2,4}$", ErrorMessage ="Invalid email address")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
