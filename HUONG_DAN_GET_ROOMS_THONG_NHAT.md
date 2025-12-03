# GetRoomsAsync - Hàm Th?ng Nh?t Tìm Ki?m Rooms

## ? Hoàn Thành - G?p T?t C? Hàm Get Thành M?t

### T?ng Quan

?ã g?p **6 hàm get riêng bi?t** thành **1 hàm duy nh?t** v?i nhi?u filter parameters.

## ?? So Sánh

### ? Tr??c (6 Hàm):

```csharp
GetAllRoomsAsync(pageNumber, pageSize)
GetRoomsByCampusAsync(campusId, pageNumber, pageSize)
GetRoomsByTypeAsync(type, pageNumber, pageSize)
GetRoomsByStatusAsync(status, pageNumber, pageSize)
SearchRoomsAsync(searchTerm, pageNumber, pageSize)
GetAvailableRoomsAsync(startTime, endTime, pageNumber, pageSize)
```

### ? Sau (1 Hàm):

```csharp
GetRoomsAsync(
    pageNumber = 1,
    pageSize = 20,
    searchTerm = null,      // Tìm ki?m text
    campusId = null,        // L?c theo campus
    type = null,            // L?c theo lo?i phòng
    status = null,          // L?c theo tr?ng thái
    availableFrom = null,   // Ki?m tra available t?
    availableTo = null      // Ki?m tra available ??n
)
```

## ?? Signature Chi Ti?t

```csharp
/// <summary>
/// L?y danh sách rooms v?i filter linh ho?t
/// </summary>
/// <param name="pageNumber">S? trang (m?c ??nh: 1)</param>
/// <param name="pageSize">S? l??ng items m?i trang (m?c ??nh: 20)</param>
/// <param name="searchTerm">Tìm ki?m trong tên room, mô t?, ho?c tên campus</param>
/// <param name="campusId">L?c theo campus c? th?</param>
/// <param name="type">L?c theo lo?i phòng (Classroom, Lab, Stadium)</param>
/// <param name="status">L?c theo tr?ng thái (Approved, Rejected...)</param>
/// <param name="availableFrom">Th?i gian b?t ??u ki?m tra available</param>
/// <param name="availableTo">Th?i gian k?t thúc ki?m tra available</param>
/// <returns>Danh sách phân trang các rooms phù h?p v?i filters</returns>
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

## ?? Cách S? D?ng

### 1. L?y T?t C? (Không Filter)

```csharp
// ??n gi?n nh?t - trang 1, 20 items
var rooms = await _roomService.GetRoomsAsync();

// Tùy ch?nh trang và size
var rooms = await _roomService.GetRoomsAsync(pageNumber: 2, pageSize: 50);
```

### 2. Tìm Ki?m Text

```csharp
// Tìm trong tên phòng, mô t?, ho?c tên campus
var rooms = await _roomService.GetRoomsAsync(searchTerm: "Lab");
var rooms = await _roomService.GetRoomsAsync(searchTerm: "Computer");
```

### 3. L?c Theo Campus

```csharp
var campusId = Guid.Parse("...");
var rooms = await _roomService.GetRoomsAsync(campusId: campusId);
```

### 4. L?c Theo Lo?i Phòng

```csharp
// Ch? l?y Classrooms
var classrooms = await _roomService.GetRoomsAsync(type: RoomType.Classroom);

// Ch? l?y Labs
var labs = await _roomService.GetRoomsAsync(type: RoomType.Lab);

// Ch? l?y Stadiums
var stadiums = await _roomService.GetRoomsAsync(type: RoomType.Stadium);
```

### 5. L?c Theo Tr?ng Thái

```csharp
// Ch? l?y phòng available
var available = await _roomService.GetRoomsAsync(status: BookingStatus.Approved);

