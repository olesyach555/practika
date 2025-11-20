
# Material Management System

This is a C# Windows Forms application for managing materials.

## Prerequisites

- .NET Framework 4.8 or later
- Visual Studio 2019 or later (with .NET desktop development workload)
- SQL Server Express or any other edition of SQL Server

## Setup

1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    ```
2.  **Configure the database connection:**
    - Open the `MaterialManagementSystem/App.config` file.
    - Modify the `connectionString` to point to your SQL Server instance.
    - The default connection string is: `Data Source=U314-S11a\\SQLEXPRESS314S11;Initial Catalog=MaterialManagementDB;Integrated Security=True;Encrypt=False`
3.  **Create the database and tables:**
    - The application will automatically create the necessary tables and seed them with initial data when it first connects to the database.
    - Ensure that the user specified in the connection string has permission to create tables and insert data.

## Running the Application

1.  Open the `MaterialManagementSystem.sln` file in Visual Studio.
2.  Build the solution (Build > Build Solution).
3.  Run the application by pressing F5 or clicking the "Start" button.

## Default Login

-   **Username:** `admin`
-   **Password:** `admin123`
