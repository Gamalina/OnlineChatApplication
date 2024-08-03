using System.ComponentModel.DataAnnotations;

namespace OnlineChatApplication.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
