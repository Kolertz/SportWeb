# SportWeb

**SportWeb** is a fitness management web application designed to help users create and track their workouts and exercises.

## Architecture
- **MVC**

## Features

### 🔐 Secure Authentication
- User login and registration with encrypted passwords using cookies and BCrypt.
- Google reCAPTCHA integration for enhanced security.

### 🏋️ Workout and Exercise Management
- Create, edit, and delete custom workout plans.
- Browse a library of categorized exercises with filters.
- Save favorite exercises and make workouts public or private to share.

### 🧑‍💻 Admin Dashboard
- Manage and approve user-submitted exercises.
- Oversee user accounts and platform content.

### 📊 User Profiles
- Customize profiles with avatars and track progress over time.

### 🧠 Other
- Pagination for сomfortable viewing infomation.
- MemoryCache and OutputCache to reduce the load on the database and server.
- Unit Tests with xUnit
## Tech Stack

- **Backend**: ASP.NET Core, Entity Framework Core
- **Frontend**: Razor Pages, Bootstrap, jQuery
- **Database**: SQL Server
- **Unit Testing**: xUnit
- **Logging**: Microsoft.Extensions.Logging
