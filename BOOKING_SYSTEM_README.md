# 📚 BOOKING SYSTEM - HƯỚNG DẪN SỬ DỤNG

## ✅ ĐÃ HOÀN THÀNH

Hệ thống Booking cho phép **Student** và **Lecturer** đặt phòng học/phòng lab/sân vận động.

---

## 🎯 CHỨC NĂNG ĐÃ IMPLEMENT

### **1. Xem danh sách Bookings của tôi (`/Booking/Index`)**

- ✅ Hiển thị tất cả booking của user hiện tại
- ✅ Filter theo:
  - Tìm kiếm (room name, purpose)
  - Status (Pending, Approved, Rejected, Cancelled, Completed)
  - From Date
- ✅ Pagination (10 items/page)
- ✅ Thông tin hiển thị:
  - Room name & Campus
  - Start Time & End Time
  - Duration (phút)
  - Purpose
  - Status badge với màu sắc khác nhau
- ✅ Actions:
  - View Details (tất cả booking)
  - Edit (chỉ Pending & chưa quá hạn)
  - Cancel (chỉ Pending/Approved & chưa quá hạn)

### **2. Tạo Booking mới (`/Booking/Create`)**

- ✅ Chọn Room theo Campus (grouped dropdown)
- ✅ Chọn Start Time & End Time (datetime picker)
- ✅ Tự động tính Duration
- ✅ Nhập Purpose (max 500 ký tự)
- ✅ Validation:
  - Không đặt trong quá khứ
  - Start time < End time
  - Room phải tồn tại & Active
  - Không conflict với booking khác
  - Không conflict với schedule cố định
  - Phải có break time 15 phút với schedule
- ✅ Status mặc định: **Pending** (chờ admin approve)

### **3. Xem chi tiết Booking (`/Booking/Details/{id}`)**

- ✅ Thông tin đầy đủ:
  - Room & Campus
  - User (tên & email)
  - Start/End Time với format đẹp
  - Duration
  - Purpose
  - Admin Note (nếu có)
  - Created At
- ✅ Actions:
  - Edit (nếu Pending & chưa quá hạn)
  - Cancel (nếu Pending/Approved & chưa quá hạn)

### **4. Sửa Booking (`/Booking/Edit/{id}`)**

- ✅ Chỉ sửa được booking **Pending**
- ✅ Chỉ owner mới sửa được
- ✅ Không sửa booking trong quá khứ
- ✅ Validate conflict tương tự Create
- ✅ Pre-fill thông tin hiện tại

### **5. Hủy Booking (`Cancel`)**

- ✅ Chỉ hủy được booking **Pending** hoặc **Approved**
- ✅ Chỉ owner mới hủy được
- ✅ Không hủy booking trong quá khứ
- ✅ Status → **Cancelled**

---

## 📋 BUSINESS RULES

### **Ai có thể book?**

- ✅ **Student**: Book phòng cho học tập
- ✅ **Lecturer**: Book phòng cho giảng dạy
- ❌ **Admin**: Không book (quản lý approve/reject)

### **Booking Lifecycle:**

```
Create → PENDING → (Admin Approve) → APPROVED → (Tự động) → COMPLETED
                ↓
         (Admin Reject) → REJECTED
                ↓
         (User Cancel) → CANCELLED
```

### **Khi nào có thể Edit/Cancel?**

- ✅ Status = Pending
- ✅ Start Time > Now (chưa bắt đầu)
- ✅ User là owner

### **Conflict Detection:**

1. **Booking Conflict**: Không trùng với booking khác (Pending/Approved) trong cùng phòng
2. **Schedule Conflict**: Không trùng với lịch cố định (Academic Course, Maintenance)
3. **Break Time**: Phải có tối thiểu 15 phút giữa các lịch

### **Room Requirements:**

- ✅ RoomStatus = Active (không Under Maintenance, Out of Service)
- ✅ Tồn tại trong hệ thống

---

## 🎨 UI/UX FEATURES

### **Navigation:**

- Menu "My Bookings" hiển thị cho Student/Lecturer
- Admin không thấy menu này (dùng Admin panel riêng)

### **Status Badge Colors:**

- 🟡 **Pending**: Warning (vàng)
- 🟢 **Approved**: Success (xanh)
- 🔴 **Rejected**: Danger (đỏ)
- ⚫ **Cancelled**: Secondary (xám)
- 🔵 **Completed**: Info (xanh dương)

### **Interactive:**

- ✅ Auto-calculate duration khi chọn time
- ✅ Auto-set end time = start time + 1 hour
- ✅ Confirm dialog khi cancel
- ✅ Min date = today (không chọn quá khứ)
- ✅ Success/Error notifications với TempData

