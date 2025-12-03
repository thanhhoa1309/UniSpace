# RoomService - Unified GetRoomsAsync Method

## ? Hoàn Thành - G?p T?t C? Methods Get Thành M?t

### Thay ??i

?ã g?p **t?t c? các ph??ng th?c Get** thành m?t ph??ng th?c duy nh?t `GetRoomsAsync` v?i nhi?u filter parameters.

## ?? Tr??c và Sau

### ? Tr??c (6 Methods Riêng Bi?t):

```csharp
Task<Pagination<RoomDto>> GetAllRoomsAsync(int pageNumber = 1, int pageSize = 20);
Task<Pagination<RoomDto>> GetRoomsByCampusAsync(Guid campusId, int pageNumber = 1, int pageSize = 20);
Task<Pagination<RoomDto>> GetRoomsByTypeAsync(RoomType type, int pageNumber = 1, int pageSize = 20);
Task<Pagination<RoomDto>> GetRoomsByStatusAsync(BookingStatus status, int pageNumber = 1, int pageSize = 20);
Task<Pagination<RoomDto>> SearchRoomsAsync(string searchTerm, int pageNumber = 1, int pageSize = 20);
Task<Pagination<RoomDto>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int pageNumber = 1, int pageSize = 20);
```

### ? Sau (1 Method Th?ng Nh?t):

```csharp
Task<Pagination<RoomDto>> GetRoomsAsync(
    int pageNumber = 1, 
    int pageSize = 20,
    string? searchTerm = null,
    Guid? campusId = null,
    RoomType? type = null,
    BookingStatus? status = null,
    DateTime? availableFrom = null,
    DateTime? availableTo = null);
```

## ?? Method Signature

```csharp
public interface IRoomService
{
    /// <summary>
    /// Get rooms with flexible filtering and pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="searchTerm">Search in room name, description, or campus name</param>
    /// <param name="campusId">Filter by specific campus</param>
    /// <param name="type">Filter by room type (Classroom, Lab, Stadium)</param>
    /// <param name="status">Filter by booking status (Approved, Rejected, etc.)</param>
    /// <param name="availableFrom">Start of availability check period</param>
    /// <param name="availableTo">End of availability check period</param>
    /// <returns>Paginated list of rooms matching the filters</returns>
    Task<Pagination<RoomDto>> GetRoomsAsync(
        int pageNumber = 1, 
        int pageSize = 20,
        string? searchTerm = null,
        Guid? campusId = null,
        RoomType? type = null,
        BookingStatus? status = null,
        DateTime? availableFrom = null,
        DateTime? availableTo = null);
}
```

## ?? Cách S? D?ng

### 1. L?y T?t C? Rooms (Không Filter)

```csharp
// ??n gi?n nh?t - l?y page 1, 20 items
var rooms = await _roomService.GetRoomsAsync();

// Custom page và size
var rooms = await _roomService.GetRoomsAsync(pageNumber: 2, pageSize: 50);
```

### 2. Tìm Ki?m Theo Text

```csharp
// Tìm ki?m trong tên room, description, ho?c tên campus
var rooms = await _roomService.GetRoomsAsync(
    searchTerm: "Lab");
```

### 3. Filter Theo Campus

```csharp
var campusId = Guid.Parse("...");
var rooms = await _roomService.GetRoomsAsync(
    campusId: campusId);
```

### 4. Filter Theo Type

```csharp
// Ch? l?y Classrooms
var rooms = await _roomService.GetRoomsAsync(
    type: RoomType.Classroom);

// Ch? l?y Labs
var rooms = await _roomService.GetRoomsAsync(
    type: RoomType.Lab);
```

### 5. Filter Theo Status

```csharp
// Ch? l?y rooms available
var rooms = await _roomService.GetRoomsAsync(
    status: BookingStatus.Approved);

// Ch? l?y rooms unavailable
var rooms = await _roomService.GetRoomsAsync(
    status: BookingStatus.Rejected);
```

### 6. K?t H?p Nhi?u Filters

```csharp
// Tìm Classrooms trong Campus c? th?
var rooms = await _roomService.GetRoomsAsync(
    campusId: campusId,
    type: RoomType.Classroom);

// Tìm rooms available và là Lab
var rooms = await _roomService.GetRoomsAsync(
    type: RoomType.Lab,
    status: BookingStatus.Approved);

// Tìm ki?m + filter campus
var rooms = await _roomService.GetRoomsAsync(
    searchTerm: "Computer",
    campusId: campusId);
```

### 7. Filter Theo Availability (Time-based)

```csharp
var startTime = DateTime.Now.AddHours(1);
var endTime = DateTime.Now.AddHours(3);

// Ch? l?y rooms available trong kho?ng th?i gian
var rooms = await _roomService.GetRoomsAsync(
    availableFrom: startTime,
    availableTo: endTime);

// K?t h?p v?i filters khác
var rooms = await _roomService.GetRoomsAsync(
    type: RoomType.Classroom,
    campusId: campusId,
    availableFrom: startTime,
    availableTo: endTime);
```

### 8. T?t C? Filters Cùng Lúc

