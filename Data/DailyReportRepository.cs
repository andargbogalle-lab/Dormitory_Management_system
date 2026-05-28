using dprmitory.Models;
using Microsoft.Data.SqlClient;

namespace dprmitory.Data
{
    public class DailyReportRepository
    {
        private readonly DatabaseHelper _db;

        public DailyReportRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public List<DailyReport> GetAll()
        {
            var reports = new List<DailyReport>();
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM DailyReports ORDER BY ReportDate DESC, CreatedAt DESC";
            using var reader = _db.ExecuteReader(connection, query);

            while (reader.Read())
            {
                reports.Add(MapDailyReport(reader));
            }

            return reports;
        }

        public List<DailyReport> GetByUserId(int userId)
        {
            var reports = new List<DailyReport>();
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM DailyReports WHERE PreparedByUserId = @userId ORDER BY ReportDate DESC";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@userId", userId));

            while (reader.Read())
            {
                reports.Add(MapDailyReport(reader));
            }

            return reports;
        }

        public DailyReport? GetById(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM DailyReports WHERE Id = @id";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@id", id));

            if (reader.Read())
            {
                return MapDailyReport(reader);
            }

            return null;
        }

        public void Add(DailyReport report)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                INSERT INTO DailyReports (ReportDate, PreparedBy, PreparedByUserId, TotalStudents, 
                                         StudentsPresent, StudentsAbsent, TotalRooms, OccupiedRooms, 
                                         AvailableRooms, TotalCapacity, CurrentOccupancy, CheckInsToday, 
                                         CheckOutsToday, Remarks, Issues, CreatedAt, Status)
                VALUES (@reportDate, @preparedBy, @preparedByUserId, @totalStudents, @studentsPresent, 
                        @studentsAbsent, @totalRooms, @occupiedRooms, @availableRooms, @totalCapacity, 
                        @currentOccupancy, @checkInsToday, @checkOutsToday, @remarks, @issues, 
                        @createdAt, @status);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new[]
            {
                new SqlParameter("@reportDate", report.ReportDate),
                new SqlParameter("@preparedBy", report.PreparedBy),
                new SqlParameter("@preparedByUserId", report.PreparedByUserId),
                new SqlParameter("@totalStudents", report.TotalStudents),
                new SqlParameter("@studentsPresent", report.StudentsPresent),
                new SqlParameter("@studentsAbsent", report.StudentsAbsent),
                new SqlParameter("@totalRooms", report.TotalRooms),
                new SqlParameter("@occupiedRooms", report.OccupiedRooms),
                new SqlParameter("@availableRooms", report.AvailableRooms),
                new SqlParameter("@totalCapacity", report.TotalCapacity),
                new SqlParameter("@currentOccupancy", report.CurrentOccupancy),
                new SqlParameter("@checkInsToday", report.CheckInsToday),
                new SqlParameter("@checkOutsToday", report.CheckOutsToday),
                new SqlParameter("@remarks", (object?)report.Remarks ?? DBNull.Value),
                new SqlParameter("@issues", (object?)report.Issues ?? DBNull.Value),
                new SqlParameter("@createdAt", report.CreatedAt),
                new SqlParameter("@status", (int)report.Status)
            };

            var id = _db.ExecuteScalar(connection, query, parameters);
            report.Id = Convert.ToInt32(id);
        }

        public void Update(DailyReport report)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                UPDATE DailyReports 
                SET ReportDate = @reportDate, TotalStudents = @totalStudents, 
                    StudentsPresent = @studentsPresent, StudentsAbsent = @studentsAbsent, 
                    TotalRooms = @totalRooms, OccupiedRooms = @occupiedRooms, 
                    AvailableRooms = @availableRooms, TotalCapacity = @totalCapacity, 
                    CurrentOccupancy = @currentOccupancy, CheckInsToday = @checkInsToday, 
                    CheckOutsToday = @checkOutsToday, Remarks = @remarks, Issues = @issues, 
                    Status = @status
                WHERE Id = @id";

            var parameters = new[]
            {
                new SqlParameter("@id", report.Id),
                new SqlParameter("@reportDate", report.ReportDate),
                new SqlParameter("@totalStudents", report.TotalStudents),
                new SqlParameter("@studentsPresent", report.StudentsPresent),
                new SqlParameter("@studentsAbsent", report.StudentsAbsent),
                new SqlParameter("@totalRooms", report.TotalRooms),
                new SqlParameter("@occupiedRooms", report.OccupiedRooms),
                new SqlParameter("@availableRooms", report.AvailableRooms),
                new SqlParameter("@totalCapacity", report.TotalCapacity),
                new SqlParameter("@currentOccupancy", report.CurrentOccupancy),
                new SqlParameter("@checkInsToday", report.CheckInsToday),
                new SqlParameter("@checkOutsToday", report.CheckOutsToday),
                new SqlParameter("@remarks", (object?)report.Remarks ?? DBNull.Value),
                new SqlParameter("@issues", (object?)report.Issues ?? DBNull.Value),
                new SqlParameter("@status", (int)report.Status)
            };

            _db.ExecuteNonQuery(connection, query, parameters);
        }

        public void Delete(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "DELETE FROM DailyReports WHERE Id = @id";
            _db.ExecuteNonQuery(connection, query, new SqlParameter("@id", id));
        }

        private DailyReport MapDailyReport(SqlDataReader reader)
        {
            return new DailyReport
            {
                Id = reader.GetInt32(0),
                ReportDate = reader.GetDateTime(1),
                PreparedBy = reader.GetString(2),
                PreparedByUserId = reader.GetInt32(3),
                TotalStudents = reader.GetInt32(4),
                StudentsPresent = reader.GetInt32(5),
                StudentsAbsent = reader.GetInt32(6),
                TotalRooms = reader.GetInt32(7),
                OccupiedRooms = reader.GetInt32(8),
                AvailableRooms = reader.GetInt32(9),
                TotalCapacity = reader.GetInt32(10),
                CurrentOccupancy = reader.GetInt32(11),
                CheckInsToday = reader.GetInt32(12),
                CheckOutsToday = reader.GetInt32(13),
                Remarks = reader.IsDBNull(14) ? null : reader.GetString(14),
                Issues = reader.IsDBNull(15) ? null : reader.GetString(15),
                CreatedAt = reader.GetDateTime(16),
                Status = (ReportStatus)reader.GetInt32(17)
            };
        }
    }
}
