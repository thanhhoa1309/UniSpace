# H??ng D?n S? D?ng Schedule Service

## T?ng Quan

Service Schedule ?ã ???c implement v?i ??y ?? ch?c n?ng CRUD cho admin qu?n lý l?ch c?a t?ng phòng, bao g?m:

? **Yêu c?u 1: Schedule không ???c trùng nhau**
- Service t? ??ng ki?m tra xung ??t gi?a các schedule
- Ki?m tra c? ph?m vi ngày và gi? trong ngày
- Ch? so sánh các schedule cùng phòng và cùng ngày trong tu?n

? **Yêu c?u 2: Có kho?ng th?i gian ngh? gi?a các schedule**
- Th?i gian ngh? m?c ??nh: 15 phút (có th? tùy ch?nh)
- T? ??ng ki?m tra khi t?o schedule m?i
- Ví d?: Schedule A k?t thúc lúc 11:00, Schedule B ph?i b?t ??u sau 11:15

? **Yêu c?u 3: Có th? b? sung thêm**
- Service ?ã ???c thi?t k? m? r?ng d? dàng
- Xem ph?n "Future Enhancements" trong documentation

## Các File ?ã T?o

### 1. DTOs (Data Transfer Objects)
```
UniSpace.BusinessObject/DTOs/ScheduleDTOs/
??? ScheduleDto.cs           (Response DTO)
??? CreateScheduleDto.cs     (Create DTO)
??? UpdateScheduleDto.cs     (Update DTO)
```

### 2. Service Interface & Implementation
```
UniSpace.Service/
??? Interfaces/IScheduleService.cs
??? Services/ScheduleService.cs
```

### 3. C?p Nh?t UnitOfWork
```
UniSpace.Domain/
??? Interfaces/IUnitOfWork.cs  (?ã thêm Schedule repository)
??? UnitOfWork.cs              (?ã implement Schedule repository)
```

### 4. ??ng Ký DI Container
```
UniSpace.Presentation/Architecture/IocContainer.cs
(?ã ??ng ký IScheduleService)
```

## Cách S? D?ng

### 1. T?o Schedule M?i (Admin)

```csharp
// Inject service trong constructor
private readonly IScheduleService _scheduleService;

public YourController(IScheduleService scheduleService)
{
    _scheduleService = scheduleService;
}

// T?o schedule cho môn h?c
var createDto = new CreateScheduleDto
{
    RoomId = roomId,
    ScheduleType = ScheduleType.Academic_Course,
    Title = "L?p trình nâng cao - PRN222",
    StartTime = new TimeSpan(7, 30, 0),    // 7:30 sáng
    EndTime = new TimeSpan(9, 30, 0),      // 9:30 sáng
    DayOfWeek = 1,                          // Th? 2 (0=CN, 1=T2, ..., 6=T7)
    StartDate = new DateTime(2024, 9, 1),   // Ngày b?t ??u h?c k?
    EndDate = new DateTime(2024, 12, 31),   // Ngày k?t thúc h?c k?
    BreakTimeMinutes = 15                   // Ngh? 15 phút gi?a các ti?t
};

try
{
    var result = await _scheduleService.CreateScheduleAsync(createDto);
    // Success: result ch?a thông tin schedule ?ã t?o
}
catch (Exception ex)
{
    // L?i: ex.Message s? cho bi?t lý do (conflict, validation, etc.)
}
```

### 2. T?o Schedule B?o Trì

```csharp
var maintenanceDto = new CreateScheduleDto
{
    RoomId = roomId,
    ScheduleType = ScheduleType.Recurring_Maintenance,
    Title = "B?o trì phòng Lab ??nh k?",
    StartTime = new TimeSpan(18, 0, 0),    // 6 gi? chi?u
    EndTime = new TimeSpan(20, 0, 0),      // 8 gi? t?i
    DayOfWeek = 5,                          // Th? 6
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 12, 31),
    BreakTimeMinutes = 30                   // B?o trì c?n th?i gian ngh? lâu h?n
};

var result = await _scheduleService.CreateScheduleAsync(maintenanceDto);
```

### 3. L?y Danh Sách Schedule

