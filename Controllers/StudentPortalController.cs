using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;

namespace dprmitory.Controllers
{
    public class StudentPortalController : Controller
    {
        private readonly StudentRepository _studentRepo;

        public StudentPortalController(StudentRepository studentRepo)
        {
            _studentRepo = studentRepo;
        }

        // GET: StudentPortal/Dashboard
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = _studentRepo.GetById(int.Parse(userId));
            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(student);
        }
    }
}
