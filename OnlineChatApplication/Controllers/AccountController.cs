using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChatApplication.Data;
using OnlineChatApplication.Models;
using OnlineChatApplication.MailService;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public AccountController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult ResetPasswordRequest()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPasswordRequest(ResetPasswordRequestViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                var resetToken = Guid.NewGuid().ToString();
                user.ResetToken = resetToken;
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Scheme);
                var message = $"Please reset your password by clicking here: {resetLink}";

                await _emailService.SendEmailAsync(model.Email, "Password Reset", message);
            }

            return RedirectToAction("ResetPasswordRequestConfirmation");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        var model = new ResetPasswordViewModel { Token = token };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == model.Token);
            if (user != null)
            {
                user.PasswordHash = ComputeSha256Hash(model.NewPassword);
                user.ResetToken = null;
                await _context.SaveChangesAsync();
                return RedirectToAction("ResetPasswordConfirmation");
            }

            ModelState.AddModelError(string.Empty, "Invalid token.");
        }

        return View(model);
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

    public IActionResult ResetPasswordRequestConfirmation()
    {
        return View();
    }

    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}