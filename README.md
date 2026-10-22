# Device Management System (ASP.NET Core MVC & Entity Framework Core)

**Course:** CSE 422 - .NET Programming  
**Assignment:** Practice Assignment 2: Building a Device Management Software  
**Target Platform:** .NET 8.0 (`net8.0`)  
**IDE:** Visual Studio 2022 (v17.8+) or Visual Studio Code / .NET CLI  
**Database:** Microsoft SQL Server (LocalDB / Express / Developer)  

---

## 1. Project Overview

The **Device Management System** is an enterprise-grade ASP.NET Core 8.0 Model-View-Controller (MVC) web application designed to track and manage hardware inventory, categorize technical equipment, maintain employee contact directories, and execute high-performance asset searches.

### Key Capabilities
- **Device Category Management:** Full CRUD operations for hardware categories (Laptops, Desktops, Monitors, Network Equipment, etc.) with duplicate name prevention and referential integrity protection (preventing deletion of categories containing active devices).
- **Device Management:** Register, edit, inspect, and delete devices with required business metadata: Name, unique Device Code, Category dropdown, Status, and Date of Entry.
- **Allowed Statuses:** Strictly managed via strongly typed enum: `InUse` ("In Use"), `Broken` ("Broken"), `UnderMaintenance` ("Under Maintenance") with color-coded UI badges.
- **Business Date Validation:** Date of Entry cannot be in the future, enforced via custom validation attribute `[NotInFuture]`.
- **Search & Filtering:** Dynamic server-side search by device name or code, combined with status and category dropdown filters, with active filter badges and an instant reset button.
- **Employee / User Management:** Manage staff contacts (Full Name, unique Email, Phone Number) who operate hardware equipment.
- **Interactive Dashboard:** Summary cards showing total devices, categories, employees, operational status distribution, category breakdown, and recent registrations.

---

## 2. Technology Stack

- **C# 12** on **.NET 8.0** (`net8.0`)
- **ASP.NET Core MVC 8.0**
- **Entity Framework Core 8.0** (`Microsoft.EntityFrameworkCore.SqlServer`)
- **Database:** Microsoft SQL Server / SQL Server LocalDB (`(localdb)\mssqllocaldb`)
- **Frontend:** Bootstrap 5, Bootstrap Icons, jQuery Validation & Unobtrusive Validation
- **Testing:** xUnit 2.9, EF Core In-Memory Provider

---

## 3. Project Structure

```
d:\Lab1_CSE_422\Lab2\
├── DeviceManagement.sln                   # Visual Studio Solution File
├── DeviceManagement.slnx                  # Visual Studio XML Solution File
├── README.md                             # Main setup & execution guide
├── docs/
│   ├── ASSIGNMENT_ANALYSIS.md            # Requirement mapping & traceability
│   ├── LECTURE_CONCEPTS.md               # Lecture slide alignment & DI patterns
│   ├── IMPLEMENTATION_PLAN.md            # Architecture, ERD, and milestones
│   ├── PROJECT_REPORT.md                 # 1-2 page academic submission report
│   ├── REQUIREMENTS_CHECKLIST.md         # Audited verification status checklist
│   └── LEARNING_GUIDE.md                 # Beginner guide (English & Tiếng Việt)
├── DeviceManagement/                     # Main Web Project
│   ├── Controllers/
│   │   ├── HomeController.cs             # Dashboard analytics & overview
│   │   ├── DeviceCategoriesController.cs # Category CRUD & integrity check
│   │   ├── DevicesController.cs          # Device CRUD, search & filters
│   │   └── EmployeesController.cs        # Staff directory CRUD
│   ├── Data/
│   │   ├── DeviceManagementContext.cs    # EF Core DbContext & model configurations
│   │   └── DbInitializer.cs              # Schema migration & sample seed data
│   ├── Models/
│   │   ├── Device.cs                     # Device entity with DataAnnotations
│   │   ├── DeviceCategory.cs             # Category entity
│   │   ├── Employee.cs                   # Employee contact entity
│   │   ├── DeviceStatus.cs               # Strongly typed status enumeration
│   │   └── ValidationAttributes/
│   │       └── NotInFutureAttribute.cs   # Custom business rule validation
│   ├── ViewModels/
│   │   ├── DeviceIndexViewModel.cs       # Combined search & filter view model
│   │   └── DashboardViewModel.cs         # Dashboard analytics view model
│   ├── Views/                            # Strongly typed Razor .cshtml templates
│   ├── wwwroot/                          # Static assets (Bootstrap, CSS, JS)
│   ├── Migrations/                       # EF Core migrations
│   ├── appsettings.json                  # Database connection string
│   └── Program.cs                        # DI setup & HTTP request pipeline
└── DeviceManagement.Tests/               # Automated xUnit Test Suite
    ├── DeviceValidationTests.cs          # Model & NotInFuture validation tests
    ├── DeviceFilterTests.cs              # EF Core LINQ search/filter tests
    └── CategoryAndEmployeeTests.cs       # Business rule & deletion safety tests
```