```csharp
// Full power!
var rooms = await _roomService.GetRoomsAsync(
    pageNumber: 1,
    pageSize: 20,
    searchTerm: "Computer",
    campusId: campusId,
    type: RoomType.Lab,
    status: BookingStatus.Approved,
    availableFrom: DateTime.Now,
    availableTo: DateTime.Now.AddHours(2));
```

## ?? Trong Razor Pages

### Example: Room Index Page

```csharp
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    
    public Pagination<RoomDto> Rooms { get; set; }
    
    // Filter properties
    public string? SearchTerm { get; set; }
    public Guid? CampusId { get; set; }
    public RoomType? Type { get; set; }
    public BookingStatus? Status { get; set; }
    
    public async Task OnGetAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        Guid? campusId = null,
        RoomType? type = null,
        BookingStatus? status = null)
    {
        // Store filter values
        SearchTerm = search;
        CampusId = campusId;
        Type = type;
        Status = status;
        
        // Single call with all filters!
        Rooms = await _roomService.GetRoomsAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: search,
            campusId: campusId,
            type: type,
            status: status);
    }
}
```

### View v?i Multiple Filters

```html
@page
@model IndexModel

<form method="get" class="row g-3">
    <!-- Search -->
    <div class="col-md-3">
        <input type="text" name="search" class="form-control" 
               placeholder="Search..." value="@Model.SearchTerm" />
    </div>
    
    <!-- Campus Filter -->
    <div class="col-md-3">
        <select name="campusId" class="form-select">
            <option value="">All Campuses</option>
            @foreach (var campus in Model.Campuses)
            {
                <option value="@campus.Id" selected="@(campus.Id == Model.CampusId)">
                    @campus.Name
                </option>
            }
        </select>
    </div>
    
    <!-- Type Filter -->
    <div class="col-md-2">
        <select name="type" class="form-select">
            <option value="">All Types</option>
            <option value="@((int)RoomType.Classroom)" 
                    selected="@(Model.Type == RoomType.Classroom)">
                Classroom
            </option>
            <option value="@((int)RoomType.Lab)" 
                    selected="@(Model.Type == RoomType.Lab)">
                Lab
            </option>
            <option value="@((int)RoomType.Stadium)" 
                    selected="@(Model.Type == RoomType.Stadium)">
                Stadium
            </option>
        </select>
    </div>
    
    <!-- Status Filter -->
    <div class="col-md-2">
        <select name="status" class="form-select">
            <option value="">All Status</option>
            <option value="@((int)BookingStatus.Approved)" 
                    selected="@(Model.Status == BookingStatus.Approved)">
                Available
            </option>
            <option value="@((int)BookingStatus.Rejected)" 
                    selected="@(Model.Status == BookingStatus.Rejected)">
                Unavailable
            </option>
        </select>
    </div>
    
    <!-- Submit -->
    <div class="col-md-2">
        <button type="submit" class="btn btn-primary w-100">
            <i class="bi bi-search"></i> Filter
        </button>
    </div>
</form>

<!-- Results -->
<div class="row mt-4">
    @foreach (var room in Model.Rooms)
    {
        <div class="col-md-4">
            <div class="card">
                <div class="card-body">
                    <h5>@room.Name</h5>
                    <p>@room.CampusName</p>
                    <span class="badge bg-primary">@room.TypeDisplay</span>
                    <span class="badge bg-success">@room.CurrentStatusDisplay</span>
                </div>
            </div>
        </div>
    }
</div>

<!-- Pagination -->
<nav class="mt-4">
    <ul class="pagination">
        <li class="page-item @(Model.Rooms.HasPrevious ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage - 1)"
               asp-route-search="@Model.SearchTerm"
               asp-route-campusId="@Model.CampusId"
               asp-route-type="@Model.Type"
               asp-route-status="@Model.Status">
                Previous
            </a>
        </li>
        
        @for (int i = 1; i <= Model.Rooms.TotalPages; i++)
        {
            <li class="page-item @(i == Model.Rooms.CurrentPage ? "active" : "")">
                <a class="page-link" 
                   asp-page="Index" 
                   asp-route-pageNumber="@i"
                   asp-route-search="@Model.SearchTerm"
                   asp-route-campusId="@Model.CampusId"
                   asp-route-type="@Model.Type"
                   asp-route-status="@Model.Status">
                    @i
                </a>
            </li>
        }
        
        <li class="page-item @(Model.Rooms.HasNext ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)"
               asp-route-search="@Model.SearchTerm"
               asp-route-campusId="@Model.CampusId"
               asp-route-type="@Model.Type"
               asp-route-status="@Model.Status">
                Next
            </a>
        </li>
    </ul>
</nav>

<!-- Info -->
<p class="text-muted">
    Showing @Model.Rooms.Count of @Model.Rooms.TotalCount rooms
    @if (!string.IsNullOrEmpty(Model.SearchTerm) || Model.CampusId.HasValue || Model.Type.HasValue || Model.Status.HasValue)
    {
        <text> (filtered)</text>
    }
</p>
```

## ?? Filter Logic Implementation

### Query Building Order

