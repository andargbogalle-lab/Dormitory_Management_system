using dprmitory.Models;
using Microsoft.Data.SqlClient;

namespace dprmitory.Data
{
    public class StudentRepository
    {
        private readonly DatabaseHelper _db;

        public StudentRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public List<Student> GetAll()
        {
            var students = new List<Student>();
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                SELECT s.*, r.RoomNumber, r.Capacity, r.CurrentOccupancy, r.Type, r.Status AS RoomStatus
                FROM Students s
                LEFT JOIN Rooms r ON s.RoomId = r.Id
                ORDER BY s.RegistrationDate DESC";
            using var reader = _db.ExecuteReader(connection, query);

            while (reader.Read())
            {
                students.Add(MapStudentWithRoom(reader));
            }

            return students;
        }

        public Student? GetById(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                SELECT s.*, r.RoomNumber, r.Capacity, r.CurrentOccupancy, r.Type, r.Status AS RoomStatus
                FROM Students s
                LEFT JOIN Rooms r ON s.RoomId = r.Id
                WHERE s.Id = @id";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@id", id));

            if (reader.Read())
            {
                return MapStudentWithRoom(reader);
            }

            return null;
        }

        public Student? GetByStudentId(string studentId)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Students WHERE StudentId = @studentId";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@studentId", studentId));

            if (reader.Read())
            {
                return MapStudent(reader);
            }

            return null;
        }

        public void Add(Student student)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                INSERT INTO Students (StudentId, Password, FullName, Email, Phone, Department, 
                                     RoomId, RegistrationDate, CheckInDate, CheckOutDate, IsCheckedIn, Status, MarkedPresentAt)
                VALUES (@studentId, @password, @fullName, @email, @phone, @department, 
                        @roomId, @regDate, @checkInDate, @checkOutDate, @isCheckedIn, @status, @markedPresentAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new[]
            {
                new SqlParameter("@studentId", student.StudentId),
                new SqlParameter("@password", student.Password),
                new SqlParameter("@fullName", student.FullName),
                new SqlParameter("@email", student.Email),
                new SqlParameter("@phone", student.Phone),
                new SqlParameter("@department", student.Department),
                new SqlParameter("@roomId", (object?)student.RoomId ?? DBNull.Value),
                new SqlParameter("@regDate", student.RegistrationDate),
                new SqlParameter("@checkInDate", (object?)student.CheckInDate ?? DBNull.Value),
                new SqlParameter("@checkOutDate", (object?)student.CheckOutDate ?? DBNull.Value),
                new SqlParameter("@isCheckedIn", student.IsCheckedIn),
                new SqlParameter("@status", (int)student.Status),
                new SqlParameter("@markedPresentAt", (object?)student.MarkedPresentAt ?? DBNull.Value)
            };

            var id = _db.ExecuteScalar(connection, query, parameters);
            student.Id = Convert.ToInt32(id);
        }

        public void Update(Student student)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                UPDATE Students 
                SET StudentId = @studentId, Password = @password, FullName = @fullName, 
                    Email = @email, Phone = @phone, Department = @department, RoomId = @roomId,
                    CheckInDate = @checkInDate, CheckOutDate = @checkOutDate, 
                    IsCheckedIn = @isCheckedIn, Status = @status, MarkedPresentAt = @markedPresentAt
                WHERE Id = @id";

            var parameters = new[]
            {
                new SqlParameter("@id", student.Id),
                new SqlParameter("@studentId", student.StudentId),
                new SqlParameter("@password", student.Password),
                new SqlParameter("@fullName", student.FullName),
                new SqlParameter("@email", student.Email),
                new SqlParameter("@phone", student.Phone),
                new SqlParameter("@department", student.Department),
                new SqlParameter("@roomId", (object?)student.RoomId ?? DBNull.Value),
                new SqlParameter("@checkInDate", (object?)student.CheckInDate ?? DBNull.Value),
                new SqlParameter("@checkOutDate", (object?)student.CheckOutDate ?? DBNull.Value),
                new SqlParameter("@isCheckedIn", student.IsCheckedIn),
                new SqlParameter("@status", (int)student.Status),
                new SqlParameter("@markedPresentAt", (object?)student.MarkedPresentAt ?? DBNull.Value)
            };

            _db.ExecuteNonQuery(connection, query, parameters);
        }

        public void Delete(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "DELETE FROM Students WHERE Id = @id";
            _db.ExecuteNonQuery(connection, query, new SqlParameter("@id", id));
        }

        public int Count()
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT COUNT(*) FROM Students";
            var result = _db.ExecuteScalar(connection, query);
            return Convert.ToInt32(result);
        }

        public int CountByStatus(AvailabilityStatus status)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT COUNT(*) FROM Students WHERE Status = @status";
            var result = _db.ExecuteScalar(connection, query, new SqlParameter("@status", (int)status));
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// Resets students whose Present status has expired (marked Present more than 24 hours ago).
        /// Called every minute by the background service.
        /// </summary>
        public void ResetExpiredPresentStatus()
        {
            using var connection = _db.GetConnection();
            connection.Open();

            // Reset only students who are Present AND were marked Present more than 24 hours ago
            var query = @"
                UPDATE Students 
                SET Status = 1, MarkedPresentAt = NULL
                WHERE Status = 0 
                  AND MarkedPresentAt IS NOT NULL 
                  AND MarkedPresentAt <= DATEADD(HOUR, -24, GETDATE())";

            _db.ExecuteNonQuery(connection, query);
        }

        /// <summary>
        /// Resets ALL students' availability status to Absent regardless of time.
        /// Used as a fallback full reset.
        /// </summary>
        public void ResetAllStatusToAbsent()
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "UPDATE Students SET Status = 1, MarkedPresentAt = NULL";
            _db.ExecuteNonQuery(connection, query);
        }

        private Student MapStudent(SqlDataReader reader)
        {
            return new Student
            {
                Id = reader.GetInt32(0),
                StudentId = reader.GetString(1),
                Password = reader.GetString(2),
                FullName = reader.GetString(3),
                Email = reader.GetString(4),
                Phone = reader.GetString(5),
                Department = reader.GetString(6),
                RoomId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                RegistrationDate = reader.GetDateTime(8),
                CheckInDate = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                CheckOutDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                IsCheckedIn = reader.GetBoolean(11),
                Status = (AvailabilityStatus)reader.GetInt32(12),
                MarkedPresentAt = reader.IsDBNull(13) ? null : reader.GetDateTime(13)
            };
        }

        // Maps student columns (0-13) + joined Room columns (14-18)
        private Student MapStudentWithRoom(SqlDataReader reader)
        {
            var student = MapStudent(reader);

            // LEFT JOIN — room columns are null if student has no room
            if (!reader.IsDBNull(14))
            {
                student.Room = new Room
                {
                    Id = student.RoomId!.Value,
                    RoomNumber = reader.GetString(14),
                    Capacity = reader.GetInt32(15),
                    CurrentOccupancy = reader.GetInt32(16),
                    Type = (RoomType)reader.GetInt32(17),
                    Status = (RoomStatus)reader.GetInt32(18)
                };
            }

            return student;
        }
    }
}