// Ch? l?y phòng unavailable
var unavailable = await _roomService.GetRoomsAsync(status: BookingStatus.Rejected);
```

### 6. K?t H?p Nhi?u Filters

```csharp
// Tìm Classrooms trong Campus c? th?
var rooms = await _roomService.GetRoomsAsync(
    campusId: campusId,
    type: RoomType.Classroom);

// Tìm Lab rooms available
var rooms = await _roomService.GetRoomsAsync(
    type: RoomType.Lab,
    status: BookingStatus.Approved);

// Tìm ki?m + l?c campus
var rooms = await _roomService.GetRoomsAsync(
    searchTerm: "Computer",
    campusId: campusId);

// Ba filters cùng lúc
var rooms = await _roomService.GetRoomsAsync(
    searchTerm: "Lab",
    campusId: campusId,
    type: RoomType.Lab);
```

### 7. L?c Theo Th?i Gian Available

```csharp
var startTime = DateTime.Now.AddHours(1);
var endTime = DateTime.Now.AddHours(3);

// Ch? l?y rooms tr?ng trong kho?ng th?i gian
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

### 8. Full Filters

```csharp
// S? d?ng t?t c? filters!
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

### PageModel

```csharp
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    
    public Pagination<RoomDto> Rooms { get; set; }
    
    // Properties cho filters
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
        // L?u filter values
        SearchTerm = search;
        CampusId = campusId;
        Type = type;
        Status = status;
        
        // M?t l?i g?i v?i t?t c? filters!
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

### View (Cshtml)

```html
<form method="get" class="row g-3">
    <!-- Tìm ki?m -->
    <div class="col-md-4">
        <input type="text" name="search" class="form-control" 
               placeholder="Tìm ki?m phòng..." 
               value="@Model.SearchTerm" />
    </div>
    
    <!-- Campus -->
    <div class="col-md-3">
        <select name="campusId" class="form-select">
            <option value="">T?t c? Campus</option>
            @foreach (var campus in Model.Campuses)
            {
                <option value="@campus.Id" 
                        selected="@(campus.Id == Model.CampusId)">
                    @campus.Name
                </option>
            }
        </select>
    </div>
    
    <!-- Lo?i phòng -->
    <div class="col-md-2">
        <select name="type" class="form-select">
            <option value="">T?t c? lo?i</option>
            <option value="0" selected="@(Model.Type == RoomType.Classroom)">
                Classroom
            </option>
            <option value="1" selected="@(Model.Type == RoomType.Lab)">
                Lab
            </option>
            <option value="2" selected="@(Model.Type == RoomType.Stadium)">
                Stadium
            </option>
        </select>
    </div>
    
    <!-- Tr?ng thái -->
    <div class="col-md-2">
        <select name="status" class="form-select">
            <option value="">T?t c?</option>
            <option value="1" selected="@(Model.Status == BookingStatus.Approved)">
                Available
            </option>
            <option value="2" selected="@(Model.Status == BookingStatus.Rejected)">
                Unavailable
            </option>
        </select>
    </div>
    
    <!-- Nút tìm -->
    <div class="col-md-1">
        <button type="submit" class="btn btn-primary w-100">
            <i class="bi bi-search"></i>
        </button>
    </div>
</form>

<!-- K?t qu? -->
<div class="row mt-4">
    @foreach (var room in Model.Rooms)
    {
        <div class="col-md-4 mb-3">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    @room.Name
                </div>
                <div class="card-body">
                    <p><i class="bi bi-building"></i> @room.CampusName</p>
                    <p><i class="bi bi-tag"></i> @room.TypeDisplay</p>
                    <p><i class="bi bi-people"></i> @room.Capacity ch?</p>
                    <span class="badge bg-success">@room.CurrentStatusDisplay</span>
                </div>
            </div>
        </div>
    }
</div>

<!-- Pagination -->
<nav>
    <ul class="pagination justify-content-center">
        <li class="page-item @(Model.Rooms.HasPrevious ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage - 1)"
               asp-route-search="@Model.SearchTerm"
               asp-route-campusId="@Model.CampusId"
               asp-route-type="@Model.Type"
               asp-route-status="@Model.Status">
                « Tr??c
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
                Sau »
            </a>
        </li>
    </ul>
</nav>

<!-- Thông tin -->
<p class="text-center text-muted">
    Hi?n th? @Model.Rooms.Count trong t?ng s? @Model.Rooms.TotalCount phòng
    @if (!string.IsNullOrEmpty(Model.SearchTerm) || Model.CampusId.HasValue || 
         Model.Type.HasValue || Model.Status.HasValue)
    {
        <text>(?ã l?c)</text>
    }
</p>
```

