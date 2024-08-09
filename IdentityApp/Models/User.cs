using System;
using Microsoft.AspNetCore.Identity;

namespace IdentityApp.Models
{
    public class User: IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
