# Room Service Pagination - Usage Guide

## Overview

The RoomService has been updated to support pagination for all list-returning methods. This allows for efficient data retrieval when dealing with large datasets.

## Changes Made

### 1. **New Pagination Class** (`Pagination.cs`)
Already existed in your codebase at `UniSpace.Services.Utils.Pagination<T>`

Properties:
- `CurrentPage`: Current page number
- `TotalPages`: Total number of pages
- `PageSize`: Number of items per page
- `TotalCount`: Total number of items
- `HasPrevious`: Boolean indicating if there's a previous page
- `HasNext`: Boolean indicating if there's a next page

### 2. **New Pagination Extensions** (`PaginationExtensions.cs`)
Helper methods to convert IQueryable or List to Pagination:

```csharp
// For IQueryable
await query.ToPaginationAsync(pageNumber, pageSize);

// For List
list.ToPagination(pageNumber, pageSize);
```

### 3. **Updated IRoomService Interface**
Added paginated versions of all list-returning methods:

```csharp
// Non-paginated (returns all)
Task<List<RoomDto>> GetAllRoomsAsync();

// Paginated (returns page)
Task<Pagination<RoomDto>> GetAllRoomsAsync(int pageNumber, int pageSize);
```

### 4. **Updated RoomService Implementation**
Implemented all paginated methods with efficient database queries using `Skip` and `Take`.

## Available Paginated Methods

| Method | Description |
|--------|-------------|
| `GetAllRoomsAsync(pageNumber, pageSize)` | Get all rooms with pagination |
| `GetRoomsByCampusAsync(campusId, pageNumber, pageSize)` | Get rooms by campus with pagination |
| `GetRoomsByTypeAsync(type, pageNumber, pageSize)` | Get rooms by type with pagination |
| `GetRoomsByStatusAsync(status, pageNumber, pageSize)` | Get rooms by status with pagination |
| `SearchRoomsAsync(searchTerm, pageNumber, pageSize)` | Search rooms with pagination |
| `GetAvailableRoomsAsync(startTime, endTime, pageNumber, pageSize)` | Get available rooms with pagination |

## Usage Examples

### Example 1: Simple Pagination in Razor Pages

```csharp
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    
    public Pagination<RoomDto> Rooms { get; set; }
    
    public async Task OnGetAsync(int pageNumber = 1)
    {
        const int pageSize = 10;
        Rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
    }
}
```

### Example 2: Pagination with Filters

```csharp
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    
    public Pagination<RoomDto> Rooms { get; set; }
    public string SearchTerm { get; set; }
    public Guid? CampusId { get; set; }
    
    public async Task OnGetAsync(
        int pageNumber = 1, 
        string? search = null, 
        Guid? campusId = null)
    {
        const int pageSize = 20;
        SearchTerm = search;
        CampusId = campusId;
        
        if (!string.IsNullOrEmpty(search))
        {
            Rooms = await _roomService.SearchRoomsAsync(search, pageNumber, pageSize);
        }
        else if (campusId.HasValue)
        {
            Rooms = await _roomService.GetRoomsByCampusAsync(
                campusId.Value, pageNumber, pageSize);
        }
        else
        {
            Rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
        }
    }
}
```

### Example 3: Razor View with Pagination Controls

```html
@model IndexModel

<!-- Display items -->
<div class="row">
    @foreach (var room in Model.Rooms)
    {
        <div class="col-md-4">
            <div class="card">
                <div class="card-body">
                    <h5>@room.Name</h5>
                    <p>@room.CampusName</p>
                </div>
            </div>
        </div>
    }
</div>

<!-- Pagination controls -->
<nav aria-label="Room pagination">
    <ul class="pagination justify-content-center">
        <!-- Previous button -->
        <li class="page-item @(Model.Rooms.HasPrevious ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage - 1)"
               asp-route-search="@Model.SearchTerm">
                Previous
            </a>
        </li>
        
        <!-- Page numbers -->
        @for (int i = 1; i <= Model.Rooms.TotalPages; i++)
        {
            <li class="page-item @(i == Model.Rooms.CurrentPage ? "active" : "")">
                <a class="page-link" 
                   asp-page="Index" 
                   asp-route-pageNumber="@i"
                   asp-route-search="@Model.SearchTerm">
                    @i
                </a>
            </li>
        }
        
        <!-- Next button -->
        <li class="page-item @(Model.Rooms.HasNext ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)"
               asp-route-search="@Model.SearchTerm">
                Next
            </a>
        </li>
    </ul>
</nav>

<!-- Pagination info -->
<div class="text-center text-muted">
    <small>
        Showing @((Model.Rooms.CurrentPage - 1) * Model.Rooms.PageSize + 1) 
        to @(Math.Min(Model.Rooms.CurrentPage * Model.Rooms.PageSize, Model.Rooms.TotalCount))
        of @Model.Rooms.TotalCount items
    </small>
</div>
```

