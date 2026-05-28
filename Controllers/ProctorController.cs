using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;

namespace dprmitory.Controllers
{
    public class ProctorController : Controller
    {
        private readonly StudentRepository _studentRepo;
        private readonly RoomRepository _roomRepo;

        public ProctorController(StudentRepository studentRepo, RoomRepository roomRepo)
        {
            _studentRepo = studentRepo;
            _roomRepo = roomRepo;
        }

        // GET: Proctor/Dashboard
        public IActionResult Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
                return RedirectToAction("Login", "Auth");

            var allStudents = _studentRepo.GetAll();
            ViewBag.TotalStudents = allStudents.Count;
            ViewBag.CheckedInStudents = allStudents.Count(s => s.IsCheckedIn);
            ViewBag.PresentStudents = allStudents.Count(s => s.IsCheckedIn);
            ViewBag.AbsentStudents = allStudents.Count(s => !s.IsCheckedIn);
            ViewBag.AvailableRooms = _roomRepo.CountByStatus(RoomStatus.Available);
            return View();
        }

        // POST: Proctor/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(int studentId)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null)
                return Json(new { success = false, message = "Student not found!" });

            if (student.RoomId == null)
                return Json(new { success = false, message = "Student must be assigned a room first!" });

            student.IsCheckedIn = true;
            student.CheckInDate = DateTime.Now;
            _studentRepo.Update(student);

            return Json(new { success = true, message = $"{student.FullName} checked in successfully!" });
        }

        // POST: Proctor/CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckOut(int studentId)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null)
                return Json(new { success = false, message = "Student not found!" });

            student.IsCheckedIn = false;
            student.CheckOutDate = DateTime.Now;
            _studentRepo.Update(student);

            return Json(new { success = true, message = $"{student.FullName} checked out successfully!" });
        }

        // POST: Proctor/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int studentId, AvailabilityStatus status)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null)
                return Json(new { success = false, message = "Student not found!" });

            student.Status = status;

            // Record the exact time when marked Present so the 24-hour auto-reset can work
            if (status == AvailabilityStatus.Present)
                student.MarkedPresentAt = DateTime.Now;
            else
                student.MarkedPresentAt = null; // Clear when manually marked Absent

            _studentRepo.Update(student);
            var statusText = status == AvailabilityStatus.Present ? "Present" : "Absent";
            return Json(new { success = true, message = $"{student.FullName} marked as {statusText}!" });
        }

        // GET: Proctor/StudentStatus
        public IActionResult StudentStatus()
        {
            return View(_studentRepo.GetAll());
        }

        // GET: Proctor/AbsentStudents
        public IActionResult AbsentStudents()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor" && userRole != "Director")
                return RedirectToAction("Login", "Auth");

            var absentStudents = _studentRepo.GetAll()
                .Where(s => !s.IsCheckedIn)
                .OrderBy(s => s.FullName)
                .ToList();

            ViewBag.TotalAbsent = absentStudents.Count;
            ViewBag.TotalStudents = _studentRepo.Count();
            ViewBag.AbsentWithRoom = absentStudents.Count(s => s.RoomId != null);
            ViewBag.AbsentWithoutRoom = absentStudents.Count(s => s.RoomId == null);
            return View(absentStudents);
        }

        // GET: Proctor/PresentStudents
        public IActionResult PresentStudents()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor" && userRole != "Director")
                return RedirectToAction("Login", "Auth");

            var presentStudents = _studentRepo.GetAll()
                .Where(s => s.IsCheckedIn)
                .OrderBy(s => s.FullName)
                .ToList();

            ViewBag.TotalPresent = presentStudents.Count;
            ViewBag.TotalStudents = _studentRepo.Count();
            return View(presentStudents);
        }
    }
}
