# H??ng D?n S? D?ng Pagination - RoomService

## T?ng Quan

RoomService ?ã ???c c?p nh?t ?? h? tr? phân trang (pagination) cho t?t c? các ph??ng th?c tr? v? danh sách. ?i?u này giúp t?i d? li?u hi?u qu? h?n khi có nhi?u b?n ghi.

## Thay ??i

### ? File M?i T?o:
1. `UniSpace.Sevice/Utils/PaginationExtensions.cs` - Các ph??ng th?c helper
2. `ROOM_SERVICE_PAGINATION_GUIDE.md` - Tài li?u chi ti?t (ti?ng Anh)
3. `ROOM_PAGINATION_SUMMARY.md` - Tóm t?t implementation

### ? File ?ã C?p Nh?t:
1. `UniSpace.Sevice/Interfaces/IRoomService.cs` - Thêm method signature
2. `UniSpace.Sevice/Services/RoomService.cs` - Implement pagination

## Các Ph??ng Th?c M?i

M?i ph??ng th?c tr? v? `List` ??u có phiên b?n phân trang:

### Ph??ng th?c g?c (v?n ho?t ??ng):
```csharp
Task<List<RoomDto>> GetAllRoomsAsync()
Task<List<RoomDto>> GetRoomsByCampusAsync(Guid campusId)
Task<List<RoomDto>> GetRoomsByTypeAsync(RoomType type)
Task<List<RoomDto>> GetRoomsByStatusAsync(BookingStatus status)
Task<List<RoomDto>> SearchRoomsAsync(string searchTerm)
Task<List<RoomDto>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime)
```

### Ph??ng th?c m?i (có phân trang):
```csharp
Task<Pagination<RoomDto>> GetAllRoomsAsync(int pageNumber, int pageSize)
Task<Pagination<RoomDto>> GetRoomsByCampusAsync(Guid campusId, int pageNumber, int pageSize)
Task<Pagination<RoomDto>> GetRoomsByTypeAsync(RoomType type, int pageNumber, int pageSize)
Task<Pagination<RoomDto>> GetRoomsByStatusAsync(BookingStatus status, int pageNumber, int pageSize)
Task<Pagination<RoomDto>> SearchRoomsAsync(string searchTerm, int pageNumber, int pageSize)
Task<Pagination<RoomDto>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime, int pageNumber, int pageSize)
```

## Pagination Class

Class `Pagination<T>` có s?n t?i `UniSpace.Services.Utils.Pagination<T>`:

```csharp
public class Pagination<T> : List<T>
{
    public int CurrentPage { get; }      // Trang hi?n t?i
    public int TotalPages { get; }       // T?ng s? trang
    public int PageSize { get; }         // S? item m?i trang
    public int TotalCount { get; }       // T?ng s? item trong DB
    public bool HasPrevious { get; }     // Có trang tr??c không?
    public bool HasNext { get; }         // Có trang sau không?
}
```

## Cách S? D?ng

### Ví D? 1: Phân Trang ??n Gi?n

```csharp
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    
    // Thay ??i t? List<RoomDto> thành Pagination<RoomDto>
    public Pagination<RoomDto> Rooms { get; set; }
    
    public async Task OnGetAsync(int pageNumber = 1)
    {
        const int pageSize = 20; // 20 phòng m?i trang
        
        Rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
    }
}
```

### Ví D? 2: Phân Trang V?i Tìm Ki?m

```csharp
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;
    
    public Pagination<RoomDto> Rooms { get; set; }
    public string SearchTerm { get; set; }
    
    public async Task OnGetAsync(int pageNumber = 1, string? search = null)
    {
        const int pageSize = 20;
        SearchTerm = search;
        
        if (!string.IsNullOrEmpty(search))
        {
            // Tìm ki?m có phân trang
            Rooms = await _roomService.SearchRoomsAsync(search, pageNumber, pageSize);
        }
        else
        {
            // L?y t?t c? có phân trang
            Rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
        }
    }
}
```

### Ví D? 3: Hi?n Th? Phân Trang Trong View

