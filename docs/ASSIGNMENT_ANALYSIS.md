# Assignment Analysis: Practice Assignment 2 - Device Management Software

**Course:** CSE 422 - .NET Programming  
**Assignment:** Practice Assignment 2: Building a Device Management Software  
**Target Platform:** ASP.NET Core MVC on .NET 8.0 (`net8.0`)  
**Database:** Microsoft SQL Server / SQL Server LocalDB with Entity Framework Core  

---

## 1. Executive Summary & Objective

The primary objective of Practice Assignment 2 is to design, implement, and document a robust **Device Management Software** web application using ASP.NET Core MVC (.NET 8) and Entity Framework Core, following university lecture guidelines, enterprise Clean Code standards, and Microsoft reference architectures.

The application allows an organization to manage hardware assets, categorize them, track employee directory information, and search/filter devices dynamically using server-side LINQ queries.

---

## 2. Requirement Extraction & Traceability Matrix

| Requirement ID | Assignment Requirement (Screenshots & Spec) | Lecture / Tutorial Reference | Planned Architecture & Implementation |
|---|---|---|---|
| **REQ-CAT-01** | Device Category Management: Add, edit, delete device categories (e.g., Computer, Printer, Phone...). | Lecture Part 1 (MVC Model, DbContext, CRUD Actions) | `DeviceCategoriesController` with standard CRUD actions (`Index`, `Details`, `Create`, `Edit`, `Delete`, `DeleteConfirmed`). Entity: `DeviceCategory`. |
| **REQ-CAT-02** | Display a list of device categories. | Lecture Part 1 (Views, Razor layout, `@foreach`) | Responsive Bootstrap table view with category name, description, device count badge, and action buttons. |
| **REQ-CAT-03** | Category input validation and duplicate prevention. | Microsoft MVC Tutorial Part 9 & Lecture Part 2 | Data Annotations (`[Required]`, `[StringLength]`), unique index in EF Core, server-side duplicate name check in Controller, client-side unobtrusive validation. |
| **REQ-CAT-04** | Referencing integrity: Prevent deletion of categories containing devices. | EF Core Relationship Rules & Clean Code | Foreign key `DeleteBehavior.Restrict` in EF Core model builder; pre-delete check in `DeviceCategoriesController.Delete` displaying user-friendly blocking alert if device count > 0. |
| **REQ-DEV-01** | Device Management: Add, edit, delete devices. | Lecture Part 1 & Microsoft Tutorial CRUD | `DevicesController` with complete GET/POST CRUD actions, antiforgery token validation, and redirect with TempData notifications. |
| **REQ-DEV-02** | Required Device Fields: Name, Device Code, Device Category, Status, Date of Entry. | Assignment Spec Screenshot 2, Item 2 | `Device` model with `Id`, `Name`, `DeviceCode`, `DeviceCategoryId`, `DeviceCategory` navigation property, `Status` (`DeviceStatus` enum), `DateOfEntry` (`DateTime`). |
| **REQ-DEV-03** | Status Values: Exactly `InUse`, `Broken`, `UnderMaintenance`. | Assignment Spec Screenshot 2, Item 2 | Strongly typed C# `enum DeviceStatus { [Display(Name = "In Use")] InUse = 1, [Display(Name = "Broken")] Broken = 2, [Display(Name = "Under Maintenance")] UnderMaintenance = 3 }`. |
| **REQ-DEV-04** | Status UI Display: Friendly labels "In Use", "Broken", "Under Maintenance". | Lecture Part 2 (Razor Display Helpers) | Razor helper/partial rendering styled Bootstrap badges (Success for In Use, Danger for Broken, Warning for Under Maintenance). |
| **REQ-DEV-05** | Category Dropdown Selection. | Lecture Part 2 (Slide 43-46: Populating Lookup Data) | `SelectList` constructed from `DeviceCategory` items passed via ViewModel or ViewBag, rendered using `asp-items`. |
| **REQ-DEV-06** | Display devices by category. | Assignment Spec Screenshot 2, Item 2 | Filterable device list by category, category direct link (e.g. `/Devices?categoryId=X`), and category details showing nested device collection. |
| **REQ-DEV-07** | Date of Entry Validation: Cannot be in the future. | System Business Rules & Clean Validation | Custom validation attribute `[NotInFuture(ErrorMessage = "Date of entry cannot be in the future.")]` applied to `DateOfEntry`. |
| **REQ-DEV-08** | Unique Device Code Validation. | Business Rules & Assignment Spec | Database unique index on `DeviceCode`, controller duplicate validation on Create and Edit. |
| **REQ-DEV-09** | Display Category Name in Device Views. | Assignment Spec & EF Core Eager Loading | EF Core `.Include(d => d.DeviceCategory)` in queries to display `category.Name` instead of just raw ID. |
| **REQ-USR-01** | User Management: Add, edit, delete users (employees who use devices). | Assignment Spec Screenshot 2, Item 3 | `EmployeesController` with standard CRUD actions. Model: `Employee`. |
| **REQ-USR-02** | Required User Fields: Full Name, Email, Phone Number. | Assignment Spec Screenshot 2, Item 3 | `Employee` model with `Id`, `FullName`, `Email`, `PhoneNumber`. |
| **REQ-USR-03** | User Validation: Required fields, valid email, valid phone, unique email. | Assignment Spec & Clean Code | `[Required]`, `[StringLength]`, `[EmailAddress]`, `[Phone]`, plus controller-level duplicate email check. |
| **REQ-SRCH-01**| Search devices by Name or Device Code. | Assignment Spec Screenshot 1, Item 4 | Combined case-insensitive search predicate in LINQ: `d.Name.Contains(query) \|\| d.DeviceCode.Contains(query)`. |
| **REQ-SRCH-02**| Filter device list by Status or Category. | Assignment Spec Screenshot 1, Item 4 | LINQ filters: `d.Status == statusFilter` and `d.DeviceCategoryId == categoryIdFilter`. |
| **REQ-SRCH-03**| Combined Search and Filters. | Assignment Spec Screenshot 1 & Prompt Sec 4.4 | Single composite LINQ query building chained `.Where(...)` clauses executed on SQL Server. |
| **REQ-SRCH-04**| Search Form with Reset Button & Filter Preservation. | Prompt Sec 4.4 & Good UX | GET form on Devices Index with input fields, category select, status select, "Search" button, and "Reset" button leading back to `/Devices`. |
| **REQ-UI-01**  | Simple, user-friendly interface with Bootstrap. | Assignment Spec Screenshot 1 (Expected Outcomes) | Modern responsive dashboard layout with cards, navbar, table views, modals/confirmation prompts, and alert banners. |
| **REQ-DI-01**  | Dependency Injection for DbContext. | Lecture Part 1 (Slide 51-54) & Part 2 (Slide 22-26) | `DeviceManagementContext` registered in `Program.cs` via `builder.Services.AddDbContext<DeviceManagementContext>(...)` and injected via controller constructors. |
| **REQ-DB-01**  | SQL Server & EF Core Integration. | Lecture Part 1 (Slide 48-54) | EF Core 8.0 with `UseSqlServer`, connection string in `appsettings.json`, automatic migrations and initial data seed. |
| **REQ-DOC-01** | Comprehensive Documentation & Lab Report. | Assignment Spec Screenshot 1 & Prompt Sec 12 | `README.md`, `docs/PROJECT_REPORT.md`, `docs/REQUIREMENTS_CHECKLIST.md`, and `docs/LEARNING_GUIDE.md`. |

---

## 3. Assumptions and Design Decisions

1. **Date of Entry Business Rule:**
   - *Assumption:* A device cannot be officially entered into the inventory with a future date. A custom validation attribute `NotInFutureAttribute` verifies that `DateOfEntry.Date <= DateTime.Today`.
2. **Category Deletion Integrity:**
   - *Assumption:* Cascading deletes of devices when a category is deleted is dangerous in an asset inventory. Therefore, EF Core is configured with `OnDelete(DeleteBehavior.Restrict)`. If a user attempts to delete a category that still contains devices, the application displays an informational warning message and blocks the deletion until devices are reassigned or removed.
3. **Authentication Scope:**
   - *Assumption:* The assignment requires "User Management (including employees who use devices): Full name, email, phone number". Per explicit instructions, no ASP.NET Identity or login mechanism is introduced; the focus is kept squarely on the employee directory and asset management.
4. **Database Provider:**
   - SQL Server LocalDB (`(localdb)\mssqllocaldb`) is configured as the default out-of-the-box connection string for seamless execution in Visual Studio on Windows, with a clear configuration fallback documented for standard SQL Server instances (`localhost` / `SQLEXPRESS`).
