using System.ComponentModel.DataAnnotations;

namespace OnlineChatApplication.Models
{
    public class ResetPasswordRequestViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
