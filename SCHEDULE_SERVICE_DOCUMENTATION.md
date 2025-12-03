# Schedule Management Service Documentation

## Overview
The Schedule Management Service provides comprehensive CRUD operations for managing room schedules with advanced features including conflict detection and break time management.

## Features Implemented

### 1. **Complete CRUD Operations**
- ? Create Schedule
- ? Read Schedule (multiple query methods)
- ? Update Schedule
- ? Soft Delete Schedule
- ? Hard Delete Schedule

### 2. **Conflict Prevention**
- ? Schedules for the same room cannot overlap on the same day of week
- ? Date range validation (schedules with overlapping date ranges are checked)
- ? Time validation (start time must be before end time)

### 3. **Break Time Management**
- ? Configurable break time between schedules (default: 15 minutes)
- ? Automatic validation to ensure minimum break time between consecutive schedules
- ? Break time is considered when checking for conflicts

### 4. **Advanced Query Methods**
- Get all schedules
- Get schedule by ID
- Get schedules by room
- Get schedules by type (Academic Course / Recurring Maintenance)
- Get schedules by date range
- Get schedules by day of week
- Get schedules for a specific room on a specific date

### 5. **Validation & Error Handling**
- Input validation with detailed error messages
- Conflict detection with specific conflict details
- Room existence validation
- Date and time range validation

## Data Model

### Schedule Entity Properties
```csharp
public class Schedule : BaseEntity
{
    public Guid RoomId { get; set; }
    public ScheduleType ScheduleType { get; set; }
    public string Title { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DayOfWeek { get; set; }  // 0 = Sunday, 6 = Saturday
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Room Room { get; set; }
}
```

### Schedule Types
- **Academic_Course**: Regular class schedules
- **Recurring_Maintenance**: Scheduled maintenance windows

## DTOs

### CreateScheduleDto
```csharp
{
    "roomId": "guid",
    "scheduleType": 0,  // 0 = Academic_Course, 1 = Recurring_Maintenance
    "title": "string",
    "startTime": "09:00:00",
    "endTime": "11:00:00",
    "dayOfWeek": 1,  // 0-6 (Sunday-Saturday)
    "startDate": "2024-01-01",
    "endDate": "2024-06-30",
    "breakTimeMinutes": 15  // Optional, default 15
}
```

### UpdateScheduleDto
```csharp
{
    "id": "guid",
    "roomId": "guid",
    "scheduleType": 0,
    "title": "string",
    "startTime": "09:00:00",
    "endTime": "11:00:00",
    "dayOfWeek": 1,
    "startDate": "2024-01-01",
    "endDate": "2024-06-30"
}
```

### ScheduleDto (Response)
```csharp
{
    "id": "guid",
    "roomId": "guid",
    "roomName": "string",
    "campusName": "string",
    "scheduleType": 0,
    "scheduleTypeDisplay": "Academic Course",
    "title": "string",
    "startTime": "09:00:00",
    "endTime": "11:00:00",
    "dayOfWeek": 1,
    "dayOfWeekDisplay": "Monday",
    "startDate": "2024-01-01",
    "endDate": "2024-06-30",
    "createdAt": "2024-01-01T00:00:00Z"
}
```

## Conflict Detection Logic

### How it Works

1. **Date Range Overlap Check**
   - Checks if the schedule's date range overlaps with existing schedules
   - Only schedules with overlapping date ranges are compared for time conflicts

2. **Day of Week Match**
   - Only compares schedules that occur on the same day of the week
   - Example: A Monday schedule won't conflict with a Tuesday schedule

3. **Time Overlap Check**
   - Compares the time slots of schedules on the same day
   - Includes break time buffer to ensure separation between schedules

4. **Break Time Buffer**
   - Adds configurable buffer (default 15 minutes) before and after each schedule
   - Example: If Schedule A ends at 11:00, Schedule B cannot start before 11:15

### Conflict Detection Algorithm

