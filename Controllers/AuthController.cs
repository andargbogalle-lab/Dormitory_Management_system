using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;
using BC = BCrypt.Net.BCrypt;

namespace dprmitory.Controllers
{
    public class AuthController : Controller
    {
        private readonly StudentRepository _studentRepo;
        private readonly UserRepository _userRepo;

        public AuthController(StudentRepository studentRepo, UserRepository userRepo)
        {
            _studentRepo = studentRepo;
            _userRepo = userRepo;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                TempData["Error"] = "All fields are required!";
                return View();
            }

            // Validate username format based on role
            if (role == "Student")
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
                {
                    TempData["Error"] = "Student ID must contain only letters and numbers!";
                    return View();
                }
            }
            else if (role == "Director" || role == "Proctor")
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z]+$"))
                {
                    TempData["Error"] = "Username must contain only letters!";
                    return View();
                }
            }

            if (role == "Student")
            {
                var student = _studentRepo.GetByStudentId(username);
                if (student != null && VerifyPassword(password, student.Password))
                {
                    HttpContext.Session.SetString("UserId", student.Id.ToString());
                    HttpContext.Session.SetString("UserRole", "Student");
                    HttpContext.Session.SetString("Username", student.FullName);
                    return RedirectToAction("Dashboard", "StudentPortal");
                }
            }
            else
            {
                var user = _userRepo.GetByUsername(username);
                if (user != null && VerifyPassword(password, user.Password) && user.Role.ToString() == role)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserRole", user.Role.ToString());
                    HttpContext.Session.SetString("Username", user.FullName);

                    return user.Role switch
                    {
                        UserRole.Director => RedirectToAction("Dashboard", "Director"),
                        UserRole.Proctor  => RedirectToAction("Dashboard", "Proctor"),
                        _                 => RedirectToAction("Index", "Home")
                    };
                }
            }

            TempData["Error"] = "Invalid username or password!";
            return View();
        }

        // GET: Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully!";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Verifies a plain-text password against a stored value.
        /// Supports both BCrypt hashes and legacy plain-text (for migration).
        /// </summary>
        public static bool VerifyPassword(string plainText, string stored)
        {
            // BCrypt hashes always start with $2
            if (stored.StartsWith("$2"))
                return BC.Verify(plainText, stored);

            // Legacy plain-text comparison (will be replaced on next login)
            return plainText == stored;
        }

        /// <summary>Hashes a plain-text password using BCrypt.</summary>
        public static string HashPassword(string plainText) => BC.HashPassword(plainText, workFactor: 11);
    }
}
