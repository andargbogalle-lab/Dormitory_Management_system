using dprmitory.Models;
using Microsoft.Data.SqlClient;

namespace dprmitory.Data
{
    public class RoomRepository
    {
        private readonly DatabaseHelper _db;

        public RoomRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public List<Room> GetAll()
        {
            var rooms = new List<Room>();
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Rooms ORDER BY RoomNumber";
            using var reader = _db.ExecuteReader(connection, query);

            while (reader.Read())
            {
                rooms.Add(MapRoom(reader));
            }

            // Close reader before running per-room student queries
            reader.Close();

            foreach (var room in rooms)
            {
                room.Students = GetStudentsForRoom(room.Id);
            }

            return rooms;
        }

        public Room? GetById(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Rooms WHERE Id = @id";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@id", id));

            Room? room = null;
            if (reader.Read())
            {
                room = MapRoom(reader);
            }

            reader.Close();

            if (room != null)
            {
                room.Students = GetStudentsForRoom(room.Id);
            }

            return room;
        }

        private List<Student> GetStudentsForRoom(int roomId)
        {
            var students = new List<Student>();
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT Id, StudentId, FullName FROM Students WHERE RoomId = @roomId";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@roomId", roomId));

            while (reader.Read())
            {
                students.Add(new Student
                {
                    Id = reader.GetInt32(0),
                    StudentId = reader.GetString(1),
                    FullName = reader.GetString(2)
                });
            }

            return students;
        }

        public Room? GetByRoomNumber(string roomNumber)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Rooms WHERE RoomNumber = @roomNumber";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@roomNumber", roomNumber));

            if (reader.Read())
            {
                return MapRoom(reader);
            }

            return null;
        }

        public void Add(Room room)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                INSERT INTO Rooms (RoomNumber, Capacity, CurrentOccupancy, Type, Status)
                VALUES (@roomNumber, @capacity, @currentOccupancy, @type, @status);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new[]
            {
                new SqlParameter("@roomNumber", room.RoomNumber),
                new SqlParameter("@capacity", room.Capacity),
                new SqlParameter("@currentOccupancy", room.CurrentOccupancy),
                new SqlParameter("@type", (int)room.Type),
                new SqlParameter("@status", (int)room.Status)
            };

            var id = _db.ExecuteScalar(connection, query, parameters);
            room.Id = Convert.ToInt32(id);
        }

        public void Update(Room room)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                UPDATE Rooms 
                SET RoomNumber = @roomNumber, Capacity = @capacity, 
                    CurrentOccupancy = @currentOccupancy, Type = @type, Status = @status
                WHERE Id = @id";

            var parameters = new[]
            {
                new SqlParameter("@id", room.Id),
                new SqlParameter("@roomNumber", room.RoomNumber),
                new SqlParameter("@capacity", room.Capacity),
                new SqlParameter("@currentOccupancy", room.CurrentOccupancy),
                new SqlParameter("@type", (int)room.Type),
                new SqlParameter("@status", (int)room.Status)
            };

            _db.ExecuteNonQuery(connection, query, parameters);
        }

        public void Delete(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "DELETE FROM Rooms WHERE Id = @id";
            _db.ExecuteNonQuery(connection, query, new SqlParameter("@id", id));
        }

        public int Count()
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT COUNT(*) FROM Rooms";
            var result = _db.ExecuteScalar(connection, query);
            return Convert.ToInt32(result);
        }

        public int CountByStatus(RoomStatus status)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT COUNT(*) FROM Rooms WHERE Status = @status";
            var result = _db.ExecuteScalar(connection, query, new SqlParameter("@status", (int)status));
            return Convert.ToInt32(result);
        }

        public int GetTotalCapacity()
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT SUM(Capacity) FROM Rooms";
            var result = _db.ExecuteScalar(connection, query);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public int GetCurrentOccupancy()
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT SUM(CurrentOccupancy) FROM Rooms";
            var result = _db.ExecuteScalar(connection, query);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private Room MapRoom(SqlDataReader reader)
        {
            return new Room
            {
                Id = reader.GetInt32(0),
                RoomNumber = reader.GetString(1),
                Capacity = reader.GetInt32(2),
                CurrentOccupancy = reader.GetInt32(3),
                Type = (RoomType)reader.GetInt32(4),
                Status = (RoomStatus)reader.GetInt32(5)
            };
        }
    }
}
