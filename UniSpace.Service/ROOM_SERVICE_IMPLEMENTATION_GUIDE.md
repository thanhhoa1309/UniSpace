# ?? Room Service - Complete CRUD Implementation

## ? Completed

### **Room CRUD Service**
- Full CRUD operations for Room entity
- Advanced search and filtering
- Room availability checking
- Status management
- Campus integration
- Booking conflict detection

---

## ?? Files Created

```
UniSpace.BusinessObject/DTOs/RoomDTOs/
??? RoomDto.cs              ? Read DTO
??? CreateRoomDto.cs        ? Create DTO
??? UpdateRoomDto.cs        ? Update DTO

UniSpace.Service/
??? Interfaces/
?   ??? IRoomService.cs     ? Service interface (updated)
??? Services/
    ??? RoomService.cs      ? Service implementation (updated)

UniSpace.Domain/
??? Interfaces/
?   ??? IUnitOfWork.cs      ? Added Room repository
??? UnitOfWork.cs           ? Registered Room repository

UniSpace.Presentation/Architecture/
??? IocContainer.cs         ? Registered RoomService
```

---

## ?? Features Implemented

### **1. Create Operations**
```csharp
Task<RoomDto?> CreateRoomAsync(CreateRoomDto createDto);
```
**Features:**
- Campus existence validation
- Room name uniqueness per campus
- Default status: Available (Approved)
- Auto-generates GUID
- Audit fields tracking

**Validations:**
- Campus must exist
- Room name required (max 100 chars)
- Capacity required (1-1000)
- Description optional (max 500 chars)
- Duplicate name check per campus

---

### **2. Read Operations**

#### **Get All Rooms**
```csharp
Task<List<RoomDto>> GetAllRoomsAsync();
```
Returns all active rooms with campus, bookings, and reports.

#### **Get Room by ID**
```csharp
Task<RoomDto?> GetRoomByIdAsync(Guid id);
```
Returns single room with full details or throws NotFound.

#### **Get Rooms by Campus**
```csharp
Task<List<RoomDto>> GetRoomsByCampusAsync(Guid campusId);
```
Filters rooms for specific campus.

#### **Get Rooms by Type**
```csharp
Task<List<RoomDto>> GetRoomsByTypeAsync(RoomType type);
```
Filters by room type: Classroom, Lab, Stadium.

#### **Get Rooms by Status**
```csharp
Task<List<RoomDto>> GetRoomsByStatusAsync(BookingStatus status);
```
Filters by booking status.

#### **Search Rooms**
```csharp
Task<List<RoomDto>> SearchRoomsAsync(string searchTerm);
```
Searches in: Name, Description, Campus Name.

#### **Get Available Rooms**
```csharp
Task<List<RoomDto>> GetAvailableRoomsAsync(DateTime startTime, DateTime endTime);
```
Returns rooms without booking conflicts in time range.

---

### **3. Update Operations**

#### **Update Room**
```csharp
Task<RoomDto?> UpdateRoomAsync(UpdateRoomDto updateDto);
```
**Features:**
- Updates all room properties
- Campus reassignment
- Status update
- Duplicate name check (excluding self)
- Audit tracking

#### **Update Room Status**
```csharp
Task<bool> UpdateRoomStatusAsync(Guid roomId, BookingStatus status);
```
Quick status update without full entity update.

---

### **4. Delete Operations**

#### **Soft Delete**
```csharp
Task<bool> SoftDeleteRoomAsync(Guid id);
```
**Features:**
- Sets IsDeleted = true
- Checks for active bookings
- Prevents deletion if has active bookings
- Audit tracking

**Validation:**
- Cannot delete room with active bookings
- Active = Pending or Approved status
- EndTime > current time

#### **Hard Delete**
```csharp
Task<bool> DeleteRoomAsync(Guid id);
```
**Features:**
- Permanently removes from database
- Checks for any bookings/reports
- Only for rooms with no history

---

### **5. Helper Methods**