### Example 4: Advanced Pagination with Bootstrap

```html
@model IndexModel

<div class="container">
    <!-- Items grid -->
    <div class="row mb-4">
        @if (Model.Rooms.Any())
        {
            @foreach (var room in Model.Rooms)
            {
                <div class="col-md-4 mb-3">
                    <div class="card">
                        <div class="card-header bg-primary text-white">
                            @room.Name
                        </div>
                        <div class="card-body">
                            <p><strong>Campus:</strong> @room.CampusName</p>
                            <p><strong>Type:</strong> @room.TypeDisplay</p>
                            <p><strong>Capacity:</strong> @room.Capacity</p>
                        </div>
                        <div class="card-footer">
                            <a asp-page="Details" asp-route-id="@room.Id" 
                               class="btn btn-sm btn-info">
                                View Details
                            </a>
                        </div>
                    </div>
                </div>
            }
        }
        else
        {
            <div class="col-12 text-center py-5">
                <i class="bi bi-inbox display-1 text-muted"></i>
                <h4 class="text-muted mt-3">No rooms found</h4>
            </div>
        }
    </div>

    <!-- Enhanced pagination with page info -->
    @if (Model.Rooms.TotalPages > 0)
    {
        <div class="row">
            <div class="col-md-6">
                <p class="text-muted">
                    Page @Model.Rooms.CurrentPage of @Model.Rooms.TotalPages
                    (Total: @Model.Rooms.TotalCount items)
                </p>
            </div>
            <div class="col-md-6">
                <nav>
                    <ul class="pagination justify-content-end">
                        <!-- First page -->
                        <li class="page-item @(Model.Rooms.CurrentPage == 1 ? "disabled" : "")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="1"
                               asp-route-search="@Model.SearchTerm">
                                <i class="bi bi-chevron-double-left"></i>
                            </a>
                        </li>
                        
                        <!-- Previous -->
                        <li class="page-item @(Model.Rooms.HasPrevious ? "" : "disabled")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="@(Model.Rooms.CurrentPage - 1)"
                               asp-route-search="@Model.SearchTerm">
                                <i class="bi bi-chevron-left"></i>
                            </a>
                        </li>
                        
                        <!-- Page numbers (show 5 pages around current) -->
                        @{
                            var startPage = Math.Max(1, Model.Rooms.CurrentPage - 2);
                            var endPage = Math.Min(Model.Rooms.TotalPages, Model.Rooms.CurrentPage + 2);
                        }
                        
                        @if (startPage > 1)
                        {
                            <li class="page-item disabled">
                                <span class="page-link">...</span>
                            </li>
                        }
                        
                        @for (int i = startPage; i <= endPage; i++)
                        {
                            <li class="page-item @(i == Model.Rooms.CurrentPage ? "active" : "")">
                                <a class="page-link" 
                                   asp-page="Index" 
                                   asp-route-pageNumber="@i"
                                   asp-route-search="@Model.SearchTerm">
                                    @i
                                </a>
                            </li>
                        }
                        
                        @if (endPage < Model.Rooms.TotalPages)
                        {
                            <li class="page-item disabled">
                                <span class="page-link">...</span>
                            </li>
                        }
                        
                        <!-- Next -->
                        <li class="page-item @(Model.Rooms.HasNext ? "" : "disabled")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)"
                               asp-route-search="@Model.SearchTerm">
                                <i class="bi bi-chevron-right"></i>
                            </a>
                        </li>
                        
                        <!-- Last page -->
                        <li class="page-item @(Model.Rooms.CurrentPage == Model.Rooms.TotalPages ? "disabled" : "")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="@Model.Rooms.TotalPages"
                               asp-route-search="@Model.SearchTerm">
                                <i class="bi bi-chevron-double-right"></i>
                            </a>
                        </li>
                    </ul>
                </nav>
            </div>
        </div>
    }
</div>
```

### Example 5: API Controller with Pagination