## ? ?u ?i?m

### 1. Code S?ch H?n

**Tr??c:**
```csharp
// Ph?i if-else ?? ch?n hàm
if (!string.IsNullOrEmpty(search))
    rooms = await _roomService.SearchRoomsAsync(search, page, size);
else if (campusId.HasValue)
    rooms = await _roomService.GetRoomsByCampusAsync(campusId.Value, page, size);
else if (type.HasValue)
    rooms = await _roomService.GetRoomsByTypeAsync(type.Value, page, size);
else
    rooms = await _roomService.GetAllRoomsAsync(page, size);
```

**Sau:**
```csharp
// M?t hàm, t?t c? filters
rooms = await _roomService.GetRoomsAsync(
    pageNumber: page,
    pageSize: size,
    searchTerm: search,
    campusId: campusId,
    type: type);
```

### 2. Linh Ho?t

- K?t h?p b?t k? filters nào
- Named parameters rõ ràng
- Default values ti?n l?i

### 3. D? Maintain

- Ch? 1 hàm thay vì 6
- Logic t?p trung
- D? thêm filter m?i

### 4. D? Test

**Tr??c:** Ph?i test 6 hàm riêng
**Sau:** Test 1 hàm v?i các combinations

## ?? Logic X? Lý

### Th? T? Apply Filters

1. **Base Query**: Load rooms + includes
2. **Search**: Tìm trong name, description, campus name
3. **Campus**: L?c theo campus ID
4. **Type**: L?c theo lo?i phòng
5. **Status**: L?c theo tr?ng thái
6. **Order**: S?p x?p theo tên
7. **Count**: ??m t?ng s?
8. **Pagination**: Skip và Take
9. **Availability**: Ki?m tra available (sau pagination)

### Performance

#### Fast Filters (Database)
- ? `searchTerm` - SQL LIKE
- ? `campusId` - SQL WHERE
- ? `type` - SQL WHERE
- ? `status` - SQL WHERE

#### Slow Filter (Memory)
- ?? `availableFrom/To` - Ki?m tra t?ng room

**L?u ý:** Filter availability ???c áp d?ng SAU pagination ?? t?ng performance.

## ?? Files ?ã C?p Nh?t

- ? `IRoomService.cs` - Interface
- ? `RoomService.cs` - Implementation
- ? `Room/Index.cshtml.cs` - Razor Page
- ? `Room/Dashboard.cshtml.cs` - Dashboard
- ? Build thành công!

## ?? Tóm T?t

| Tiêu Chí | Tr??c | Sau |
|----------|-------|-----|
| **S? hàm** | 6 hàm riêng | 1 hàm th?ng nh?t |
| **Lines of code** | ~300 dòng | ~100 dòng |
| **Flexibility** | H?n ch? | Không gi?i h?n |
| **Maintenance** | Khó (6 hàm) | D? (1 hàm) |
| **Testing** | 6 test suites | 1 comprehensive suite |

## ?? K?t Lu?n

**Status:** ? HOÀN THÀNH

- G?p thành công 6 hàm thành 1
- Code s?ch và d? maintain h?n
- Linh ho?t h?n trong vi?c k?t h?p filters
- Build thành công, không có l?i

Gi? ?ây ch? c?n nh? **m?t hàm duy nh?t** `GetRoomsAsync` v?i t?t c? các filters! ??