---

## 🔧 BACKEND ARCHITECTURE

### **Files Created:**

#### **DTOs:**

- `BookingDto.cs` - Hiển thị booking
- `CreateBookingDto.cs` - Tạo booking mới
- `UpdateBookingDto.cs` - Cập nhật booking

#### **Service:**

- `IBookingService.cs` - Interface
- `BookingService.cs` - Business logic với:
  - CRUD operations
  - Conflict detection
  - Permission checks
  - Pagination & filtering

#### **Pages:**

- `Index.cshtml/.cs` - Danh sách bookings
- `Create.cshtml/.cs` - Tạo mới
- `Details.cshtml/.cs` - Chi tiết
- `Edit.cshtml/.cs` - Chỉnh sửa

#### **Infrastructure:**

- Added `Booking` repository to `UnitOfWork`
- Registered `BookingService` in `IocContainer`
- Updated `_Layout.cshtml` with navigation

---

## 📊 DATABASE

Entity `Booking` đã có sẵn:

```csharp
public class Booking : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public string Purpose { get; set; }
    public string AdminNote { get; set; }

    public User User { get; set; }
    public Room Room { get; set; }
}
```

**BookingStatus Enum:**

- Pending (0)
- Approved (1)
- Rejected (2)
- Completed (3)
- Cancelled (4)

---

## 🚀 CÁCH SỬ DỤNG

### **1. Chạy dự án:**

```powershell
cd c:\BaoData\prn222\UniSpace
docker-compose up --build
```

### **2. Login:**

- Student: `student1@gmail.com` / `1@`
- Lecturer: `lecturer1@gmail.com` / `1@`

### **3. Truy cập:**

- Danh sách booking: http://localhost:5000/Booking/Index
- Tạo booking mới: http://localhost:5000/Booking/Create

### **4. Workflow:**

1. Click "New Booking"
2. Chọn Room (theo Campus)
3. Chọn thời gian
4. Nhập Purpose
5. Click "Create Booking"
6. Booking ở trạng thái **Pending**
7. **Chờ Admin approve** (chức năng này chưa làm)

---

## ❌ CHƯA LÀM

### **Admin Approval System:**

- ❌ Admin Dashboard cho Bookings
- ❌ Approve/Reject booking
- ❌ Add AdminNote khi reject
- ❌ Notification khi approved/rejected
- ❌ Statistics & Reports

### **Auto-Complete Status:**

- ❌ Tự động chuyển Approved → Completed khi EndTime qua

### **Advanced Features:**

- ❌ Email notification
- ❌ Calendar view
- ❌ Recurring bookings
- ❌ Room availability calendar

---

## 🔍 TEST SCENARIOS

### **Scenario 1: Tạo booking thành công**

1. Login student1@gmail.com
2. Create booking: Room 101, Tomorrow 9:00-10:00
3. ✅ Kết quả: Booking created, Status = Pending

### **Scenario 2: Conflict với booking khác**

1. Tạo booking: Room 101, Tomorrow 9:00-10:00
2. Tạo booking khác: Room 101, Tomorrow 9:30-10:30
3. ❌ Kết quả: Error "Room is already booked"

### **Scenario 3: Conflict với schedule**

1. Room 101 có schedule: Monday 7:30-9:30
2. Tạo booking: Monday 9:00-10:00
3. ❌ Kết quả: Error "Conflicts with scheduled activities"

### **Scenario 4: Edit booking**

1. Tạo booking (Pending)
2. Click Edit
3. Đổi time → Save
4. ✅ Kết quả: Updated

### **Scenario 5: Cancel booking**

1. Tạo booking (Pending)
2. Click Cancel
3. Confirm
4. ✅ Kết quả: Status = Cancelled

### **Scenario 6: Không edit booking đã approve**

1. Admin approve booking (giả sử)
2. User thử edit
3. ❌ Kết quả: "Cannot update booking with status: Approved"

---

## 📝 NOTES

- Tất cả bookings mặc định **Pending** - cần admin approve
- User chỉ thấy booking của mình
- Không thể book phòng đang Under Maintenance
- Break time 15 phút được enforce với schedules
- Soft delete - không xóa hẳn khỏi DB

---

## 🎯 NEXT STEPS

Nếu muốn làm tiếp Admin Approval:

1. Tạo `/Admin/Booking/Index` - List all pending bookings
2. Tạo `/Admin/Booking/Approve/{id}` - Approve handler
3. Tạo `/Admin/Booking/Reject/{id}` - Reject handler with note
4. Add statistics to Admin Dashboard

---

**🎉 HỆ THỐNG BOOKING HOÀN CHỈNH CHO STUDENT/LECTURER!**