---

## 4. Prerequisites

1. **.NET 8.0 SDK or .NET 10.0 SDK** (with .NET 8 targeting pack).
2. **Visual Studio 2022** (with *ASP.NET and web development* workload) OR **VS Code** with C# Dev Kit.
3. **Microsoft SQL Server LocalDB** (included with Visual Studio) or a standard SQL Server / SQL Server Express instance.

---

## 5. How to Open and Run in Visual Studio 2022

1. Launch **Visual Studio 2022**.
2. Click **Open a project or solution**.
3. Select `DeviceManagement.sln` located in `d:\Lab1_CSE_422\Lab2\DeviceManagement.sln`.
4. In **Solution Explorer**, ensure `DeviceManagement` is set as the startup project (bold text). If not, right-click `DeviceManagement` and select **Set as Startup Project**.
5. Press **F5** (or click the green **https / http** run button) to build and launch the application in your browser.
6. The application automatically migrates the database schema and seeds realistic initial sample categories, devices, and employees on first launch!

---

## 6. How to Run via .NET CLI (Command Line)

You can also run, build, and test the project directly from PowerShell or Terminal:

### 1. Restore & Build Solution
```powershell
dotnet restore DeviceManagement.sln
dotnet build DeviceManagement.sln
```

### 2. Run Automated Tests
```powershell
dotnet test DeviceManagement.sln
```
*All 28 unit tests verify model validation, date boundary rules, search predicates, and referential deletion constraints.*

### 3. Run the Web Application
```powershell
cd DeviceManagement
dotnet run
```
Then open your browser at the displayed URL (e.g., `https://localhost:7085` or `http://localhost:5218`).

---

## 7. Database Configuration

The connection string is configured in `DeviceManagement/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DeviceManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Alternative SQL Server Connection Strings

- **Local SQL Server / Express (Windows Authentication):**
  ```json
  "DefaultConnection": "Server=localhost;Database=DeviceManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  ```
- **SQL Server with SQL Authentication:**
  ```json
  "DefaultConnection": "Server=localhost;Database=DeviceManagementDb;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  ```

---

## 8. Main Application Routes

| Route | Feature Description |
|---|---|
| `/` or `/Home/Index` | **Dashboard:** Inventory summary metrics, status cards, category distribution, recent devices. |
| `/Devices` | **Device List:** Full inventory with search (Name/Code) and filters (Category, Status). |
| `/Devices/Create` | **Create Device:** Form with category dropdown, status selector, and entry date. |
| `/Devices/Details/1` | **Device Details:** Detailed hardware specifications and status badge. |
| `/Devices/Edit/1` | **Edit Device:** Update existing hardware metadata. |
| `/Devices/Delete/1` | **Delete Device:** Confirmation screen with destructive post action. |
| `/DeviceCategories` | **Categories List:** Categories with live device counters. |
| `/DeviceCategories/Details/1` | **Category Details:** Category info and nested table of all devices in this category. |
| `/DeviceCategories/Delete/1` | **Delete Category:** Pre-delete check preventing deletion if devices are attached. |
| `/Employees` | **Employee Directory:** List of company staff and device users. |
| `/Employees/Create` | **Add Employee:** Form with full name, email, and phone validation. |

---

## 9. Troubleshooting Common Issues

### Issue 1: LocalDB is not started
If you encounter a SQL connection error:
```powershell
sqllocaldb start MSSQLLocalDB
```

### Issue 2: Applying EF Core Migrations manually
The application applies migrations automatically on startup via `DbInitializer.cs`. If you prefer to update manually:
```powershell
dotnet ef database update --project DeviceManagement
```

### Issue 3: Port conflict
If port 5000/7085 is in use, modify `launchSettings.json` in `DeviceManagement/Properties/launchSettings.json` or pass a custom URL:
```powershell
dotnet run --urls "http://127.0.0.1:5250"
```
