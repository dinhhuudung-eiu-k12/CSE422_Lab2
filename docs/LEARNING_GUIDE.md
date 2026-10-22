# ASP.NET Core MVC Learning Guide: Device Management System

**Target Audience:** University students learning .NET programming, C#, and ASP.NET Core MVC  
**Bilingual Terms:** Important technical terms include English definitions with Vietnamese explanations (Thuật ngữ Tiếng Việt) to support classroom presentations and oral defense before lecturers.

---

## 1. How an HTTP Request Travels Through MVC (Vòng đời của một HTTP Request)

When a user interacts with a web browser (e.g., clicking **Devices** in the navigation menu), the request flows through three distinct components:

```
[Browser Client] 
      │ (1) HTTP GET /Devices?categoryId=1
      ▼
[Routing Middleware] (Program.cs)
      │ Matches pattern: {controller=Devices}/{action=Index}
      ▼
[DevicesController]
      │ (2) Executes LINQ query via DbContext
      ▼
[DeviceManagementContext & SQL Server]
      │ (3) Returns filtered Device entities
      ▼
[DevicesController]
      │ (4) Bundles data into DeviceIndexViewModel and calls View(viewModel)
      ▼
[Razor View Engine] (Views/Devices/Index.cshtml)
      │ (5) Generates pure HTML output
      ▼
[Browser Client] (Renders HTML response)
```

### Key Concept: Separation of Concerns (Phân tách mối quan tâm)
- **Controller (Bộ điều khiển):** Receives the request, validates input, talks to the database, and decides which view to render.
- **Model (Mô hình dữ liệu):** Represents the data and validation rules (`Device`, `DeviceCategory`, `Employee`).
- **View (Giao diện hiển thị):** Formats the HTML for the user. A view should **never** talk directly to SQL Server or execute business calculations.

---

## 2. What `Program.cs` Does (Điểm khởi đầu của ứng dụng)

`Program.cs` is the entry point of the entire application. It performs three critical jobs:
1. **Creates the WebApplicationBuilder:** Initializes configuration from `appsettings.json`.
2. **Registers Dependencies in the DI Container:** Tells .NET which services are available throughout the app.
3. **Configures the HTTP Request Pipeline (Middleware Pipeline):** Controls how requests are handled (e.g., static files, routing, authorization).

### Real Code Snippet from `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC controllers and Razor views
builder.Services.AddControllersWithViews();

// 2. Register EF Core DbContext with SQL Server
builder.Services.AddDbContext<DeviceManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 3. Configure HTTP pipeline (Middleware)
app.UseHttpsRedirection();
app.UseStaticFiles(); // Serves css, js, images from wwwroot
app.UseRouting();
app.UseAuthorization();

// 4. Default MVC Route: /Controller/Action/Id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## 3. Dependency Injection (Tiêm phụ thuộc - DI)

### What is Dependency Injection?
Instead of creating a database context inside each controller using `new DeviceManagementContext()`, ASP.NET Core creates the context for us and passes it through the controller's **constructor** (*Constructor Injection*).

### Why is this beneficial?
- **Loose Coupling (Giảm sự phụ thuộc chặt chẽ):** The controller does not care how the database connection is configured.
- **Testability (Khả năng kiểm thử):** In unit tests, we can inject an In-Memory test database instead of a live SQL Server without changing a single line of controller code!
- **Lifecycle Management (Quản lý vòng đời):** EF Core `DbContext` is registered with a **Scoped** lifetime, meaning one context instance is created per HTTP request and cleanly disposed of when the request finishes.

### Real Code Snippet from `DevicesController.cs`:
```csharp
public class DevicesController : Controller
{
    private readonly DeviceManagementContext _context;

    // The DI container automatically supplies 'context' when DevicesController is created
    public DevicesController(DeviceManagementContext context)
    {
        _context = context;
    }
}
```

---

## 4. How EF Core Interacts with SQL Server (Tương tác với Cơ sở dữ liệu)

**Entity Framework Core (EF Core)** is an Object-Relational Mapper (ORM). It allows us to write queries in C# using **LINQ (Language Integrated Query)** instead of writing raw SQL strings (`SELECT * FROM Devices`).

### Eager Loading with `.Include()` (Nạp dữ liệu liên kết)
In relational databases, each device stores `DeviceCategoryId` (Foreign Key). If we want to display the category name (`category.Name`) in the device table, we use `.Include(d => d.DeviceCategory)`. This performs a SQL `LEFT JOIN` in a single optimized query!

```csharp
var devices = await _context.Devices
    .Include(d => d.DeviceCategory) // Eager loading
    .AsNoTracking()                 // Optimization: read-only query without change tracking
    .ToListAsync();
```

---

## 5. How Razor Views Display Data (Cách Razor View hiển thị dữ liệu)

Razor files (`.cshtml`) combine standard HTML with C# expressions using the `@` symbol.

### Key Directives:
- `@model`: Declares the type of data the view expects.
- `@foreach`: Loops over a collection of items.
- `@if` / `@else`: Conditionally renders HTML blocks.

### Real Code Snippet from `Views/Devices/Index.cshtml`:
```razor
@model DeviceManagement.ViewModels.DeviceIndexViewModel

@foreach (var item in Model.Devices)
{
    <tr>
        <td>@item.Name</td>
        <td><code>@item.DeviceCode</code></td>
        <td>@(item.DeviceCategory?.Name ?? "Unassigned")</td>
        <td>
            @if (item.Status == DeviceStatus.InUse)
            {
                <span class="badge bg-success">In Use</span>
            }
            else if (item.Status == DeviceStatus.UnderMaintenance)
            {
                <span class="badge bg-warning text-dark">Under Maintenance</span>
            }
            else if (item.Status == DeviceStatus.Broken)
            {
                <span class="badge bg-danger">Broken</span>
            }
        </td>
        <td>@item.DateOfEntry.ToString("yyyy-MM-dd")</td>
    </tr>
}
```