```csharp
// L?y t?t c? schedule c?a 1 phòng
var roomSchedules = await _scheduleService.GetSchedulesByRoomAsync(roomId);

// L?y schedule theo th? trong tu?n
var mondaySchedules = await _scheduleService.GetSchedulesByDayOfWeekAsync(1); // Th? 2

// L?y schedule c?a phòng vào 1 ngày c? th?
var todaySchedules = await _scheduleService.GetSchedulesForRoomOnDateAsync(
    roomId, 
    DateTime.Today
);

// L?y schedule trong kho?ng th?i gian
var semesterSchedules = await _scheduleService.GetSchedulesByDateRangeAsync(
    new DateTime(2024, 9, 1),
    new DateTime(2024, 12, 31)
);
```

### 4. C?p Nh?t Schedule

```csharp
var updateDto = new UpdateScheduleDto
{
    Id = scheduleId,
    RoomId = roomId,
    ScheduleType = ScheduleType.Academic_Course,
    Title = "L?p trình nâng cao - PRN222 (C?p nh?t)",
    StartTime = new TimeSpan(8, 0, 0),     // ??i gi?
    EndTime = new TimeSpan(10, 0, 0),
    DayOfWeek = 1,
    StartDate = new DateTime(2024, 9, 1),
    EndDate = new DateTime(2024, 12, 31)
};

var updated = await _scheduleService.UpdateScheduleAsync(updateDto);
```

### 5. Xóa Schedule

```csharp
// Soft delete (không xóa th?t, ch? ?ánh d?u IsDeleted = true)
var success = await _scheduleService.SoftDeleteScheduleAsync(scheduleId);

// Hard delete (xóa h?n kh?i database) - CHÚ Ý: C?n th?n khi dùng
var deleted = await _scheduleService.DeleteScheduleAsync(scheduleId);
```

### 6. Ki?m Tra Xung ??t Tr??c Khi T?o

```csharp
// Ki?m tra có xung ??t không
var hasConflict = await _scheduleService.HasScheduleConflictAsync(
    roomId: roomId,
    dayOfWeek: 1,
    startTime: new TimeSpan(9, 0, 0),
    endTime: new TimeSpan(11, 0, 0),
    startDate: new DateTime(2024, 9, 1),
    endDate: new DateTime(2024, 12, 31)
);

if (hasConflict)
{
    // L?y danh sách các schedule b? xung ??t
    var conflicts = await _scheduleService.GetConflictingSchedulesAsync(
        roomId,
        dayOfWeek: 1,
        startTime: new TimeSpan(9, 0, 0),
        endTime: new TimeSpan(11, 0, 0),
        startDate: new DateTime(2024, 9, 1),
        endDate: new DateTime(2024, 12, 31)
    );
    
    // Hi?n th? cho user bi?t schedule nào b? trùng
    foreach (var conflict in conflicts)
    {
        Console.WriteLine($"Trùng v?i: {conflict.Title} ({conflict.StartTime} - {conflict.EndTime})");
    }
}
```

## Các Tr??ng H?p Xung ??t

### Tr??ng H?p 1: Trùng Gi? Cùng Ngày
```
Schedule A: T2, 9:00-11:00, t? 01/09/2024 ??n 31/12/2024
Schedule B: T2, 10:00-12:00, t? 01/09/2024 ??n 31/12/2024
? XUNG ??T: Trùng t? 10:00-11:00
```

### Tr??ng H?p 2: Không ?? Th?i Gian Ngh?
```
Schedule A: T2, 9:00-11:00, BreakTime = 15 phút
Schedule B: T2, 11:10-13:00
? XUNG ??T: Ch? ngh? 10 phút, c?n 15 phút
```

### Tr??ng H?p 3: OK - Khác Ngày
```
Schedule A: T2, 9:00-11:00
Schedule B: T3, 9:00-11:00
? OK: Khác ngày trong tu?n
```

### Tr??ng H?p 4: OK - Khác K?
```
Schedule A: 9:00-11:00, t? 01/09/2024 ??n 31/12/2024 (K? 1)
Schedule B: 9:00-11:00, t? 01/01/2025 ??n 31/05/2025 (K? 2)
? OK: Không trùng th?i gian
```

### Tr??ng H?p 5: OK - ?? Th?i Gian Ngh?
```
Schedule A: T2, 9:00-11:00, BreakTime = 15 phút
Schedule B: T2, 11:15-13:00
? OK: Ngh? ?? 15 phút
```

