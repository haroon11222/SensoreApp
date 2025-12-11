using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using SensoreApp.Data;
using SensoreApp.Models;

namespace SensoreApp.Controllers
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); 
            }
            
            string hashedPassword = HashPassword(model.Password);
            
            var user = _context.Users
                .FirstOrDefault(u => u.Email == model.Email && 
                                     u.PasswordHash == hashedPassword);

            if (user != null)
            {
                if (user.Role == "Patient")
                {
                    // Successful login redirects to the Patient Dashboard
                    return RedirectToAction("Dashboard", "Patient"); 
                }
                
                // For other roles, just redirect to home for now
                 return RedirectToAction("Index", "Home"); 
            }
            
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    FullName = model.FullName,
                    PasswordHash = HashPassword(model.Password),
                    Role = "Patient" 
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful. You can now log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }
        
        // --- NEW: Logout Action ---
        // Handles the Logout request and redirects to Login
        [HttpGet]
        public IActionResult Logout()
        {
            // In a real application, you would sign out the user's authentication scheme here.
            TempData["LogoutMessage"] = "You have been logged out.";
            return RedirectToAction("Login", "Account");
        }
    }
}