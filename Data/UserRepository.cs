using dprmitory.Models;
using Microsoft.Data.SqlClient;

namespace dprmitory.Data
{
    public class UserRepository
    {
        private readonly DatabaseHelper _db;

        public UserRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public List<User> GetAll()
        {
            var users = new List<User>();
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Users ORDER BY CreatedDate DESC";
            using var reader = _db.ExecuteReader(connection, query);

            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public User? GetById(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Users WHERE Id = @id";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@id", id));

            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null;
        }

        public User? GetByUsername(string username)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Users WHERE Username = @username";
            using var reader = _db.ExecuteReader(connection, query, new SqlParameter("@username", username));

            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null;
        }

        public User? ValidateUser(string username, string password)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "SELECT * FROM Users WHERE Username = @username AND Password = @password";
            using var reader = _db.ExecuteReader(connection, query, 
                new SqlParameter("@username", username),
                new SqlParameter("@password", password));

            if (reader.Read())
            {
                return MapUser(reader);
            }

            return null;
        }

        public void Add(User user)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                INSERT INTO Users (Username, Password, FullName, Role, CreatedDate)
                VALUES (@username, @password, @fullName, @role, @createdDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new[]
            {
                new SqlParameter("@username", user.Username),
                new SqlParameter("@password", user.Password),
                new SqlParameter("@fullName", user.FullName),
                new SqlParameter("@role", (int)user.Role),
                new SqlParameter("@createdDate", user.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"))
            };

            var id = _db.ExecuteScalar(connection, query, parameters);
            user.Id = Convert.ToInt32(id);
        }

        public void Update(User user)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = @"
                UPDATE Users 
                SET Username = @username, Password = @password, FullName = @fullName, Role = @role
                WHERE Id = @id";

            var parameters = new[]
            {
                new SqlParameter("@id", user.Id),
                new SqlParameter("@username", user.Username),
                new SqlParameter("@password", user.Password),
                new SqlParameter("@fullName", user.FullName),
                new SqlParameter("@role", (int)user.Role)
            };

            _db.ExecuteNonQuery(connection, query, parameters);
        }

        public void Delete(int id)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            var query = "DELETE FROM Users WHERE Id = @id";
            _db.ExecuteNonQuery(connection, query, new SqlParameter("@id", id));
        }

        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                FullName = reader.GetString(3),
                Role = (UserRole)reader.GetInt32(4),
                CreatedDate = reader.GetDateTime(5)
            };
        }
    }
}