```html
@model IndexModel

<!-- Hi?n th? danh sách phòng -->
<div class="row">
    @foreach (var room in Model.Rooms)
    {
        <div class="col-md-4">
            <div class="card mb-3">
                <div class="card-header bg-primary text-white">
                    @room.Name
                </div>
                <div class="card-body">
                    <p><strong>Campus:</strong> @room.CampusName</p>
                    <p><strong>Lo?i:</strong> @room.TypeDisplay</p>
                    <p><strong>S?c ch?a:</strong> @room.Capacity</p>
                </div>
                <div class="card-footer">
                    <a asp-page="Details" asp-route-id="@room.Id" 
                       class="btn btn-sm btn-info">
                        Xem chi ti?t
                    </a>
                </div>
            </div>
        </div>
    }
</div>

<!-- ?i?u khi?n phân trang -->
<nav aria-label="Room pagination">
    <ul class="pagination justify-content-center">
        <!-- Nút Previous -->
        <li class="page-item @(Model.Rooms.HasPrevious ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage - 1)"
               asp-route-search="@Model.SearchTerm">
                « Tr??c
            </a>
        </li>
        
        <!-- S? trang -->
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
        
        <!-- Nút Next -->
        <li class="page-item @(Model.Rooms.HasNext ? "" : "disabled")">
            <a class="page-link" 
               asp-page="Index" 
               asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)"
               asp-route-search="@Model.SearchTerm">
                Sau »
            </a>
        </li>
    </ul>
</nav>

<!-- Thông tin phân trang -->
<div class="text-center text-muted mt-3">
    <small>
        Hi?n th? @((Model.Rooms.CurrentPage - 1) * Model.Rooms.PageSize + 1) 
        ??n @(Math.Min(Model.Rooms.CurrentPage * Model.Rooms.PageSize, Model.Rooms.TotalCount))
        trong t?ng s? @Model.Rooms.TotalCount phòng
    </small>
</div>
```

### Ví D? 4: Phân Trang Nâng Cao

```html
@model IndexModel

<div class="container">
    <!-- Hi?n th? rooms -->
    <div class="row mb-4">
        @if (Model.Rooms.Any())
        {
            @foreach (var room in Model.Rooms)
            {
                <div class="col-md-4 mb-3">
                    <div class="card h-100">
                        <div class="card-header bg-primary text-white">
                            <h6 class="mb-0">@room.Name</h6>
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
        }
        else
        {
            <div class="col-12 text-center py-5">
                <i class="bi bi-inbox display-1 text-muted"></i>
                <h4 class="text-muted mt-3">Không tìm th?y phòng nào</h4>
            </div>
        }
    </div>

    <!-- Phân trang nâng cao -->
    @if (Model.Rooms.TotalPages > 0)
    {
        <div class="row align-items-center">
            <div class="col-md-6">
                <p class="text-muted mb-0">
                    Trang @Model.Rooms.CurrentPage / @Model.Rooms.TotalPages
                    (T?ng: @Model.Rooms.TotalCount phòng)
                </p>
            </div>
            <div class="col-md-6">
                <nav>
                    <ul class="pagination justify-content-end mb-0">
                        <!-- Trang ??u -->
                        <li class="page-item @(Model.Rooms.CurrentPage == 1 ? "disabled" : "")">
                            <a class="page-link" asp-page="Index" asp-route-pageNumber="1">
                                <i class="bi bi-chevron-double-left"></i>
                            </a>
                        </li>
                        
                        <!-- Trang tr??c -->
                        <li class="page-item @(Model.Rooms.HasPrevious ? "" : "disabled")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="@(Model.Rooms.CurrentPage - 1)">
                                <i class="bi bi-chevron-left"></i>
                            </a>
                        </li>
                        
                        <!-- Hi?n th? 5 trang xung quanh trang hi?n t?i -->
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
                                <a class="page-link" asp-page="Index" asp-route-pageNumber="@i">
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
                        
                        <!-- Trang sau -->
                        <li class="page-item @(Model.Rooms.HasNext ? "" : "disabled")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)">
                                <i class="bi bi-chevron-right"></i>
                            </a>
                        </li>
                        
                        <!-- Trang cu?i -->
                        <li class="page-item @(Model.Rooms.CurrentPage == Model.Rooms.TotalPages ? "disabled" : "")">
                            <a class="page-link" 
                               asp-page="Index" 
                               asp-route-pageNumber="@Model.Rooms.TotalPages">
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

## Hi?u Su?t

### Tr??c Khi Có Pagination:
- Load **t?t c?** phòng t? database (ví d?: 1000 phòng)
- Tiêu t?n nhi?u b? nh? (~1MB)
- Ch?m khi có nhi?u d? li?u

### Sau Khi Có Pagination:
- Ch? load **s? l??ng c?n thi?t** (ví d?: 20 phòng)
- Ti?t ki?m b? nh? (~20KB)
- Nhanh và ?n ??nh

### SQL Query So Sánh:

**Không có pagination:**
```sql
SELECT * FROM Rooms WHERE IsDeleted = false;
-- L?y T?T C? (có th? hàng nghìn b?n ghi)
```

**Có pagination:**
```sql
SELECT * FROM Rooms 
WHERE IsDeleted = false 
ORDER BY CreatedAt DESC
OFFSET 20 ROWS          -- B? qua 20 b?n ghi ??u
FETCH NEXT 10 ROWS ONLY; -- Ch? l?y 10 b?n ghi
-- Ch? l?y ?úng s? l??ng c?n thi?t
```

## Kích Th??c Trang ?? Xu?t

| Tr??ng H?p | S? L??ng |
|------------|----------|
| Mobile | 5-10 |
| Desktop (Table) | 10-25 |
| Desktop (Grid 3 c?t) | 12, 24 |
| Desktop (Grid 4 c?t) | 12, 20, 24 |
| API | 20-50 |
| Admin Panel | 25-50 |

## T??ng Thích Ng??c

**Quan tr?ng:** T?t c? code c? v?n ho?t ??ng bình th??ng!

```csharp
// Code c? - v?n OK
List<RoomDto> rooms = await _roomService.GetAllRoomsAsync();