#### **Room Exists**
```csharp
Task<bool> RoomExistsAsync(Guid id);
```
Checks if room exists and is not deleted.

#### **Room Name Exists in Campus**
```csharp
Task<bool> RoomNameExistsInCampusAsync(Guid campusId, string name, Guid? excludeId);
```
Checks duplicate names within same campus.

#### **Get Room Capacity**
```csharp
Task<int> GetRoomCapacityAsync(Guid roomId);
```
Returns room capacity or 0.

#### **Is Room Available**
```csharp
Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime);
```
**Logic:**
- Room must exist and not deleted
- Room status must be Approved (Available)
- No overlapping bookings in time range
- Only checks Pending/Approved bookings

**Overlap Detection:**
```
Conflict if:
- New start is within existing booking
- New end is within existing booking
- New booking completely covers existing
```

---

## ?? DTOs Structure

### **RoomDto (Read)**
```csharp
public class RoomDto
{
    public Guid Id { get; set; }
    public Guid CampusId { get; set; }
    public string CampusName { get; set; }
    public string Name { get; set; }
    public RoomType Type { get; set; }
    public string TypeDisplay { get; set; }
    public int Capacity { get; set; }
    public BookingStatus CurrentStatus { get; set; }
    public string CurrentStatusDisplay { get; set; }
    public string Description { get; set; }
    public int TotalBookings { get; set; }
    public int PendingReports { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### **CreateRoomDto**
```csharp
public class CreateRoomDto
{
    [Required] public Guid CampusId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; }
    [Required] public RoomType Type { get; set; }
    [Required, Range(1, 1000)] public int Capacity { get; set; }
    [StringLength(500)] public string Description { get; set; }
}
```

### **UpdateRoomDto**
```csharp
public class UpdateRoomDto
{
    [Required] public Guid Id { get; set; }
    [Required] public Guid CampusId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; }
    [Required] public RoomType Type { get; set; }
    [Required, Range(1, 1000)] public int Capacity { get; set; }
    [Required] public BookingStatus CurrentStatus { get; set; }
    [StringLength(500)] public string Description { get; set; }
}
```

---

## ?? Enums

### **RoomType**
```csharp
public enum RoomType
{
    Classroom,  // "Classroom"
    Lab,        // "Laboratory"
    Stadium     // "Stadium"
}
```

### **BookingStatus (for Room.CurrentStatus)**
```csharp
public enum BookingStatus
{
    Pending,    // "Pending"
    Approved,   // "Available" (for rooms)
    Rejected,   // "Unavailable"
    Completed,  // "Completed"
    Cancelled   // "Cancelled"
}
```

**Display Mapping:**
- `Approved` ? "Available" (when used for room status)
- Other statuses use direct mapping

---

## ?? Usage Examples

### **1. Create Room**
```csharp
var createDto = new CreateRoomDto
{
    CampusId = campusId,
    Name = "Room A101",
    Type = RoomType.Classroom,
    Capacity = 50,
    Description = "Large classroom with projector"
};

var room = await _roomService.CreateRoomAsync(createDto);
// Returns: RoomDto with all details
```

### **2. Get Rooms by Campus**
```csharp
var rooms = await _roomService.GetRoomsByCampusAsync(campusId);
// Returns: List<RoomDto> for specific campus
```

### **3. Search Rooms**
```csharp
var rooms = await _roomService.SearchRoomsAsync("A101");
// Searches in: Name, Description, Campus Name
```

### **4. Check Availability**
```csharp
var startTime = DateTime.Now.AddHours(1);
var endTime = DateTime.Now.AddHours(3);

var isAvailable = await _roomService.IsRoomAvailableAsync(
    roomId, 
    startTime, 
    endTime
);
// Returns: true if no conflicts, false otherwise
```

### **5. Get Available Rooms**
```csharp
var availableRooms = await _roomService.GetAvailableRoomsAsync(
    startTime, 
    endTime
);
// Returns: List<RoomDto> without booking conflicts
```

### **6. Update Room**
```csharp
var updateDto = new UpdateRoomDto
{
    Id = roomId,
    CampusId = campusId,
    Name = "Room A101 - Updated",
    Type = RoomType.Lab,
    Capacity = 40,
    CurrentStatus = BookingStatus.Approved,
    Description = "Updated description"
};

