using Microsoft.Data.SqlClient;
using System.Data;

namespace dprmitory.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private void EnsureDatabaseExists()
        {
            try
            {
                // Create database if it doesn't exist
                var builder = new SqlConnectionStringBuilder(_connectionString);
                var databaseName = builder.InitialCatalog;
                builder.InitialCatalog = "master";
                
                using var masterConnection = new SqlConnection(builder.ConnectionString);
                masterConnection.Open();
                
                // Check if database exists
                var checkDbQuery = $"SELECT database_id FROM sys.databases WHERE name = '{databaseName}'";
                using var checkCmd = new SqlCommand(checkDbQuery, masterConnection);
                var result = checkCmd.ExecuteScalar();
                
                if (result == null)
                {
                    // Create database
                    var createDbQuery = $"CREATE DATABASE [{databaseName}]";
                    using var createCmd = new SqlCommand(createDbQuery, masterConnection);
                    createCmd.ExecuteNonQuery();
                    Console.WriteLine($"Database '{databaseName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Database '{databaseName}' already exists.");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        private void DropAllTables(SqlConnection connection)
        {
            try
            {
                // Drop tables in reverse order of dependencies
                var dropTables = @"
                    -- Drop dependent tables first
                    IF OBJECT_ID('dbo.DailyReports', 'U') IS NOT NULL
                        DROP TABLE dbo.DailyReports;
                    
                    IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL
                        DROP TABLE dbo.Students;
                    
                    IF OBJECT_ID('dbo.Rooms', 'U') IS NOT NULL
                        DROP TABLE dbo.Rooms;
                    
                    IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
                        DROP TABLE dbo.Users;
                ";
                
                ExecuteNonQuery(connection, dropTables);
                Console.WriteLine("All existing tables dropped successfully.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error dropping tables: {ex.Message}");
            }
        }

        public void InitializeDatabase()
        {
            // Ensure database exists
            EnsureDatabaseExists();

            using var connection = GetConnection();
            connection.Open();

            // FIRST: Expand Password columns if they exist and are too small
            ExpandPasswordColumns(connection);

            // Add MarkedPresentAt column to Students if it doesn't exist yet
            AddMarkedPresentAtColumn(connection);

            // THEN: Create tables only if they don't already exist
            var createUsersTable = @"
                IF OBJECT_ID('dbo.Users', 'U') IS NULL
                BEGIN
                    CREATE TABLE Users (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        Username NVARCHAR(50) NOT NULL UNIQUE,
                        Password NVARCHAR(100) NOT NULL,
                        FullName NVARCHAR(100) NOT NULL,
                        Role INT NOT NULL,
                        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT CK_Users_Role CHECK (Role IN (1, 2))
                    );
                    CREATE INDEX IX_Users_Username ON Users(Username);
                    CREATE INDEX IX_Users_Role ON Users(Role);
                    PRINT 'Users table created.';
                END
            ";

            var createRoomsTable = @"
                IF OBJECT_ID('dbo.Rooms', 'U') IS NULL
                BEGIN
                    CREATE TABLE Rooms (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        RoomNumber NVARCHAR(10) NOT NULL UNIQUE,
                        Capacity INT NOT NULL,
                        CurrentOccupancy INT NOT NULL DEFAULT 0,
                        Type INT NOT NULL,
                        Status INT NOT NULL DEFAULT 0,
                        CONSTRAINT CK_Rooms_Capacity CHECK (Capacity > 0),
                        CONSTRAINT CK_Rooms_Occupancy CHECK (CurrentOccupancy >= 0 AND CurrentOccupancy <= Capacity),
                        CONSTRAINT CK_Rooms_Type CHECK (Type IN (1, 2, 3, 4)),
                        CONSTRAINT CK_Rooms_Status CHECK (Status IN (0, 1, 2))
                    );
                    CREATE INDEX IX_Rooms_RoomNumber ON Rooms(RoomNumber);
                    CREATE INDEX IX_Rooms_Status ON Rooms(Status);
                    PRINT 'Rooms table created.';
                END
            ";

            var createStudentsTable = @"
                IF OBJECT_ID('dbo.Students', 'U') IS NULL
                BEGIN
                    CREATE TABLE Students (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        StudentId NVARCHAR(20) NOT NULL UNIQUE,
                        Password NVARCHAR(100) NOT NULL,
                        FullName NVARCHAR(100) NOT NULL,
                        Email NVARCHAR(100) NOT NULL,
                        Phone NVARCHAR(15) NOT NULL,
                        Department NVARCHAR(100) NOT NULL,
                        RoomId INT NULL,
                        RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
                        CheckInDate DATETIME NULL,
                        CheckOutDate DATETIME NULL,
                        IsCheckedIn BIT NOT NULL DEFAULT 0,
                        Status INT NOT NULL DEFAULT 1,
                        MarkedPresentAt DATETIME NULL,
                        CONSTRAINT FK_Students_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE SET NULL
                    );
                    CREATE INDEX IX_Students_StudentId ON Students(StudentId);
                    CREATE INDEX IX_Students_RoomId ON Students(RoomId);
                    CREATE INDEX IX_Students_IsCheckedIn ON Students(IsCheckedIn);
                    CREATE INDEX IX_Students_Department ON Students(Department);
                    PRINT 'Students table created.';
                END
            ";

            var createReportsTable = @"
                IF OBJECT_ID('dbo.DailyReports', 'U') IS NULL
                BEGIN
                    CREATE TABLE DailyReports (
                        Id INT PRIMARY KEY IDENTITY(1,1),
                        ReportDate DATE NOT NULL,
                        PreparedBy NVARCHAR(100) NOT NULL,
                        PreparedByUserId INT NOT NULL,
                        TotalStudents INT NOT NULL DEFAULT 0,
                        StudentsPresent INT NOT NULL DEFAULT 0,
                        StudentsAbsent INT NOT NULL DEFAULT 0,
                        TotalRooms INT NOT NULL DEFAULT 0,
                        OccupiedRooms INT NOT NULL DEFAULT 0,
                        AvailableRooms INT NOT NULL DEFAULT 0,
                        TotalCapacity INT NOT NULL DEFAULT 0,
                        CurrentOccupancy INT NOT NULL DEFAULT 0,
                        CheckInsToday INT NOT NULL DEFAULT 0,
                        CheckOutsToday INT NOT NULL DEFAULT 0,
                        Remarks NVARCHAR(MAX) NULL,
                        Issues NVARCHAR(MAX) NULL,
                        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                        Status INT NOT NULL DEFAULT 0,
                        CONSTRAINT FK_DailyReports_Users FOREIGN KEY (PreparedByUserId) REFERENCES Users(Id) ON DELETE NO ACTION
                    );
                    CREATE INDEX IX_DailyReports_ReportDate ON DailyReports(ReportDate);
                    CREATE INDEX IX_DailyReports_PreparedByUserId ON DailyReports(PreparedByUserId);
                    CREATE INDEX IX_DailyReports_Status ON DailyReports(Status);
                    PRINT 'DailyReports table created.';
                END
            ";

            ExecuteNonQuery(connection, createUsersTable);
            ExecuteNonQuery(connection, createRoomsTable);
            ExecuteNonQuery(connection, createStudentsTable);
            ExecuteNonQuery(connection, createReportsTable);

            Console.WriteLine("Database schema verified.");

            // Seed initial data only if tables are empty
            SeedInitialData(connection);
        }

        /// <summary>
        /// Expands Password columns from NVARCHAR(50) to NVARCHAR(100) if needed.
        /// Must run before password migration to avoid truncation errors.
        /// </summary>
        private void ExpandPasswordColumns(SqlConnection connection)
        {
            try
            {
                // Check and expand Users.Password if table exists
                var checkUsersTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
                var usersTableExists = ExecuteScalar(connection, checkUsersTable);
                if (Convert.ToInt32(usersTableExists) > 0)
                {
                    var checkUsersCol = @"
                        SELECT CHARACTER_MAXIMUM_LENGTH 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Password'";
                    var usersColLength = ExecuteScalar(connection, checkUsersCol);
                    if (usersColLength != null && Convert.ToInt32(usersColLength) < 100)
                    {
                        Console.WriteLine($"Expanding Users.Password from {usersColLength} to 100 characters...");
                        ExecuteNonQuery(connection, "ALTER TABLE Users ALTER COLUMN Password NVARCHAR(100) NOT NULL");
                        Console.WriteLine("Users.Password column expanded successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Users.Password column is already {usersColLength ?? "NULL"} characters.");
                    }
                }

                // Check and expand Students.Password if table exists
                var checkStudentsTable = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Students'";
                var studentsTableExists = ExecuteScalar(connection, checkStudentsTable);
                if (Convert.ToInt32(studentsTableExists) > 0)
                {
                    var checkStudentsCol = @"
                        SELECT CHARACTER_MAXIMUM_LENGTH 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'Password'";
                    var studentsColLength = ExecuteScalar(connection, checkStudentsCol);
                    if (studentsColLength != null && Convert.ToInt32(studentsColLength) < 100)
                    {
                        Console.WriteLine($"Expanding Students.Password from {studentsColLength} to 100 characters...");
                        ExecuteNonQuery(connection, "ALTER TABLE Students ALTER COLUMN Password NVARCHAR(100) NOT NULL");
                        Console.WriteLine("Students.Password column expanded successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Students.Password column is already {studentsColLength ?? "NULL"} characters.");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error expanding password columns: {ex.Message}");
                throw; // Re-throw to prevent migration from running with wrong column size
            }
        }

        private void AddMarkedPresentAtColumn(SqlConnection connection)
        {
            try
            {
                // Only add the column if the Students table exists and the column doesn't yet
                var checkColumn = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Students' AND COLUMN_NAME = 'MarkedPresentAt'";
                var exists = Convert.ToInt32(ExecuteScalar(connection, checkColumn));
                if (exists == 0)
                {
                    ExecuteNonQuery(connection,
                        "ALTER TABLE Students ADD MarkedPresentAt DATETIME NULL");
                    Console.WriteLine("Added MarkedPresentAt column to Students table.");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error adding MarkedPresentAt column: {ex.Message}");
            }
        }

        private void SeedInitialData(SqlConnection connection)
        {
            // Check if users already exist
            var checkUsers = "SELECT COUNT(*) FROM Users";
            var userCount = ExecuteScalar(connection, checkUsers);

            if (Convert.ToInt32(userCount) == 0)
            {
                // Hash default passwords with BCrypt
                var directorHash = BCrypt.Net.BCrypt.HashPassword("1234", workFactor: 11);
                var proctorHash  = BCrypt.Net.BCrypt.HashPassword("5678", workFactor: 11);

                var insertDirector = @"
                    INSERT INTO Users (Username, Password, FullName, Role, CreatedDate)
                    VALUES ('director', @pwd, 'Director Admin', 2, @date)";
                var insertProctor = @"
                    INSERT INTO Users (Username, Password, FullName, Role, CreatedDate)
                    VALUES ('proctor', @pwd, 'Proctor Staff', 1, @date)";

                ExecuteNonQuery(connection, insertDirector,
                    new SqlParameter("@pwd", directorHash),
                    new SqlParameter("@date", DateTime.Now));
                ExecuteNonQuery(connection, insertProctor,
                    new SqlParameter("@pwd", proctorHash),
                    new SqlParameter("@date", DateTime.Now));
            }
            else
            {
                // Migrate any existing plain-text passwords to BCrypt hashes
                MigratePlainTextPasswords(connection);
            }

            // Check if rooms already exist
            var checkRooms = "SELECT COUNT(*) FROM Rooms";
            var roomCount = ExecuteScalar(connection, checkRooms);

            if (Convert.ToInt32(roomCount) == 0)
            {
                var roomNumbers = new[] { "101", "102", "103", "104", "105", "201", "202", "203", "204", "205" };
                foreach (var roomNumber in roomNumbers)
                {
                    var insertRoom = @"
                        INSERT INTO Rooms (RoomNumber, Capacity, CurrentOccupancy, Type, Status)
                        VALUES (@roomNumber, 6, 0, 4, 0)";
                    ExecuteNonQuery(connection, insertRoom,
                        new SqlParameter("@roomNumber", roomNumber));
                }
            }
        }

        private void MigratePlainTextPasswords(SqlConnection connection)
        {
            // Migrate Users table
            var usersQuery = "SELECT Id, Password FROM Users";
            var users = new List<(int Id, string Password)>();
            using (var reader = ExecuteReader(connection, usersQuery))
            {
                while (reader.Read())
                    users.Add((reader.GetInt32(0), reader.GetString(1)));
            }
            foreach (var (id, pwd) in users)
            {
                if (!pwd.StartsWith("$2"))
                {
                    var hash = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 11);
                    // Ensure the hash fits in the column (should be 60 chars, but check)
                    if (hash.Length > 100)
                    {
                        Console.WriteLine($"Warning: BCrypt hash too long ({hash.Length} chars), skipping user {id}");
                        continue;
                    }
                    ExecuteNonQuery(connection,
                        "UPDATE Users SET Password = @hash WHERE Id = @id",
                        new SqlParameter("@hash", hash),
                        new SqlParameter("@id", id));
                }
            }

            // Migrate Students table
            var studentsQuery = "SELECT Id, Password FROM Students";
            var students = new List<(int Id, string Password)>();
            using (var reader = ExecuteReader(connection, studentsQuery))
            {
                while (reader.Read())
                    students.Add((reader.GetInt32(0), reader.GetString(1)));
            }
            foreach (var (id, pwd) in students)
            {
                if (!pwd.StartsWith("$2"))
                {
                    var hash = BCrypt.Net.BCrypt.HashPassword(pwd, workFactor: 11);
                    if (hash.Length > 100)
                    {
                        Console.WriteLine($"Warning: BCrypt hash too long ({hash.Length} chars), skipping student {id}");
                        continue;
                    }
                    ExecuteNonQuery(connection,
                        "UPDATE Students SET Password = @hash WHERE Id = @id",
                        new SqlParameter("@hash", hash),
                        new SqlParameter("@id", id));
                }
            }
        }

        public void ExecuteNonQuery(SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            command.ExecuteNonQuery();
        }

        public object? ExecuteScalar(SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteScalar();
        }

        public SqlDataReader ExecuteReader(SqlConnection connection, string query, params SqlParameter[] parameters)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command.ExecuteReader();
        }
    }
}
