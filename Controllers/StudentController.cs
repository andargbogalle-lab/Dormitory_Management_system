using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;

namespace dprmitory.Controllers
{
    public class StudentController : Controller
    {
        private readonly StudentRepository _studentRepo;

        public StudentController(StudentRepository studentRepo)
        {
            _studentRepo = studentRepo;
        }

        // GET: Student/Register
        public IActionResult Register()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor" && userRole != "Director")
                return RedirectToAction("Login", "Auth");
            return View();
        }

        // POST: Student/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Student student)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor" && userRole != "Director")
                return RedirectToAction("Login", "Auth");

            // Auto-generate a random 6-character alphanumeric password
            var plainPassword = GeneratePassword();
            student.Password = AuthController.HashPassword(plainPassword);
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
                return View(student);

            if (_studentRepo.GetByStudentId(student.StudentId) != null)
            {
                ModelState.AddModelError("StudentId", "Student ID already exists!");
                return View(student);
            }

            if (!string.IsNullOrEmpty(student.Email))
            {
                var existing = _studentRepo.GetAll().FirstOrDefault(s => s.Email == student.Email);
                if (existing != null)
                {
                    ModelState.AddModelError("Email", "Email already exists!");
                    return View(student);
                }
            }

            student.RegistrationDate = DateTime.Now;
            student.IsCheckedIn = false;
            student.Status = AvailabilityStatus.Absent;
            student.RoomId = null;
            student.CheckInDate = null;
            student.CheckOutDate = null;

            try
            {
                _studentRepo.Add(student);
                TempData["Success"] = $"Student registered! Login password: {plainPassword} — share this with the student.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error registering student: {ex.Message}");
                return View(student);
            }
        }

        // GET: Student/Index
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
                return RedirectToAction("Login", "Auth");
            return View(_studentRepo.GetAll());
        }

        // GET: Student/Details/5
        public IActionResult Details(int id)
        {
            var student = _studentRepo.GetById(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // GET: Student/Search
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            var results = string.IsNullOrWhiteSpace(searchTerm)
                ? _studentRepo.GetAll()
                : _studentRepo.GetAll()
                    .Where(s => s.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                s.StudentId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            return PartialView("_StudentList", results);
        }

        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
        }
    }
}
