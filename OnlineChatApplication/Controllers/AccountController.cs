using Microsoft.AspNetCore.Mvc;
using OnlineChatApplication.Models;
using System.Text;
using System.Security.Cryptography;
using OnlineChatApplication.Data;

namespace OnlineChatApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string email)
        {
            var user = new User
            {
                Username = username,
                PasswordHash = ComputeSha256Hash(password),
                Email = email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
