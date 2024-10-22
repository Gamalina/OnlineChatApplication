﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineChatApplication.Data;
using OnlineChatApplication.Models;
using OnlineChatApplication.MailService;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                Email = model.Email,
                Username = model.Username,
                PasswordHash = ComputeSha256Hash(model.Password),
                ResetToken = string.Empty
            };

            var userProfile = new UserProfile
            {
                Biography = model.Biography,
                Gender = model.Gender,
                Age = model.Age,
                User = user
            };

            _context.Users.Add(user);
            _context.UserProfiles.Add(userProfile);
            await _context.SaveChangesAsync();

            return RedirectToAction("RegisterConfirmation");
        }

        return View(model);
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user != null && VerifyPasswordHash(model.Password, user.PasswordHash))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        var hash = ComputeSha256Hash(password);
        return hash == storedHash;
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
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
                user.ResetToken = string.Empty;
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

    public IActionResult RegisterConfirmation()
    {
        return View();
    }
}