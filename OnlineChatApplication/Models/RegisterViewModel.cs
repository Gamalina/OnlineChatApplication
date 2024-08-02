using System.ComponentModel.DataAnnotations;

namespace OnlineChatApplication.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Biography { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public int Age { get; set; }
    }
}