---

## 6. How Model Validation Works (Cơ chế kiểm tra tính hợp lệ dữ liệu)

Validation ensures invalid data (such as future dates, missing names, or malformed emails) never corrupts the database.

### 1. Data Annotations on the Model:
```csharp
public class Device
{
    [Required(ErrorMessage = "Device name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[A-Za-z0-9\-_]+$", ErrorMessage = "Only letters, numbers, hyphens, and underscores.")]
    public string DeviceCode { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [NotInFuture(ErrorMessage = "Date of entry cannot be in the future.")]
    public DateTime DateOfEntry { get; set; }
}
```

### 2. Custom Validation Attribute (`NotInFutureAttribute.cs`):
```csharp
public class NotInFutureAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime && dateTime.Date > DateTime.Today)
        {
            return new ValidationResult("Date of entry cannot be in the future.");
        }
        return ValidationResult.Success;
    }
}
```

### 3. Server-side Controller Validation (`ModelState.IsValid`):
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Device device)
{
    if (ModelState.IsValid)
    {
        _context.Add(device);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Validation failed: reload dropdowns and redisplay form with error messages
    await PopulateCategoriesDropDownListAsync(device.DeviceCategoryId);
    return View(device);
}
```

---

## 7. How Search & Filter Queries Work (Truy vấn Tìm kiếm và Lọc dữ liệu)

Instead of fetching all devices into memory and filtering them in C# (which is slow for large datasets), we dynamically build an `IQueryable<Device>` pipeline. EF Core translates this entire pipeline into a single SQL `WHERE` clause executed directly on SQL Server:

```csharp
var query = _context.Devices
    .Include(d => d.DeviceCategory)
    .AsNoTracking()
    .AsQueryable();

// 1. Text Search (Matches Name OR Device Code)
if (!string.IsNullOrWhiteSpace(searchString))
{
    var trimmed = searchString.Trim();
    query = query.Where(d => d.Name.Contains(trimmed) || d.DeviceCode.Contains(trimmed));
}

// 2. Filter by Status
if (status.HasValue)
{
    query = query.Where(d => d.Status == status.Value);
}

// 3. Filter by Category
if (categoryId.HasValue && categoryId.Value > 0)
{
    query = query.Where(d => d.DeviceCategoryId == categoryId.Value);
}

// Execute query on SQL Server
var results = await query.ToListAsync();
```

---

## 8. Relational Integrity: Preventing Orphaned Devices (Ràng buộc toàn vẹn)

In a relational database, what happens if an administrator tries to delete a category (e.g., "Laptops") while 10 laptop devices still belong to it?
- If **Cascade Delete** is used, all 10 laptops would be silently deleted from the database! This is dangerous for asset tracking.
- In our project, we enforce **`DeleteBehavior.Restrict`**:
  ```csharp
  modelBuilder.Entity<Device>()
      .HasOne(d => d.DeviceCategory)
      .WithMany(c => c.Devices)
      .HasForeignKey(d => d.DeviceCategoryId)
      .OnDelete(DeleteBehavior.Restrict);
  ```
- In `DeviceCategoriesController.Delete`, we check `category.Devices.Count > 0`. If devices exist, the UI renders an informative warning banner and blocks the delete button, protecting data integrity.

---

## 9. Clean Code Principles Demonstrated in this Project (Nguyên lý viết mã sạch)

1. **Meaningful Names:** Descriptive names (`DeviceStatus`, `NotInFutureAttribute`, `PopulateCategoriesDropDownListAsync`) rather than cryptic abbreviations.
2. **Single Responsibility Principle (SRP):** Controllers handle HTTP endpoints; models define structure and validation; views present HTML.
3. **DRY (Don't Repeat Yourself):** Reusable layout `_Layout.cshtml`, partial views `_Alerts.cshtml` and `_ValidationScriptsPartial.cshtml`, and shared view models.
4. **Security by Default:** Anti-forgery tokens (`[ValidateAntiForgeryToken]`) protect against Cross-Site Request Forgery (CSRF); parameterized queries protect against SQL injection.
5. **Asynchronous Programming (`async`/`await`):** All database operations use async methods (`ToListAsync()`, `SaveChangesAsync()`), ensuring high server concurrency without thread starvation.

---

## 10. Glossary of Key Terms (Bảng tra cứu thuật ngữ Anh - Việt)

| English Term | Thuật ngữ Tiếng Việt | Explanation |
|---|---|---|
| **Controller** | Bộ điều khiển | C# class that accepts HTTP requests and returns views or data. |
| **Model** | Mô hình dữ liệu | C# class defining entities and business data structures. |
| **View** | Giao diện hiển thị | `.cshtml` file that renders HTML output sent to the browser. |
| **Dependency Injection** | Tiêm phụ thuộc | Design pattern where required services are supplied from an external container. |
| **ORM (Object-Relational Mapping)** | Ánh xạ đối tượng - quan hệ | Framework (EF Core) that converts database tables into C# objects. |
| **Migration** | Di chuyển / Nâng cấp CSDL | Code-first schema changes versioned in C# files. |
| **Foreign Key** | Khóa ngoại | Column in a child table linking to the primary key of a parent table. |
| **Eager Loading** | Nạp dữ liệu sớm | Loading related entities in a single SQL JOIN query using `.Include()`. |
| **Data Annotations** | Chú thích dữ liệu | Attributes (e.g. `[Required]`) specifying rules and display metadata. |
| **Anti-Forgery Token** | Mã thông báo chống giả mạo | Security token preventing CSRF attacks on POST forms. |
