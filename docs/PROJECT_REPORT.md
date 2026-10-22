# Project Report: Device Management System

**Course Code:** CSE 422 — .NET Programming  
**Assignment:** Practice Assignment 2: Building a Device Management Software  
**Framework:** ASP.NET Core 8.0 MVC | Entity Framework Core 8.0  
**Target Solution:** `DeviceManagement.sln`  

---

## 1. Project Purpose

The **Device Management System** is an enterprise-oriented web application built to streamline hardware asset lifecycle management within an organization. It allows administrative personnel to:
1. Maintain an inventory of hardware devices (laptops, workstations, displays, network routers, printers).
2. Organize devices under distinct categories with referential integrity.
3. Manage organizational employees and device users.
4. Perform multi-parameter search and dynamic filtering via database-driven queries.

The architecture emphasizes student comprehension, adherence to university lecture guidelines, separation of concerns, and Clean Code principles.

---

## 2. Technologies Used

- **Runtime & Language:** .NET 8.0 (`net8.0`), C# 12
- **Web Framework:** ASP.NET Core MVC (Model-View-Controller)
- **Data Access:** Entity Framework Core 8.0 (Code-First with Migrations)
- **Database Engine:** Microsoft SQL Server / SQL Server LocalDB (`(localdb)\mssqllocaldb`)
- **UI & Presentation:** Razor View Engine (`.cshtml`), Bootstrap 5, Bootstrap Icons, jQuery Validation
- **Automated Testing:** xUnit 2.9, EF Core In-Memory Provider

---

## 3. Project Structure & Responsibilities

The codebase follows the official ASP.NET Core MVC directory structure:

```
DeviceManagement/
├── Controllers/
│   ├── HomeController.cs             # Aggregates dashboard KPI metrics & category asset breakdown
│   ├── DeviceCategoriesController.cs # CRUD workflows for asset categories & deletion safeguard
│   ├── DevicesController.cs          # CRUD workflows, combined search, and status/category filters
│   └── EmployeesController.cs        # Directory management for device-using personnel
├── Data/
│   ├── DeviceManagementContext.cs    # EF Core DbContext mapping POCOs to SQL Server tables
│   └── DbInitializer.cs              # Migration execution and idempotent sample data seeding
├── Models/
│   ├── DeviceCategory.cs             # Category entity (Id, Name, Description, Devices navigation)
│   ├── Device.cs                     # Device entity (Id, Name, DeviceCode, Category, Status, DateOfEntry)
│   ├── Employee.cs                   # User contact entity (Id, FullName, Email, PhoneNumber)
│   ├── DeviceStatus.cs               # Strongly typed enum: InUse, Broken, UnderMaintenance
│   └── ValidationAttributes/
│       └── NotInFutureAttribute.cs   # Business rule validation rejecting future entry dates
├── ViewModels/
│   ├── DeviceIndexViewModel.cs       # Carries filtered device collection, search keywords, and filter options
│   └── DashboardViewModel.cs         # Carries summary counters and category distribution for the landing page
├── Views/                            # Strongly typed Razor templates with Tag Helpers
├── wwwroot/                          # Client assets (CSS stylesheets, JS scripts, Bootstrap libraries)
└── Program.cs                        # WebApplication bootstrap, DI service registration, HTTP middleware pipeline
```

---

## 4. Database Entities & Relationships

The database schema is generated via EF Core Code-First migrations with the following configuration:

```
[DeviceCategories] 1 <─────── N [Devices]
PK: Id                            PK: Id
Index (Unique): Name              FK: DeviceCategoryId
                                  Index (Unique): DeviceCode
                                  Check: Status IN (1, 2, 3)

[Employees]
PK: Id
Index (Unique): Email
```

### Key Relational Rules:
1. **One-to-Many Relationship:** One `DeviceCategory` contains many `Devices`. Each `Device` belongs to exactly one `DeviceCategory`.
2. **Referential Integrity Protection:** `OnDelete(DeleteBehavior.Restrict)` is configured on the foreign key. Deleting a category that contains active hardware devices is prevented, protecting the inventory from orphaned records.
3. **Unique Keys:** Unique database indexes are placed on `DeviceCategory.Name`, `Device.DeviceCode`, and `Employee.Email`.

