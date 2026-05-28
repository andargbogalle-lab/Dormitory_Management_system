using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;

namespace dprmitory.Controllers
{
    public class RoomController : Controller
    {
        private readonly StudentRepository _studentRepo;
        private readonly RoomRepository _roomRepo;

        public RoomController(StudentRepository studentRepo, RoomRepository roomRepo)
        {
            _studentRepo = studentRepo;
            _roomRepo = roomRepo;
        }

        // GET: Room/Index
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
            {
                return RedirectToAction("Login", "Auth");
            }
            return View(_roomRepo.GetAll());
        }

        // GET: Room/Allocate
        public IActionResult Allocate()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor" && userRole != "Director")
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Students = _studentRepo.GetAll().Where(s => s.RoomId == null).ToList();
            ViewBag.Rooms = _roomRepo.GetAll().Where(r => r.Status != RoomStatus.Full).ToList();
            return View();
        }

        // POST: Room/Allocate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Allocate(int studentId, int roomId)
        {
            var student = _studentRepo.GetById(studentId);
            var room = _roomRepo.GetById(roomId);

            if (student == null || room == null)
            {
                TempData["Error"] = "Invalid student or room selection!";
                return RedirectToAction("Allocate");
            }

            if (room.CurrentOccupancy >= room.Capacity)
            {
                TempData["Error"] = $"Room {room.RoomNumber} is full!";
                return RedirectToAction("Allocate");
            }

            student.RoomId = roomId;
            room.CurrentOccupancy++;

            if (room.CurrentOccupancy >= room.Capacity)
            {
                room.Status = RoomStatus.Full;
            }
            else
            {
                room.Status = RoomStatus.Occupied;
            }

            _studentRepo.Update(student);
            _roomRepo.Update(room);

            TempData["Success"] = $"Room {room.RoomNumber} allocated to {student.FullName}!";
            return RedirectToAction("Index");
        }

        // POST: Room/Deallocate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deallocate(int studentId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor" && userRole != "Director")
                return RedirectToAction("Login", "Auth");
            var student = _studentRepo.GetById(studentId);
            
            if (student == null || student.RoomId == null)
            {
                TempData["Error"] = "Student not found or not assigned to any room!";
                return RedirectToAction("Index");
            }

            var room = _roomRepo.GetById(student.RoomId.Value);
            if (room != null)
            {
                room.CurrentOccupancy--;
                room.Status = room.CurrentOccupancy == 0 ? RoomStatus.Available : RoomStatus.Occupied;
                _roomRepo.Update(room);
            }

            student.RoomId = null;
            student.CheckOutDate = DateTime.Now;
            student.IsCheckedIn = false;
            _studentRepo.Update(student);

            TempData["Success"] = "Room deallocated successfully!";
            return RedirectToAction("Index");
        }
    }
}
