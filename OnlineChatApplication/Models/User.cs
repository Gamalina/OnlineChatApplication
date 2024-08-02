using System.ComponentModel.DataAnnotations;

namespace OnlineChatApplication.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string ResetToken { get; internal set; }

        public UserProfile UserProfile { get; set; }

    }
}