var room = await _roomService.UpdateRoomAsync(updateDto);
```

### **7. Update Status Only**
```csharp
await _roomService.UpdateRoomStatusAsync(
    roomId, 
    BookingStatus.Rejected // Unavailable
);
```

### **8. Delete Room**
```csharp
// Soft Delete (recommended)
var success = await _roomService.SoftDeleteRoomAsync(roomId);

// Hard Delete (only if no bookings/reports)
var success = await _roomService.DeleteRoomAsync(roomId);
```

---

## ?? Test Scenarios

### **Test 1: Create Room**
```csharp
Input:
- CampusId: existing-campus-id
- Name: "Room A101"
- Type: Classroom
- Capacity: 50

Expected:
? Room created
? Status = Available
? TotalBookings = 0
? CreatedAt set
```

### **Test 2: Create Duplicate Room**
```csharp
Input:
- Same campus
- Same name "Room A101"

Expected:
? Error: "Room with name 'Room A101' already exists in this campus"
```

### **Test 3: Create Room in Invalid Campus**
```csharp
Input:
- CampusId: non-existing-id

Expected:
? Error: "Campus with ID '...' not found"
```

### **Test 4: Get Rooms by Campus**
```csharp
Action:
var rooms = await GetRoomsByCampusAsync(campusId);

Expected:
? Returns rooms for that campus only
? Each room has CampusName
? TotalBookings calculated
```

### **Test 5: Search Rooms**
```csharp
Input: "A101"

Expected:
? Finds "Room A101"
? Finds "Room A1012"
? Finds rooms with "A101" in description
```

### **Test 6: Check Availability**
```csharp
Scenario:
- Room has booking: 10:00 - 12:00
- Check: 11:00 - 13:00

Expected:
? Not available (overlap)

Scenario:
- Room has booking: 10:00 - 12:00
- Check: 13:00 - 15:00

Expected:
? Available (no overlap)
```

### **Test 7: Update Room**
```csharp
Input:
- Change capacity from 50 to 40
- Change status to Unavailable

Expected:
? Room updated
? UpdatedAt set
? UpdatedBy set
```

### **Test 8: Delete with Active Bookings**
```csharp
Scenario:
- Room has active booking (Approved, future EndTime)

Action:
await SoftDeleteRoomAsync(roomId);

Expected:
? Error: "Cannot delete room with active bookings"
```

### **Test 9: Delete without Bookings**
```csharp
Scenario:
- Room has no bookings or all completed

Action:
await SoftDeleteRoomAsync(roomId);

Expected:
? Room soft deleted
? IsDeleted = true
? DeletedAt set
```

---

## ?? Business Rules

### **Room Creation:**
1. Campus must exist
2. Room name must be unique per campus
3. Capacity: 1-1000
4. Default status: Available

### **Room Update:**
1. Campus reassignment allowed
2. Name uniqueness validated (excluding self)
3. All fields updatable

### **Room Deletion:**
1. Soft delete checks active bookings
2. Active = Pending/Approved + future EndTime
3. Hard delete checks any bookings/reports
4. Recommend soft delete for data integrity

### **Availability Check:**
1. Room must exist and not deleted
2. Status must be Approved (Available)
3. No booking overlap in time range
4. Overlap = any intersection of time periods

---

## ?? Database Queries

### **Create:**
```sql
INSERT INTO Rooms (Id, CampusId, Name, Type, Capacity, CurrentStatus, 
                   Description, IsDeleted, CreatedAt, CreatedBy)
VALUES (NEWID(), @CampusId, @Name, @Type, @Capacity, 1, 
        @Description, 0, GETDATE(), @UserId);
