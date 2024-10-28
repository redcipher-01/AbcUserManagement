
# AbcUserManagement

## Overview

AbcUserManagement is a web API built with .NET 7 that provides JWT authentication and user management functionalities. The API allows users to log in, and list, create, update, and delete company users. The application ensures security and insulation of company data and includes a throttling middleware to limit the number of requests per user.

## Features

- JWT Authentication
- User Management (CRUD operations)
- Role-based access control (Admin and User roles)
- Throttling middleware to limit requests
- Logging for monitoring and debugging

## Technologies Used

- .NET 7
- Dapper for data access
- MS SQL for database
- NUnit for testing

## Project Structure

```
AbcUserManagement/
├── Controllers/
│   └── UsersController.cs
├── Middleware/
│   └── ThrottlingMiddleware.cs
├── Models/
│   └── Role.cs
├── Services/
│   ├── IUserService.cs
│   └── UserService.cs
├── Data/
│   ├── IUserRepository.cs
│   └── UserRepository.cs
├── Tests/
│   ├── ThrottlingMiddlewareTests.cs
│   └── UserServiceTests.cs
├── Program.cs
├── appsettings.json
└── README.md
```

## Setup Instructions

### Prerequisites

- .NET 7 SDK
- MS SQL Server
- Visual Studio or Visual Studio Code

### Database Setup

1. **Create the Database Tables**

   Run the following SQL script to create the necessary tables:

   ```sql
   -- Create the Companies table
   CREATE TABLE Companies (
       Id INT PRIMARY KEY IDENTITY,
       Name NVARCHAR(100) NOT NULL
   );

   -- Create the Users table
   CREATE TABLE Users (
       Id INT PRIMARY KEY IDENTITY,
       Username NVARCHAR(50) NOT NULL,
       Password NVARCHAR(50) NOT NULL,
       Role NVARCHAR(20) NOT NULL,
       CompanyId INT NOT NULL,
       FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
   );
   ```

2. **Seed Initial Data**

   Run the following SQL script to seed initial data:

   ```sql
   -- Insert initial data into the Companies table
   INSERT INTO Companies (Name) VALUES ('Company A');
   INSERT INTO Companies (Name) VALUES ('Company B');

   -- Insert initial data into the Users table
   INSERT INTO Users (Username, Password, Role, CompanyId) VALUES ('admin1', 'password1', 'Admin', 1);
   INSERT INTO Users (Username, Password, Role, CompanyId) VALUES ('user1', 'password1', 'User', 1);
   INSERT INTO Users (Username, Password, Role, CompanyId) VALUES ('admin2', 'password2', 'Admin', 2);
   INSERT INTO Users (Username, Password, Role, CompanyId) VALUES ('user2', 'password2', 'User', 2);
   ```

### Application Setup

1. **Clone the Repository**

   ```bash
   git clone https://github.com/your-repo/AbcUserManagement.git
   cd AbcUserManagement
   ```

2. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

3. **Update Configuration**

   Update the `appsettings.json` file with your database connection string and JWT settings:

   ```json
   {
     "Jwt": {
       "Key": "YourSecretKey",
       "Issuer": "YourIssuer",
       "Audience": "YourAudience"
     },
     "ConnectionStrings": {
       "DefaultConnection": "YourConnectionString"
     }
   }
   ```

4. **Run the Application**

   ```bash
   dotnet run
   ```

### Running Tests

To run the tests, use the following command:

```bash
dotnet test
```

## API Endpoints

### Authentication

- **POST /api/auth/login**: Authenticate a user and return a JWT token.

### User Management

- **GET /api/users**: Get a list of users for the authenticated user's company.
- **GET /api/users/{id}**: Get a user by ID.
- **POST /api/users**: Create a new user.
- **PUT /api/users/{id}**: Update an existing user.
- **DELETE /api/users/{id}**: Delete a user.

### Example Requests

#### 1. Authentication (Login)
**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
    "username": "admin1",
    "password": "password1"
}
```

**Response:**
```json
{
    "token": "your_jwt_token_here"
}
```

#### 2. Get All Users
**Request:**
```http
GET /api/users
Authorization: Bearer your_jwt_token_here
```

**Response:**
```json
[
    {
        "id": 1,
        "username": "admin1",
        "role": "Admin",
        "companyId": 1
    },
    {
        "id": 2,
        "username": "user1",
        "role": "User",
        "companyId": 1
    }
]
```

#### 3. Get User by ID
**Request:**
```http
GET /api/users/1
Authorization: Bearer your_jwt_token_here
```

**Response:**
```json
{
    "id": 1,
    "username": "admin1",
    "role": "Admin",
    "companyId": 1
}
```

#### 4. Create a New User
**Request:**
```http
POST /api/users
Content-Type: application/json
Authorization: Bearer your_jwt_token_here

{
    "username": "newuser",
    "password": "newpassword",
    "role": "User",
    "companyId": 1
}
```

**Response:**
```json
{
    "id": 3,
    "username": "newuser",
    "role": "User",
    "companyId": 1
}
```

#### 5. Update an Existing User
**Request:**
```http
PUT /api/users/3
Content-Type: application/json
Authorization: Bearer your_jwt_token_here

{
    "id": 3,
    "username": "updateduser",
    "password": "updatedpassword",
    "role": "User",
    "companyId": 1
}
```

**Response:**
```json
{
    "id": 3,
    "username": "updateduser",
    "role": "User",
    "companyId": 1
}
```

#### 6. Delete a User
**Request:**
```http
DELETE /api/users/3
Authorization: Bearer your_jwt_token_here
```

**Response:**
```json
{
    "message": "User deleted successfully"
}
```

## Middleware

### Throttling Middleware

The throttling middleware limits the number of requests to 10 per minute per user. If the limit is exceeded, a `429 Too Many Requests` status code is returned.

## Logging

The application uses logging to capture important events and errors. Logs are written to the console and can be configured in the `appsettings.json` file.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## License

This project is licensed under the MIT License.
