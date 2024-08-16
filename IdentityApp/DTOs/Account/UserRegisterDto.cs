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
        [RegularExpression("^\\w+@[a-zA-Z_]+?\\.[a-zA-Z]{2,3}$", ErrorMessage ="Invalid email address")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
