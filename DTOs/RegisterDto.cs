using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [MinLength(2)]
        public string username { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(15)]
        public string password { get; set; }
    }
}