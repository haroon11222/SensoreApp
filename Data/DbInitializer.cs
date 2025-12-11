using SensoreApp.Models; 
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Linq; 

namespace SensoreApp.Data // Defines the SensoreApp.Data namespace
{
    public static class DbInitializer
    {
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;   
            }

            var patient = new User
            {
                UserId = 1, 
                Email = "patient@sensore.com", // Updated email domain for consistency
                PasswordHash = HashPassword("password"), 
                Role = "Patient",
                FullName = "Sensore Patient User"
            };
            
            context.Users.Add(patient);
            
            context.SaveChanges();
        }
    }
}