---

## 5. Feature Implementation Details

### 5.1 Device Category Management
- Implemented in `DeviceCategoriesController` with `Index`, `Details`, `Create`, `Edit`, and `Delete` actions.
- The `Details` view displays category metadata along with a nested table of all devices currently assigned to that category.
- Duplicate names are checked on both server-side controller level and database level.
- When deleting a category, `Delete.cshtml` checks `ViewBag.HasReferencingDevices`. If devices exist, an alert message is rendered and the confirm button is blocked.

### 5.2 Device Management
- Tracks all required fields: Name, Device Code, Category, Status, and Date of Entry.
- **Allowed Statuses:** Managed by `DeviceStatus` enum (`InUse`, `Broken`, `UnderMaintenance`). In the UI, status values render as stylized badges ("In Use" - green, "Broken" - red, "Under Maintenance" - yellow).
- **Date Handling:** `DateOfEntry` defaults to `DateTime.Today` and is validated via `[NotInFuture]`.
- Category selection utilizes `ViewBag.Categories` populated with a `SelectList`.
- All list and details views eager-load the related category using `.Include(d => d.DeviceCategory)`.

### 5.3 User / Employee Management
- Implemented in `EmployeesController` for staff contacts (`FullName`, `Email`, `PhoneNumber`).
- Validates required fields, email formatting (`[EmailAddress]`), phone formatting (`[Phone]`), and prevents duplicate emails.

### 5.4 Search and Filter Functionality
- Server-side query composition in `DevicesController.Index`:
  ```csharp
  var query = _context.Devices.Include(d => d.DeviceCategory).AsNoTracking().AsQueryable();

  if (!string.IsNullOrWhiteSpace(searchString))
      query = query.Where(d => d.Name.Contains(searchString) || d.DeviceCode.Contains(searchString));

  if (status.HasValue)
      query = query.Where(d => d.Status == status.Value);

  if (categoryId.HasValue && categoryId.Value > 0)
      query = query.Where(d => d.DeviceCategoryId == categoryId.Value);
  ```
- Filtering occurs directly inside SQL Server rather than in-memory.
- An intuitive search bar allows users to combine text search, category dropdown, and status dropdown, accompanied by an instant "Reset" button.

---

## 6. Validation & Clean Code Application

- **Validation:** Server-side validation via `ModelState.IsValid`, Data Annotations (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[RegularExpression]`), and custom validation (`[NotInFuture]`). Client-side validation enabled via `_ValidationScriptsPartial.cshtml`.
- **Security:** Anti-forgery tokens (`[ValidateAntiForgeryToken]`) on all state-changing POST endpoints; parameterized LINQ queries prevent SQL injection.
- **Separation of Concerns:** Views do not query the database; business logic is handled in controllers and validation attributes; controllers access data exclusively through injected `DbContext`.
- **Clean Code Conventions:** Consistent PascalCase and camelCase naming, asynchronous database queries (`async`/`await`), meaningful variable names, zero dead code, and flash messaging via `TempData`.

---

## 7. Verification & Test Execution

An automated test project `DeviceManagement.Tests` was developed and executed using xUnit:
- **28 Automated Unit Tests Passed:**
  - Date boundary tests for `NotInFutureAttribute`.
  - Field constraint & regex validation tests for `Device`, `DeviceCategory`, and `Employee`.
  - Multi-parameter search and combined filtering tests using EF Core In-Memory.
  - Business rule verification for category deletion safety.
- **End-to-End Runtime Verification:** The application was booted against SQL Server LocalDB, automatic migrations and seed data were confirmed, and all routes (`/`, `/Devices`, `/DeviceCategories`, `/Employees`) returned HTTP 200 with live database content.

---

## 8. Summary of How to Run

1. Open `DeviceManagement.sln` in Visual Studio 2022.
2. Verify the connection string in `DeviceManagement/appsettings.json` points to `(localdb)\mssqllocaldb`.
3. Press **F5** to run. The database is migrated and populated automatically on first launch.
