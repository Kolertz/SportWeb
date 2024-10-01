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

## Screenshots

### List of exercises:
![2](https://github.com/user-attachments/assets/ec8b7ad9-eca5-42ad-a351-03c214c231ac)

### User profile:
![3](https://github.com/user-attachments/assets/7fa39035-2b15-4ad0-bced-e12f9e132542)

### Index page:
![1](https://github.com/user-attachments/assets/12c52446-9cf6-46e7-8bfe-34ada4499c6c)

### Pending exercises on Admin Panel:
![5](https://github.com/user-attachments/assets/6322e527-a81b-44f8-b593-2419a67540ef)

### Workout edit page (you can move exercises with the cursor, including taking them out or putting them in supersets):
![4](https://github.com/user-attachments/assets/051faba8-9cbb-46ff-a3c7-d3978f318941)