```

### **Get All:**
```sql
SELECT r.*, c.Name AS CampusName,
       (SELECT COUNT(*) FROM Bookings WHERE RoomId = r.Id AND IsDeleted = 0) AS TotalBookings,
       (SELECT COUNT(*) FROM RoomReports WHERE RoomId = r.Id AND IsDeleted = 0 AND Status = 0) AS PendingReports
FROM Rooms r
INNER JOIN Campuses c ON r.CampusId = c.Id
WHERE r.IsDeleted = 0;
```

### **Get Available:**
```sql
SELECT r.*
FROM Rooms r
WHERE r.IsDeleted = 0 
  AND r.CurrentStatus = 1 -- Approved
  AND NOT EXISTS (
      SELECT 1 FROM Bookings b
      WHERE b.RoomId = r.Id
        AND b.IsDeleted = 0
        AND b.Status IN (0, 1) -- Pending or Approved
        AND (
            (@StartTime >= b.StartTime AND @StartTime < b.EndTime) OR
            (@EndTime > b.StartTime AND @EndTime <= b.EndTime) OR
            (@StartTime <= b.StartTime AND @EndTime >= b.EndTime)
        )
  );
```

---

## ? Performance Considerations

### **Optimization Tips:**
1. Use `GetRoomsByCampusAsync` instead of filtering all rooms
2. Cache frequently accessed rooms
3. Index on: CampusId, Type, CurrentStatus
4. Eager load Campus, Bookings, Reports with includes

### **Efficient Queries:**
```csharp
// Good: Filtered at database level
var rooms = await GetRoomsByCampusAsync(campusId);

// Bad: Filters all rooms in memory
var rooms = (await GetAllRoomsAsync())
    .Where(r => r.CampusId == campusId)
    .ToList();
```

---

## ?? Next Steps

### **Phase 1: Room UI Pages**
- [ ] Room Index (list view)
- [ ] Room Create form
- [ ] Room Edit form
- [ ] Room Details page
- [ ] Room Dashboard

### **Phase 2: Advanced Features**
- [ ] Room images upload
- [ ] Room amenities (projector, whiteboard, etc.)
- [ ] Room maintenance schedule
- [ ] Room usage statistics
- [ ] Bulk import/export

### **Phase 3: Integration**
- [ ] Booking system integration
- [ ] Calendar view
- [ ] Room reservation
- [ ] Conflict resolution UI

---

## ? Verification Checklist

- [x] RoomDto created
- [x] CreateRoomDto with validation
- [x] UpdateRoomDto with validation
- [x] IRoomService interface complete
- [x] RoomService implementation
- [x] All CRUD operations
- [x] Search functionality
- [x] Availability checking
- [x] Status management
- [x] Campus validation
- [x] Duplicate name check
- [x] Active booking check
- [x] Soft delete
- [x] Hard delete
- [x] Helper methods
- [x] Error handling
- [x] Logging
- [x] UnitOfWork registration
- [x] DI registration
- [x] Build successful

---

## ?? Quick Start

```csharp
// Inject service
public class RoomController
{
    private readonly IRoomService _roomService;
    
    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }
    
    // Create
    var room = await _roomService.CreateRoomAsync(createDto);
    
    // Read
    var allRooms = await _roomService.GetAllRoomsAsync();
    var room = await _roomService.GetRoomByIdAsync(id);
    var campusRooms = await _roomService.GetRoomsByCampusAsync(campusId);
    var searchResults = await _roomService.SearchRoomsAsync("A101");
    
    // Update
    var updated = await _roomService.UpdateRoomAsync(updateDto);
    await _roomService.UpdateRoomStatusAsync(id, BookingStatus.Approved);
    
    // Delete
    await _roomService.SoftDeleteRoomAsync(id);
    
    // Check
    var exists = await _roomService.RoomExistsAsync(id);
    var available = await _roomService.IsRoomAvailableAsync(id, start, end);
}
```

---

**Room Service CRUD hoàn ch?nh! ??**

Ready for UI implementation!