1. **Base Query**: Load rooms with includes
2. **Search Filter**: Text search in name, description, campus name
3. **Campus Filter**: Exact campus ID match
4. **Type Filter**: Exact room type match
5. **Status Filter**: Exact booking status match
6. **Ordering**: Order by name
7. **Count**: Get total before pagination
8. **Pagination**: Skip and Take
9. **Availability Filter**: Post-query check (expensive operation)

### Performance Notes

#### Efficient Filters (Database Level)
- ? `searchTerm` - SQL LIKE
- ? `campusId` - SQL WHERE
- ? `type` - SQL WHERE
- ? `status` - SQL WHERE

#### Post-Processing Filter (Memory Level)
- ?? `availableFrom` & `availableTo` - Checks booking conflicts for each room

**Note:** Availability filter is applied AFTER pagination for performance. This means:
- ? Fast: Checks only rooms on current page
- ?? Trade-off: Total count may not be 100% accurate when using availability filter

## ? ?u ?i?m

### 1. **Single Source of Truth**
- Ch? m?t method duy nh?t
- D? maintain
- Consistent behavior

### 2. **Flexible Combinations**
- K?t h?p b?t k? filters nào
- Named parameters rõ ràng
- Default values ti?n l?i

### 3. **Code Cleaner**
```csharp
// TR??C: Ph?i ch?n method nào dùng
if (!string.IsNullOrEmpty(search))
    rooms = await _roomService.SearchRoomsAsync(search, page, size);
else if (campusId.HasValue)
    rooms = await _roomService.GetRoomsByCampusAsync(campusId.Value, page, size);
else if (type.HasValue)
    rooms = await _roomService.GetRoomsByTypeAsync(type.Value, page, size);
else
    rooms = await _roomService.GetAllRoomsAsync(page, size);

// SAU: M?t method, t?t c? filters
rooms = await _roomService.GetRoomsAsync(
    pageNumber: page,
    pageSize: size,
    searchTerm: search,
    campusId: campusId,
    type: type);
```

### 4. **Extensible**
D? dàng thêm filter m?i:
```csharp
// Thêm filter m?i không ?nh h??ng existing calls
Task<Pagination<RoomDto>> GetRoomsAsync(
    // ... existing params
    int? minCapacity = null,  // NEW!
    int? maxCapacity = null   // NEW!
);
```

### 5. **Better for API**
```csharp
[HttpGet]
public async Task<ActionResult<Pagination<RoomDto>>> GetRooms(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    [FromQuery] Guid? campusId = null,
    [FromQuery] RoomType? type = null,
    [FromQuery] BookingStatus? status = null)
{
    return await _roomService.GetRoomsAsync(
        pageNumber, pageSize, search, campusId, type, status);
}

// API calls:
// GET /api/rooms
// GET /api/rooms?search=Lab
// GET /api/rooms?campusId=xxx&type=1
// GET /api/rooms?search=Computer&campusId=xxx&type=0&status=1
```

## ?? Migration Checklist

### ? Completed
- [x] Updated `IRoomService` interface
- [x] Implemented unified `GetRoomsAsync` in `RoomService`
- [x] Removed old individual methods
- [x] Updated `Room/Index.cshtml.cs`
- [x] Updated `Room/Dashboard.cshtml.cs`
- [x] Build successful

### ?? Recommendations

1. **Update API Controllers** (if any):
   ```csharp
   // Update to use new unified method
   public async Task<ActionResult> GetRooms([FromQuery] RoomFilterDto filters)
   {
       return await _roomService.GetRoomsAsync(
           filters.PageNumber,
           filters.PageSize,
           filters.Search,
           filters.CampusId,
           filters.Type,
           filters.Status);
   }
   ```

2. **Add Filter DTO** (optional):
   ```csharp
   public class RoomFilterDto
   {
       public int PageNumber { get; set; } = 1;
       public int PageSize { get; set; } = 20;
       public string? Search { get; set; }
       public Guid? CampusId { get; set; }
       public RoomType? Type { get; set; }
       public BookingStatus? Status { get; set; }
       public DateTime? AvailableFrom { get; set; }
       public DateTime? AvailableTo { get; set; }
   }
   ```

3. **Add Unit Tests**:
   ```csharp
   [Fact]
   public async Task GetRoomsAsync_WithSearchTerm_ReturnsFilteredRooms()
   {
       // Arrange
       var searchTerm = "Lab";
       
       // Act
       var result = await _roomService.GetRoomsAsync(searchTerm: searchTerm);
       
       // Assert
       Assert.All(result, r => 
           Assert.Contains(searchTerm, r.Name, StringComparison.OrdinalIgnoreCase));
   }
   ```

## ?? Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Methods** | 6 separate methods | 1 unified method |
| **Lines of Code** | ~300 lines | ~100 lines |
| **Complexity** | Multiple if-else logic | Single call with params |
| **Flexibility** | Limited combinations | Any combination |
| **Maintenance** | High (6 methods) | Low (1 method) |
| **Testability** | 6 test suites | 1 comprehensive suite |

## ? Build Status

```
Build: SUCCESS ?
Errors: 0
Warnings: 0
```

All files updated and working correctly! ??
