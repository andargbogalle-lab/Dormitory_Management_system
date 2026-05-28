using dprmitory.Models;
using dprmitory.Data;
using Microsoft.AspNetCore.Mvc;

namespace dprmitory.Controllers
{
    public class DailyReportController : Controller
    {
        private readonly DailyReportRepository _reportRepo;
        private readonly StudentRepository _studentRepo;
        private readonly RoomRepository _roomRepo;

        public DailyReportController(DailyReportRepository reportRepo, StudentRepository studentRepo, RoomRepository roomRepo)
        {
            _reportRepo = reportRepo;
            _studentRepo = studentRepo;
            _roomRepo = roomRepo;
        }
        // GET: DailyReport/Index - View all reports (Director)
        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Director")
            {
                return RedirectToAction("Login", "Auth");
            }

            var reports = _reportRepo.GetAll();
            return View(reports);
        }

        // GET: DailyReport/ProctorReports - Proctor's report management
        public IActionResult ProctorReports()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var reports = _reportRepo.GetByUserId(int.Parse(userId ?? "0"));
            return View(reports);
        }

        // GET: DailyReport/Create - Create new report (Proctor)
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Auto-populate with current statistics
            var allStudents = _studentRepo.GetAll();
            var report = new DailyReport
            {
                ReportDate = DateTime.Now.Date,
                TotalStudents = allStudents.Count,
                StudentsPresent = allStudents.Count(s => s.IsCheckedIn),
                StudentsAbsent = allStudents.Count(s => !s.IsCheckedIn),
                TotalRooms = _roomRepo.Count(),
                OccupiedRooms = _roomRepo.GetAll().Count(r => r.Status == RoomStatus.Occupied || r.Status == RoomStatus.Full),
                AvailableRooms = _roomRepo.CountByStatus(RoomStatus.Available),
                TotalCapacity = _roomRepo.GetTotalCapacity(),
                CurrentOccupancy = _roomRepo.GetCurrentOccupancy(),
                CheckInsToday = allStudents.Count(s => s.CheckInDate?.Date == DateTime.Now.Date),
                CheckOutsToday = allStudents.Count(s => s.CheckOutDate?.Date == DateTime.Now.Date)
            };

            return View(report);
        }

        // POST: DailyReport/Create
        [HttpPost]
        public IActionResult Create(DailyReport report, string action)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Remove validation errors for optional fields
            ModelState.Remove("Remarks");
            ModelState.Remove("Issues");
            ModelState.Remove("PreparedBy");
            ModelState.Remove("PreparedByUserId");

            if (!ModelState.IsValid)
            {
                return View(report);
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("Username");

            report.PreparedByUserId = int.Parse(userId ?? "0");
            report.PreparedBy = userName ?? "Unknown";
            report.CreatedAt = DateTime.Now;
            report.Status = action == "submit" ? ReportStatus.Submitted : ReportStatus.Draft;

            _reportRepo.Add(report);

            TempData["Success"] = action == "submit" 
                ? "✓ Daily report submitted successfully!" 
                : "✓ Daily report saved as draft!";

            return RedirectToAction("ProctorReports");
        }

        // GET: DailyReport/Edit/5
        public IActionResult Edit(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var report = _reportRepo.GetById(id);
            if (report == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (report.PreparedByUserId.ToString() != userId)
            {
                TempData["Error"] = "You can only edit your own reports!";
                return RedirectToAction("ProctorReports");
            }

            return View(report);
        }

        // POST: DailyReport/Edit/5
        [HttpPost]
        public IActionResult Edit(DailyReport updatedReport, string action)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Remove validation errors for optional fields
            ModelState.Remove("Remarks");
            ModelState.Remove("Issues");
            ModelState.Remove("PreparedBy");
            ModelState.Remove("PreparedByUserId");

            var report = _reportRepo.GetById(updatedReport.Id);
            if (report == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (report.PreparedByUserId.ToString() != userId)
            {
                TempData["Error"] = "You can only edit your own reports!";
                return RedirectToAction("ProctorReports");
            }

            // Update report fields
            report.ReportDate = updatedReport.ReportDate;
            report.TotalStudents = updatedReport.TotalStudents;
            report.StudentsPresent = updatedReport.StudentsPresent;
            report.StudentsAbsent = updatedReport.StudentsAbsent;
            report.TotalRooms = updatedReport.TotalRooms;
            report.OccupiedRooms = updatedReport.OccupiedRooms;
            report.AvailableRooms = updatedReport.AvailableRooms;
            report.TotalCapacity = updatedReport.TotalCapacity;
            report.CurrentOccupancy = updatedReport.CurrentOccupancy;
            report.CheckInsToday = updatedReport.CheckInsToday;
            report.CheckOutsToday = updatedReport.CheckOutsToday;
            report.Remarks = updatedReport.Remarks;
            report.Issues = updatedReport.Issues;
            report.Status = action == "submit" ? ReportStatus.Submitted : ReportStatus.Draft;

            _reportRepo.Update(report);

            TempData["Success"] = "✓ Report updated successfully!";
            return RedirectToAction("ProctorReports");
        }

        // GET: DailyReport/Details/5
        public IActionResult Details(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Director" && userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var report = _reportRepo.GetById(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: DailyReport/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Proctor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var report = _reportRepo.GetById(id);
            if (report == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (report.PreparedByUserId.ToString() != userId)
            {
                TempData["Error"] = "You can only delete your own reports!";
                return RedirectToAction("ProctorReports");
            }

            _reportRepo.Delete(id);
            TempData["Success"] = "Report deleted successfully!";
            return RedirectToAction("ProctorReports");
        }

        // POST: DailyReport/MarkReviewed/5 (Director only)
        [HttpPost]
        public IActionResult MarkReviewed(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Director")
            {
                return RedirectToAction("Login", "Auth");
            }

            var report = _reportRepo.GetById(id);
            if (report == null)
            {
                return NotFound();
            }

            report.Status = ReportStatus.Reviewed;
            _reportRepo.Update(report);
            TempData["Success"] = "Report marked as reviewed!";
            return RedirectToAction("Index");
        }
    }
}