```csharp
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    
    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }
    
    [HttpGet]
    public async Task<ActionResult<Pagination<RoomDto>>> GetRooms(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? campusId = null)
    {
        if (pageSize > 100)
        {
            return BadRequest("Page size cannot exceed 100");
        }
        
        Pagination<RoomDto> rooms;
        
        if (!string.IsNullOrEmpty(search))
        {
            rooms = await _roomService.SearchRoomsAsync(search, pageNumber, pageSize);
        }
        else if (campusId.HasValue)
        {
            rooms = await _roomService.GetRoomsByCampusAsync(
                campusId.Value, pageNumber, pageSize);
        }
        else
        {
            rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
        }
        
        // Add pagination headers
        Response.Headers.Add("X-Pagination-CurrentPage", rooms.CurrentPage.ToString());
        Response.Headers.Add("X-Pagination-TotalPages", rooms.TotalPages.ToString());
        Response.Headers.Add("X-Pagination-PageSize", rooms.PageSize.ToString());
        Response.Headers.Add("X-Pagination-TotalCount", rooms.TotalCount.ToString());
        
        return Ok(rooms);
    }
}
```

## Performance Considerations

### Database Efficiency
The paginated methods use `Skip` and `Take` which translates to efficient SQL queries:

```sql
-- Non-paginated (loads all)
SELECT * FROM Rooms WHERE IsDeleted = false;

-- Paginated (loads only requested page)
SELECT * FROM Rooms 
WHERE IsDeleted = false 
ORDER BY CreatedAt DESC
OFFSET 20 ROWS       -- Skip (pageNumber - 1) * pageSize
FETCH NEXT 10 ROWS ONLY;  -- Take pageSize
```

### When to Use Pagination vs Full List

**Use Pagination when:**
- Displaying data in UI (tables, grids)
- Building APIs for frontend consumption
- Dealing with large datasets (> 100 items)
- Implementing infinite scroll or load more functionality

**Use Full List when:**
- Building dropdown/select options (small datasets)
- Exporting data
- Internal calculations requiring all data
- Dashboard statistics

## Typical Page Sizes

| Use Case | Recommended Page Size |
|----------|----------------------|
| Mobile view | 5-10 items |
| Desktop table view | 10-25 items |
| Desktop grid view | 12-24 items (divisible by 3 or 4) |
| API responses | 20-50 items |
| Admin panels | 25-50 items |

## Backward Compatibility

All non-paginated methods still exist and work as before:

```csharp
// Old code - still works
var allRooms = await _roomService.GetAllRoomsAsync();

// New code - with pagination
var pagedRooms = await _roomService.GetAllRoomsAsync(pageNumber: 1, pageSize: 10);
```

## Tips & Best Practices

### 1. **Default Page Size**
Always provide a sensible default page size:

```csharp
public async Task OnGetAsync(int pageNumber = 1, int pageSize = 20)
{
    // pageSize defaults to 20 if not provided
}
```

### 2. **Validate Page Numbers**
Ensure page numbers are valid:

```csharp
if (pageNumber < 1) pageNumber = 1;
if (pageSize < 1) pageSize = 10;
if (pageSize > 100) pageSize = 100; // Limit max page size
```

### 3. **Preserve Filter State**
When paginating, preserve filter parameters:

```html
<a asp-page="Index" 
   asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)"
   asp-route-search="@Model.SearchTerm"
   asp-route-campusId="@Model.CampusId"
   asp-route-type="@Model.FilterType">
    Next
</a>
```

### 4. **Show Pagination Info**
Always show users where they are:

```html
<p>Showing @Model.Rooms.Count of @Model.Rooms.TotalCount items</p>
```

### 5. **Handle Empty Results**
Gracefully handle empty pages:

```csharp
if (rooms.TotalCount == 0)
{
    // Show "No results found" message
}
```

## Migration Guide

To update existing Razor Pages to use pagination:

### Before:
```csharp
public List<RoomDto> Rooms { get; set; }

public async Task OnGetAsync()
{
    Rooms = await _roomService.GetAllRoomsAsync();
}
```

### After:
```csharp
public Pagination<RoomDto> Rooms { get; set; }

public async Task OnGetAsync(int pageNumber = 1)
{
    const int pageSize = 20;
    Rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
}
```

### View Update:
```html
<!-- Before -->
@foreach (var room in Model.Rooms)

<!-- After (no change needed!) -->
@foreach (var room in Model.Rooms)
```

Pagination inherits from List<T>, so existing foreach loops work without changes!

## Summary

? All RoomService list methods now have paginated versions  
? Backward compatible - old methods still work  
? Efficient database queries using Skip/Take  
? Pagination class provides navigation helpers  
? Easy to integrate into Razor Pages  
? Suitable for both UI and API scenarios  

The pagination implementation is production-ready and follows .NET best practices!
