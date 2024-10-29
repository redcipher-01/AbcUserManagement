
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
│
├── Testing
│   ├── AbcUserManagement.IntegrationTests
│   ├── AbcUserManagement.ServiceTests
│   └── AbcUserManagement.UnitTests
│
└── AbcUserManagement
    ├── Connected Services
    ├── Controllers
    │   └── UsersController.cs
    ├── DataAccess
    │   ├── IUserRepository.cs
    │   └── UserRepository.cs
    ├── Middleware
    │   └── ThrottlingMiddleware.cs
    ├── Models
    │   └── Role.cs
    ├── Properties
    ├── Services
    │   ├── IUserService.cs
    │   └── UserService.cs
    ├── appsettings.json
    ├── Program.cs
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
    -- Create the database
    CREATE DATABASE AbcUserManagement;
    
    -- Use the database
    USE AbcUserManagement;
    
    -- Create the Companies table
    CREATE TABLE Companies (
        Id INT PRIMARY KEY IDENTITY,
        Name NVARCHAR(100) NOT NULL
    );
    
    -- Create the Users table
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY,
        Username NVARCHAR(50) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(20) NOT NULL,
        CompanyId INT NOT NULL,
        CreatedBy NVARCHAR(50),
        CreatedDate DATETIME,
        ModifiedBy NVARCHAR(50),
        ModifiedDate DATETIME,
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
    INSERT INTO Users (Username, PasswordHash, Role, CompanyId, CreatedBy, CreatedDate) VALUES ('admin1', '$2a$11$Jprs6nzu6dmalcB.uTaIP.ki2qtKFuSY/.tliSysj9a80Tyi9zZ7u', 'Admin', 1, 'system', GETDATE());
    INSERT INTO Users (Username, PasswordHash, Role, CompanyId, CreatedBy, CreatedDate) VALUES ('user1', '$2a$11$Jprs6nzu6dmalcB.uTaIP.ki2qtKFuSY/.tliSysj9a80Tyi9zZ7u', 'User', 1, 'system', GETDATE());
    INSERT INTO Users (Username, PasswordHash, Role, CompanyId, CreatedBy, CreatedDate) VALUES ('admin2', '$2a$11$Jprs6nzu6dmalcB.uTaIP.ki2qtKFuSY/.tliSysj9a80Tyi9zZ7u', 'Admin', 2, 'system', GETDATE());
    INSERT INTO Users (Username, PasswordHash, Role, CompanyId, CreatedBy, CreatedDate) VALUES ('user2', '$2a$11$Jprs6nzu6dmalcB.uTaIP.ki2qtKFuSY/.tliSysj9a80Tyi9zZ7u', 'User', 2, 'system', GETDATE());
   ```

### Application Setup

1. **Clone the Repository**

   ```bash
   git clone https://github.com/redcipher-01/AbcUserManagement.git
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


### Summary of Changes

- **Database Schema**: Updated the `Users` table to include `PasswordHash`, `CreatedBy`, `CreatedDate`, `ModifiedBy`, and `ModifiedDate` columns.
- **Authentication**: Ensure the [`GenerateJwtToken`](command:_github.copilot.openSymbolFromReferences?%5B%22%22%2C%5B%7B%22uri%22%3A%7B%22scheme%22%3A%22file%22%2C%22authority%22%3A%22%22%2C%22path%22%3A%22%2FUsers%2Freynananislag%2FProjects%2FAbcUserManagement%2FAbcUserManagement%2FControllers%2FAuthController.cs%22%2C%22query%22%3A%22%22%2C%22fragment%22%3A%22%22%7D%2C%22pos%22%3A%7B%22line%22%3A42%2C%22character%22%3A28%7D%7D%5D%2C%22932a8d3e-b41f-4c9d-bd71-0f8a8c882545%22%5D "Go to definition") method includes the [`UserId`](command:_github.copilot.openSymbolFromReferences?%5B%22%22%2C%5B%7B%22uri%22%3A%7B%22scheme%22%3A%22file%22%2C%22authority%22%3A%22%22%2C%22path%22%3A%22%2FUsers%2Freynananislag%2FProjects%2FAbcUserManagement%2FAbcUserManagement%2FControllers%2FAuthController.cs%22%2C%22query%22%3A%22%22%2C%22fragment%22%3A%22%22%7D%2C%22pos%22%3A%7B%22line%22%3A62%2C%22character%22%3A31%7D%7D%5D%2C%22932a8d3e-b41f-4c9d-bd71-0f8a8c882545%22%5D "Go to definition") claim.
- **Throttling Middleware**: Ensure the middleware correctly extracts the [`UserId`](command:_github.copilot.openSymbolFromReferences?%5B%22%22%2C%5B%7B%22uri%22%3A%7B%22scheme%22%3A%22file%22%2C%22authority%22%3A%22%22%2C%22path%22%3A%22%2FUsers%2Freynananislag%2FProjects%2FAbcUserManagement%2FAbcUserManagement%2FControllers%2FAuthController.cs%22%2C%22query%22%3A%22%22%2C%22fragment%22%3A%22%22%7D%2C%22pos%22%3A%7B%22line%22%3A62%2C%22character%22%3A31%7D%7D%5D%2C%22932a8d3e-b41f-4c9d-bd71-0f8a8c882545%22%5D "Go to definition") from the claims.
