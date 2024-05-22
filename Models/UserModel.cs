using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BamukoyaPhotoApp.Models
{
    public class UserModel : IdentityUser<long>
    {
        [Required]
        public required string Name { get; set; }
    }
}
