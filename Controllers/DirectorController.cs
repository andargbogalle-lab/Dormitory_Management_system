using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;

namespace dprmitory.Controllers
{
    public class DirectorController : Controller
    {
        private readonly StudentRepository _studentRepo;
        private readonly RoomRepository _roomRepo;

        public DirectorController(StudentRepository studentRepo, RoomRepository roomRepo)
        {
            _studentRepo = studentRepo;
            _roomRepo = roomRepo;
        }

        // GET: Director/Dashboard
        public IActionResult Dashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Director")
                return RedirectToAction("Login", "Auth");

            var allStudents = _studentRepo.GetAll();
            ViewBag.TotalStudents = allStudents.Count;
            ViewBag.TotalRooms = _roomRepo.Count();
            ViewBag.OccupiedRooms = _roomRepo.GetAll().Count(r => r.Status == RoomStatus.Occupied || r.Status == RoomStatus.Full);
            ViewBag.CheckedInStudents = allStudents.Count(s => s.IsCheckedIn);
            ViewBag.PresentStudents = allStudents.Count(s => s.IsCheckedIn);
            ViewBag.AbsentStudents = allStudents.Count(s => !s.IsCheckedIn);
            return View();
        }

        // GET: Director/Reports
        public IActionResult Reports()
        {
            var totalCapacity = _roomRepo.GetTotalCapacity();
            var currentOccupancy = _roomRepo.GetCurrentOccupancy();
            
            var report = new
            {
                Students = _studentRepo.GetAll(),
                Rooms = _roomRepo.GetAll(),
                OccupancyRate = totalCapacity > 0 ? currentOccupancy * 100.0 / totalCapacity : 0
            };
            
            return View(report);
        }
    }
}