## X? Lý L?i

### 1. L?i Xung ??t (409 Conflict)
```
Message: "Schedule conflicts with existing schedule(s): 
         L?p trình c? s? (09:00 - 11:00). 
         Please ensure there is at least 15 minutes break time between schedules."
```

**Cách x? lý:**
- ??i th?i gian schedule
- ??i ngày trong tu?n
- T?ng th?i gian ngh? gi?a các schedule

### 2. L?i Validation (400 Bad Request)
```
Message: "Start time must be before end time"
```

**Cách x? lý:**
- Ki?m tra l?i input
- ??m b?o StartTime < EndTime
- ??m b?o StartDate < EndDate

### 3. L?i Not Found (404)
```
Message: "Room with ID 'xxx' not found"
```

**Cách x? lý:**
- Ki?m tra RoomId có t?n t?i không
- Ki?m tra phòng có b? xóa (soft delete) không

## Ví D? Razor Page (Admin)

### Index.cshtml.cs - Hi?n th? danh sách schedule

```csharp
public class IndexModel : PageModel
{
    private readonly IScheduleService _scheduleService;

    public IndexModel(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    public List<ScheduleDto> Schedules { get; set; }
    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? roomId)
    {
        try
        {
            if (roomId.HasValue)
            {
                Schedules = await _scheduleService.GetSchedulesByRoomAsync(roomId.Value);
            }
            else
            {
                Schedules = await _scheduleService.GetAllSchedulesAsync();
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
```

### Create.cshtml.cs - T?o schedule m?i

```csharp
public class CreateModel : PageModel
{
    private readonly IScheduleService _scheduleService;
    private readonly IRoomService _roomService;

    [BindProperty]
    public CreateScheduleDto Input { get; set; }
    
    public List<SelectListItem> RoomOptions { get; set; }
    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Load danh sách phòng
        var rooms = await _roomService.GetAllRoomsAsync();
        RoomOptions = rooms.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = $"{r.Name} - {r.CampusName}"
        }).ToList();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            var result = await _scheduleService.CreateScheduleAsync(Input);
            return RedirectToPage("Details", new { id = result.Id });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await OnGetAsync();
            return Page();
        }
    }
}
```

## Tips & Best Practices

### 1. Validation Phía Client
```javascript
// Ki?m tra th?i gian tr??c khi submit
function validateScheduleTime() {
    const startTime = document.getElementById('startTime').value;
    const endTime = document.getElementById('endTime').value;
    
    if (startTime >= endTime) {
        alert('Th?i gian b?t ??u ph?i tr??c th?i gian k?t thúc');
        return false;
    }
    return true;
}
```

### 2. Hi?n th? Schedule Trên Calendar
```javascript
// S? d?ng FullCalendar ho?c t??ng t?
var calendar = new FullCalendar.Calendar(calendarEl, {
    events: '/api/schedules/room/' + roomId,
    eventClick: function(info) {
        // Show schedule details
    }
});
```

### 3. Color Coding
```csharp
// Trong view, tô màu theo lo?i schedule
@if (schedule.ScheduleType == ScheduleType.Academic_Course)
{
    <span class="badge bg-primary">Môn h?c</span>
}
else
{
    <span class="badge bg-warning">B?o trì</span>
}
```

## K? Ho?ch M? R?ng (Optional)

N?u mu?n thêm tính n?ng, có th? implement:

1. **Bulk Create**: T?o nhi?u schedule cùng lúc (Copy schedule t? k? này sang k? khác)
2. **Templates**: T?o template schedule và apply cho nhi?u phòng
3. **Export**: Xu?t schedule ra Excel/PDF
4. **Notifications**: Thông báo khi schedule thay ??i
5. **Statistics**: Th?ng kê s? d?ng phòng, schedule ph? bi?n nh?t, etc.

## T?ng K?t

Service Schedule ?ã hoàn thi?n v?i:
- ? CRUD ??y ??
- ? Ki?m tra xung ??t thông minh
- ? Qu?n lý th?i gian ngh?
- ? Nhi?u ph??ng th?c query
- ? Error handling chi ti?t
- ? Audit trail ??y ??

B?n có th? b?t ??u s? d?ng ngay trong Razor Pages ho?c API Controllers!