```
For new schedule (Room R, Day D, Time T1-T2, Date Range D1-D2):
  1. Find all schedules for Room R on Day D
  2. Filter schedules with overlapping date ranges
  3. For each existing schedule (Time E1-E2):
     a. Extend E1 by subtracting break time: E1' = E1 - break
     b. Extend E2 by adding break time: E2' = E2 + break
     c. Check if new schedule overlaps with extended time:
        - T1 is between E1' and E2'
        - T2 is between E1' and E2'
        - T1-T2 completely covers E1'-E2'
  4. If any overlap found, reject with conflict details
```

## Usage Examples

### Example 1: Create a Class Schedule

```csharp
var createDto = new CreateScheduleDto
{
    RoomId = roomId,
    ScheduleType = ScheduleType.Academic_Course,
    Title = "Advanced Programming - CS301",
    StartTime = new TimeSpan(9, 0, 0),    // 9:00 AM
    EndTime = new TimeSpan(11, 0, 0),     // 11:00 AM
    DayOfWeek = 1,                         // Monday
    StartDate = new DateTime(2024, 1, 15),
    EndDate = new DateTime(2024, 5, 30),
    BreakTimeMinutes = 15
};

var result = await _scheduleService.CreateScheduleAsync(createDto);
```

### Example 2: Create Maintenance Schedule

```csharp
var maintenanceDto = new CreateScheduleDto
{
    RoomId = roomId,
    ScheduleType = ScheduleType.Recurring_Maintenance,
    Title = "Weekly Lab Maintenance",
    StartTime = new TimeSpan(17, 0, 0),   // 5:00 PM
    EndTime = new TimeSpan(19, 0, 0),     // 7:00 PM
    DayOfWeek = 5,                         // Friday
    StartDate = new DateTime(2024, 1, 1),
    EndDate = new DateTime(2024, 12, 31),
    BreakTimeMinutes = 30  // Longer break for maintenance
};

var result = await _scheduleService.CreateScheduleAsync(maintenanceDto);
```

### Example 3: Query Schedules

```csharp
// Get all schedules for a specific room
var roomSchedules = await _scheduleService.GetSchedulesByRoomAsync(roomId);

// Get all Monday schedules
var mondaySchedules = await _scheduleService.GetSchedulesByDayOfWeekAsync(1);

// Get schedules for a specific date
var todaySchedules = await _scheduleService.GetSchedulesForRoomOnDateAsync(
    roomId, 
    DateTime.Today
);

// Check for conflicts before creating
var conflicts = await _scheduleService.GetConflictingSchedulesAsync(
    roomId,
    dayOfWeek: 1,
    startTime: new TimeSpan(9, 0, 0),
    endTime: new TimeSpan(11, 0, 0),
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 6, 30)
);
```

## Error Handling

### Common Errors

1. **Conflict Error**
   ```
   Status: 409 Conflict
   Message: "Schedule conflicts with existing schedule(s): Advanced Math (09:00 - 11:00). 
            Please ensure there is at least 15 minutes break time between schedules."
   ```

2. **Validation Error**
   ```
   Status: 400 Bad Request
   Message: "Start time must be before end time"
   ```

3. **Not Found Error**
   ```
   Status: 404 Not Found
   Message: "Room with ID 'xxx' not found"
   ```

## Additional Features & Requirements

### Current Implementation ?

1. ? **No Schedule Overlap**: Schedules for the same room cannot overlap on the same day
2. ? **Break Time Management**: Configurable break time between schedules
3. ? **Date Range Validation**: Start date must be before end date
4. ? **Time Validation**: Start time must be before end time
5. ? **Soft Delete Support**: Schedules can be soft-deleted for audit trail
6. ? **Audit Fields**: Tracks who created/updated/deleted schedules and when

### Future Enhancement Suggestions

You can easily extend this service with:

1. **Bulk Operations**
   ```csharp
   Task<List<ScheduleDto>> CreateBulkSchedulesAsync(List<CreateScheduleDto> createDtos);
   ```

