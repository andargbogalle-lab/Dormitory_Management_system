# Dormitory Management System

An event-driven ASP.NET Core MVC application for managing student housing in a dormitory.

## Project Information

**Course:** Event-Driven Programming  
**Institution:** Wollo University - KIOT - College of Computing and Informatics  
**Department:** Information Technology

### Group Members
1. ANDARGACHEW BOGALE - 0248/16
2. MAREY GASHAW - 1226/16
3. WONDATIR FETENE - 2003/16
4. MEAZA WONDYE - 1252/16
5. HUSEN ALI - 1071/16
6. HAYMANOT FENTAW - 1021/16

## Features

### 1. Authentication System
- Role-based login (Director, Proctor, Student)
- Session management with 30-minute timeout
- Secure password authentication
- Automatic redirection based on user role

### 2. Student Registration
- Register new students with personal details
- Real-time form validation (TextChanged events)
- Automatic ID generation
- Password creation for student login

### 3. Room Allocation
- Assign rooms to students
- Real-time room availability checking
- Automatic capacity management
- Warning messages for full rooms

### 4. Check-In / Check-Out
- Student check-in recording
- Check-out with room deallocation
- Timestamp tracking

### 5. Student Availability Tracking
- Mark students as Present/Absent
- Real-time status updates
- Status filtering and reporting

### 6. Search & View Records
- Live search with TextChanged events
- Filter students by name or ID
- Detailed student information view

### 7. Room Deallocation
- Remove students from rooms
- Update room availability status
- Automatic status management

## Event Handling

The system uses various event types:

- **Button Click Events:** Register, Assign Room, Check-In, Check-Out, Mark Present/Absent
- **Text Change Events:** Search students in real-time
- **Selection Events:** Room selection with availability display
- **Form Load Events:** Load student and room data on page load
- **Data Update Events:** Refresh lists after status changes

## Actors

### 1. Student
- View assigned room
- Check personal status
- View payment information

### 2. Proctor
- Register students
- Assign/deallocate rooms
- Mark attendance status
- Monitor room availability

### 3. Director of Proctor
- View system reports
- Monitor overall operations
- Review room allocation
- Check student statistics

## Technology Stack

- **Framework:** ASP.NET Core MVC (.NET 10.0)
- **Language:** C#
- **Frontend:** HTML, CSS, JavaScript, Bootstrap 5
- **Data Storage:** In-memory (DormitoryContext)

## Running the Application

1. Ensure you have .NET 10.0 SDK installed
2. Navigate to the project directory
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser and navigate to the displayed URL (typically https://localhost:5001)
5. Login with default credentials (see CREDENTIALS.md)

## Default Login Credentials

### Director
- Username: `director`
- Password: `1234`

### Proctor
- Username: `proctor`
- Password: `5678`

For complete credential information, see [CREDENTIALS.md](CREDENTIALS.md)

## Project Structure

```
dprmitory/
├── Controllers/
│   ├── AuthController.cs
│   ├── HomeController.cs
│   ├── StudentController.cs
│   ├── StudentPortalController.cs
│   ├── RoomController.cs
│   ├── ProctorController.cs
│   └── DirectorController.cs
├── Models/
│   ├── User.cs
│   ├── Student.cs
│   ├── Room.cs
│   ├── DormitoryContext.cs
│   └── ErrorViewModel.cs
├── Views/
│   ├── Home/
│   ├── Student/
│   ├── Room/
│   ├── Proctor/
│   ├── Director/
│   └── Shared/
└── wwwroot/
    ├── css/
    ├── js/
    └── lib/
```

## Key Components

### Models
- **User:** System user accounts (Director, Proctor)
- **Student:** Student information and status with login credentials
- **Room:** Room details and occupancy
- **DormitoryContext:** In-memory data storage with default users

### Controllers
- **AuthController:** Login, logout, and authentication
- **StudentController:** Student registration and management (Proctor/Director)
- **StudentPortalController:** Student dashboard and personal view
- **RoomController:** Room allocation and deallocation
- **ProctorController:** Check-in/out and status management
- **DirectorController:** Reports and overview
- **HomeController:** Landing page and routing

### Views
- Responsive design with Bootstrap 5
- Real-time updates using JavaScript
- Event-driven user interactions

## Sample Data

The system initializes with:

### Default Users
- 1 Director account
- 1 Proctor account

### Sample Rooms
- Room 101: Double (Capacity: 2)
- Room 102: Double (Capacity: 2)
- Room 103: Triple (Capacity: 3)
- Room 104: Quad (Capacity: 4)
- Room 201: Single (Capacity: 1)

## Security Features

- Session-based authentication
- Role-based access control
- Automatic session timeout (30 minutes)
- Protected routes requiring authentication
- Password-protected student accounts

## Future Enhancements

- Database integration (SQL Server/PostgreSQL)
- Email notifications
- Report generation (PDF/Excel)
- Password encryption
- Password reset functionality
- Mobile responsive improvements
- Profile picture uploads
- Advanced search filters