// Code m?i - có phân trang
Pagination<RoomDto> pagedRooms = await _roomService.GetAllRoomsAsync(1, 20);
```

Vì `Pagination<T>` k? th?a t? `List<T>`, nên code foreach v?n ho?t ??ng:

```csharp
// Ho?t ??ng v?i c? List<RoomDto> và Pagination<RoomDto>
foreach (var room in rooms)
{
    // X? lý room
}
```

## Tips H?u Ích

### 1. Luôn có giá tr? m?c ??nh
```csharp
public async Task OnGetAsync(int pageNumber = 1, int pageSize = 20)
{
    // pageSize m?c ??nh là 20 n?u không ???c cung c?p
}
```

### 2. Validate input
```csharp
if (pageNumber < 1) pageNumber = 1;
if (pageSize < 1) pageSize = 10;
if (pageSize > 100) pageSize = 100; // Gi?i h?n t?i ?a
```

### 3. Gi? tr?ng thái filter
```html
<a asp-page="Index" 
   asp-route-pageNumber="@(Model.Rooms.CurrentPage + 1)"
   asp-route-search="@Model.SearchTerm"
   asp-route-campusId="@Model.CampusId">
    Trang sau
</a>
```

### 4. Hi?n th? thông tin cho user
```html
<p>
    ?ang hi?n th? @Model.Rooms.Count 
    trong t?ng s? @Model.Rooms.TotalCount phòng
</p>
```

## Migration Guide - C?p Nh?t Code C?

### Tr??c:
```csharp
public List<RoomDto> Rooms { get; set; }

public async Task OnGetAsync()
{
    Rooms = await _roomService.GetAllRoomsAsync();
}
```

### Sau:
```csharp
public Pagination<RoomDto> Rooms { get; set; }

public async Task OnGetAsync(int pageNumber = 1)
{
    const int pageSize = 20;
    Rooms = await _roomService.GetAllRoomsAsync(pageNumber, pageSize);
}
```

### View:
```html
<!-- Không c?n thay ??i gì! -->
@foreach (var room in Model.Rooms)
{
    <!-- Code c? v?n ho?t ??ng -->
}
```

## T?ng K?t

? T?t c? ph??ng th?c RoomService ??u có phiên b?n phân trang  
? T??ng thích ng??c - code c? v?n ho?t ??ng  
? Hi?u su?t t?t h?n v?i database queries  
? D? dàng tích h?p vào Razor Pages  
? S?n sàng cho production  

B?n có th? b?t ??u s? d?ng ngay b?ng cách thay ??i:
```csharp
List<RoomDto> ? Pagination<RoomDto>
```

Và thêm tham s? `pageNumber` vào method OnGet!

## Xem Thêm

- `ROOM_SERVICE_PAGINATION_GUIDE.md` - Tài li?u chi ti?t (ti?ng Anh)
- `ROOM_PAGINATION_SUMMARY.md` - Tóm t?t k? thu?t