2. **Schedule Templates**
   ```csharp
   Task<List<ScheduleDto>> CreateFromTemplateAsync(Guid templateId, DateTime startDate, DateTime endDate);
   ```

3. **Conflict Resolution Suggestions**
   ```csharp
   Task<List<TimeSlotSuggestion>> SuggestAvailableTimeSlotsAsync(Guid roomId, int dayOfWeek, DateTime date);
   ```

4. **Schedule Export**
   ```csharp
   Task<byte[]> ExportSchedulesToExcelAsync(Guid roomId, DateTime startDate, DateTime endDate);
   ```

5. **Recurring Pattern Support**
   - Weekly recurrence (every n weeks)
   - Custom date exclusions (holidays, breaks)

6. **Notification System**
   - Alert users when schedules are created/updated/deleted
   - Remind users of upcoming schedule changes

## Testing Recommendations

### Unit Tests
```csharp
- Test_CreateSchedule_Success
- Test_CreateSchedule_WithConflict_ShouldFail
- Test_CreateSchedule_WithInsufficientBreakTime_ShouldFail
- Test_UpdateSchedule_Success
- Test_UpdateSchedule_WithConflict_ShouldFail
- Test_GetSchedulesByRoom_ReturnsOrderedList
- Test_GetSchedulesForDate_FiltersCorrectly
- Test_SoftDelete_Success
- Test_IsTimeSlotAvailable_WithBreakTime
- Test_GetConflictingSchedules_ReturnsCorrectConflicts
```

### Integration Tests
```csharp
- Test_CreateMultipleSchedules_NoOverlap
- Test_ScheduleAcrossSemesters
- Test_WeeklyMaintenanceSchedule
- Test_ConflictDetection_AcrossDateRanges
```

## Performance Considerations

1. **Database Indexing**: Add indexes on:
   - `RoomId`
   - `DayOfWeek`
   - `StartDate, EndDate`
   - Composite index: `(RoomId, DayOfWeek, StartDate, EndDate)`

2. **Caching**: Consider caching frequently accessed schedules

3. **Batch Operations**: For bulk schedule creation, use `AddRangeAsync`

## Database Schema

```sql
CREATE TABLE Schedules (
    Id UUID PRIMARY KEY,
    RoomId UUID NOT NULL,
    ScheduleType TEXT NOT NULL,
    Title TEXT NOT NULL,
    StartTime INTERVAL NOT NULL,
    EndTime INTERVAL NOT NULL,
    DayOfWeek INTEGER NOT NULL,
    StartDate TIMESTAMP NOT NULL,
    EndDate TIMESTAMP NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP NOT NULL,
    CreatedBy UUID NOT NULL,
    UpdatedAt TIMESTAMP,
    UpdatedBy UUID,
    DeletedAt TIMESTAMP,
    DeletedBy UUID,
    FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE,
    CHECK (StartTime < EndTime),
    CHECK (StartDate < EndDate),
    CHECK (DayOfWeek >= 0 AND DayOfWeek <= 6)
);

-- Recommended Indexes
CREATE INDEX idx_schedules_room_day ON Schedules(RoomId, DayOfWeek) WHERE IsDeleted = FALSE;
CREATE INDEX idx_schedules_daterange ON Schedules(StartDate, EndDate) WHERE IsDeleted = FALSE;
CREATE INDEX idx_schedules_room_day_date ON Schedules(RoomId, DayOfWeek, StartDate, EndDate) WHERE IsDeleted = FALSE;
```

## Security Considerations

1. **Authorization**: Ensure only admins can create/update/delete schedules
2. **Validation**: All inputs are validated before processing
3. **Soft Delete**: Maintains audit trail of deleted schedules
4. **User Tracking**: All operations track the user who performed them

## Summary

This Schedule Management Service provides a robust, production-ready solution for managing room schedules with:
- ? Complete CRUD operations
- ? Intelligent conflict detection
- ? Configurable break time management
- ? Comprehensive query capabilities
- ? Full audit trail
- ? Detailed error handling and validation

The service is designed to be extensible and can be easily enhanced with additional features as needed.
