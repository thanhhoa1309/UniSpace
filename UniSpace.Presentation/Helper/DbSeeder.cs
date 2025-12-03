using Microsoft.EntityFrameworkCore;
using UniSpace.BusinessObject.Enums;
using UniSpace.Domain;
using UniSpace.Domain.Entities;
using UniSpace.Services.Utils;

namespace EVAuctionTrader.Presentation.Helper
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(UniSpaceDbContext context)
        {
            if (!await context.User.AnyAsync(u => u.Role == RoleType.Admin))
            {
                var passwordHasher = new PasswordHasher();
                var admin = new User
                {
                    FullName = "Admin User",
                    Email = "admin@gmail.com",
                    PasswordHash = passwordHasher.HashPassword("1@"),
                    Role = RoleType.Admin,
                    IsActive = true
                };

                await context.User.AddAsync(admin);
            }

            if (!await context.User.AnyAsync(u => u.Role == RoleType.Student))
            {
                var passwordHasher = new PasswordHasher();
                var students = new List<User>
                {
                    new User
                    {
                        FullName = "Student 1",
                        Email = "student1@gmail.com",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Student,
                        IsActive = true
                    },
                    new User
                    {
                        FullName = "Student 2",
                        Email = "student2@gmail.com",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Student,
                        IsActive = true
                    }
                };
                await context.User.AddRangeAsync(students);
            }

            if (!await context.User.AnyAsync(u => u.Role == RoleType.Lecturer))
            {
                var passwordHasher = new PasswordHasher();
                var lecturers = new List<User>
                {
                    new User
                    {
                        FullName = "Lecturer 1",
                        Email = "lecturer1@gmail.com",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Lecturer,
                        IsActive = true
                    },
                    new User
                    {
                        FullName = "Lecturer 2",
                        Email = "lecturer2@gmail.com",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Lecturer,
                        IsActive = true
                    }
                };
                await context.User.AddRangeAsync(lecturers);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedCampusesAsync(UniSpaceDbContext context)
        {
            if (!await context.Campuses.AnyAsync())
            {
                var admin = await context.User.FirstOrDefaultAsync(u => u.Role == RoleType.Admin);
                var adminId = admin?.Id ?? Guid.Empty;

                var campuses = new List<Campus>
                {
                    new Campus
                    {
                        Id = Guid.NewGuid(),
                        Name = "FPT University - Hoa Lac Campus",
                        Address = "Thach That, Hanoi, Vietnam",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    },
                    new Campus
                    {
                        Id = Guid.NewGuid(),
                        Name = "FPT University - Quy Nhon Campus",
                        Address = "Quy Nhon, Binh Dinh, Vietnam",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    },
                    new Campus
                    {
                        Id = Guid.NewGuid(),
                        Name = "FPT University - Da Nang Campus",
                        Address = "Ngu Hanh Son, Da Nang, Vietnam",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    },
                    new Campus
                    {
                        Id = Guid.NewGuid(),
                        Name = "FPT University - Ho Chi Minh Campus",
                        Address = "District 9, Ho Chi Minh City, Vietnam",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    },
                    new Campus
                    {
                        Id = Guid.NewGuid(),
                        Name = "FPT University - Can Tho Campus",
                        Address = "Ninh Kieu, Can Tho, Vietnam",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    }
                };

                await context.Campuses.AddRangeAsync(campuses);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedRoomsAsync(UniSpaceDbContext context)
        {
            if (!await context.Rooms.AnyAsync())
            {
                var admin = await context.User.FirstOrDefaultAsync(u => u.Role == RoleType.Admin);
                var adminId = admin?.Id ?? Guid.Empty;

                // Get all campuses
                var campuses = await context.Campuses.ToListAsync();

                if (!campuses.Any())
                {
                    // If no campuses exist, seed them first
                    await SeedCampusesAsync(context);
                    campuses = await context.Campuses.ToListAsync();
                }

                var rooms = new List<Room>();

                // Seed rooms for each campus
                foreach (var campus in campuses)
                {
                    // Classrooms (5 per campus)
                    for (int i = 1; i <= 5; i++)
                    {
                        rooms.Add(new Room
                        {
                            Id = Guid.NewGuid(),
                            CampusId = campus.Id,
                            Name = $"Room {i}01",
                            Type = RoomType.Classroom,
                            Capacity = 40 + (i * 10), // 50, 60, 70, 80, 90
                            CurrentStatus = BookingStatus.Approved, // Available
                            RoomStatus = RoomStatus.Active,
                            Description = $"Standard classroom with projector and whiteboard. Capacity: {40 + (i * 10)} students.",
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = adminId
                        });
                    }

                    // Labs (3 per campus)
                    for (int i = 1; i <= 3; i++)
                    {
                        rooms.Add(new Room
                        {
                            Id = Guid.NewGuid(),
                            CampusId = campus.Id,
                            Name = $"Lab {i}",
                            Type = RoomType.Lab,
                            Capacity = 30 + (i * 5), // 35, 40, 45
                            CurrentStatus = BookingStatus.Approved, // Available
                            RoomStatus = RoomStatus.Active,
                            Description = $"Computer lab equipped with {30 + (i * 5)} workstations. High-speed internet and development software installed.",
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = adminId
                        });
                    }

                    // Stadiums (1-2 per campus)
                    rooms.Add(new Room
                    {
                        Id = Guid.NewGuid(),
                        CampusId = campus.Id,
                        Name = "Main Stadium",
                        Type = RoomType.Stadium,
                        Capacity = 500,
                        CurrentStatus = BookingStatus.Approved, // Available
                        RoomStatus = RoomStatus.Active,
                        Description = "Large outdoor stadium suitable for sports events, ceremonies, and large gatherings. Capacity: 500 people.",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    });

                    rooms.Add(new Room
                    {
                        Id = Guid.NewGuid(),
                        CampusId = campus.Id,
                        Name = "Indoor Sports Hall",
                        Type = RoomType.Stadium,
                        Capacity = 200,
                        CurrentStatus = BookingStatus.Approved, // Available
                        RoomStatus = RoomStatus.Active,
                        Description = "Indoor sports facility for basketball, volleyball, and badminton. Capacity: 200 people.",
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    });
                }

                // Add all rooms to database
                await context.Rooms.AddRangeAsync(rooms);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedSchedulesAsync(UniSpaceDbContext context)
        {
            if (!await context.Schedules.AnyAsync())
            {
                var admin = await context.User.FirstOrDefaultAsync(u => u.Role == RoleType.Admin);
                var adminId = admin?.Id ?? Guid.Empty;

                // Get all rooms
                var rooms = await context.Rooms.Where(r => !r.IsDeleted).ToListAsync();

                if (!rooms.Any())
                {
                    // If no rooms exist, seed them first
                    await SeedRoomsAsync(context);
                    rooms = await context.Rooms.Where(r => !r.IsDeleted).ToListAsync();
                }

                var schedules = new List<Schedule>();
                var currentDate = DateTime.UtcNow.Date;

                // Academic semester dates
                var semesterStart = new DateTime(currentDate.Year, 9, 1); // September 1st
                var semesterEnd = new DateTime(currentDate.Year, 12, 20); // December 20th

                // If current date is past semester end, use next year
                if (currentDate > semesterEnd)
                {
                    semesterStart = new DateTime(currentDate.Year + 1, 9, 1);
                    semesterEnd = new DateTime(currentDate.Year + 1, 12, 20);
                }

                // Seed Academic Course Schedules
                var classrooms = rooms.Where(r => r.Type == RoomType.Classroom).ToList();
                var labs = rooms.Where(r => r.Type == RoomType.Lab).ToList();

                // Sample courses for classrooms
                var courses = new[]
                {
                    new { Name = "PRN221 - Advanced C# Programming", Time = (new TimeSpan(7, 30, 0), new TimeSpan(9, 30, 0)), Day = 2 }, // Monday 7:30-9:30
                    new { Name = "PRN222 - ASP.NET Core Development", Time = (new TimeSpan(9, 45, 0), new TimeSpan(11, 45, 0)), Day = 2 }, // Monday 9:45-11:45
                    new { Name = "SWP391 - Software Project Management", Time = (new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0)), Day = 3 }, // Tuesday 13:00-15:00
                    new { Name = "PRN212 - Basics of C# Programming", Time = (new TimeSpan(15, 15, 0), new TimeSpan(17, 15, 0)), Day = 3 }, // Tuesday 15:15-17:15
                    new { Name = "SWR302 - Software Requirements", Time = (new TimeSpan(7, 30, 0), new TimeSpan(9, 30, 0)), Day = 4 }, // Wednesday 7:30-9:30
                    new { Name = "SWT301 - Software Testing", Time = (new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0)), Day = 4 }, // Wednesday 13:00-15:00
                    new { Name = "PRN231 - Web API Development", Time = (new TimeSpan(9, 45, 0), new TimeSpan(11, 45, 0)), Day = 5 }, // Thursday 9:45-11:45
                    new { Name = "IOT102 - Internet of Things", Time = (new TimeSpan(15, 15, 0), new TimeSpan(17, 15, 0)), Day = 5 }, // Thursday 15:15-17:15
                    new { Name = "SWD392 - SW Architecture & Design", Time = (new TimeSpan(7, 30, 0), new TimeSpan(9, 30, 0)), Day = 6 }, // Friday 7:30-9:30
                    new { Name = "MLN111 - Machine Learning Basics", Time = (new TimeSpan(13, 0, 0), new TimeSpan(15, 0, 0)), Day = 6 } // Friday 13:00-15:00
                };

                // Assign courses to classrooms
                for (int i = 0; i < Math.Min(classrooms.Count, courses.Length); i++)
                {
                    var classroom = classrooms[i];
                    var course = courses[i];

                    schedules.Add(new Schedule
                    {
                        Id = Guid.NewGuid(),
                        RoomId = classroom.Id,
                        ScheduleType = ScheduleType.Academic_Course,
                        Title = course.Name,
                        StartTime = course.Time.Item1,
                        EndTime = course.Time.Item2,
                        DayOfWeek = course.Day,
                        StartDate = semesterStart,
                        EndDate = semesterEnd,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    });
                }

                // Lab courses
                var labCourses = new[]
                {
                    new { Name = "PRN221 - Lab Session A", Time = (new TimeSpan(7, 30, 0), new TimeSpan(10, 30, 0)), Day = 2 },
                    new { Name = "PRN221 - Lab Session B", Time = (new TimeSpan(13, 0, 0), new TimeSpan(16, 0, 0)), Day = 2 },
                    new { Name = "PRN222 - Lab Session A", Time = (new TimeSpan(7, 30, 0), new TimeSpan(10, 30, 0)), Day = 4 },
                    new { Name = "PRN222 - Lab Session B", Time = (new TimeSpan(13, 0, 0), new TimeSpan(16, 0, 0)), Day = 4 },
                    new { Name = "IOT102 - Hardware Lab", Time = (new TimeSpan(7, 30, 0), new TimeSpan(10, 30, 0)), Day = 6 }
                };

                // Assign lab courses
                for (int i = 0; i < Math.Min(labs.Count, labCourses.Length); i++)
                {
                    var lab = labs[i];
                    var course = labCourses[i];

                    schedules.Add(new Schedule
                    {
                        Id = Guid.NewGuid(),
                        RoomId = lab.Id,
                        ScheduleType = ScheduleType.Academic_Course,
                        Title = course.Name,
                        StartTime = course.Time.Item1,
                        EndTime = course.Time.Item2,
                        DayOfWeek = course.Day,
                        StartDate = semesterStart,
                        EndDate = semesterEnd,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId
                    });
                }

                // Seed Recurring Maintenance Schedules
                var maintenanceRooms = rooms.Take(10).ToList(); // First 10 rooms for maintenance

                var maintenanceSchedules = new[]
                {
                    new { Name = "Weekly Cleaning Service", Time = (new TimeSpan(18, 0, 0), new TimeSpan(19, 0, 0)), Day = 6 }, // Friday 18:00-19:00
                    new { Name = "HVAC System Check", Time = (new TimeSpan(7, 0, 0), new TimeSpan(7, 30, 0)), Day = 7 }, // Saturday 7:00-7:30
                    new { Name = "Equipment Inspection", Time = (new TimeSpan(8, 0, 0), new TimeSpan(9, 0, 0)), Day = 7 }, // Saturday 8:00-9:00
                    new { Name = "Network Infrastructure Check", Time = (new TimeSpan(19, 0, 0), new TimeSpan(20, 0, 0)), Day = 3 }, // Tuesday 19:00-20:00
                    new { Name = "Electrical System Maintenance", Time = (new TimeSpan(6, 0, 0), new TimeSpan(7, 0, 0)), Day = 1 } // Sunday 6:00-7:00
                };

                // Maintenance period (whole year)
                var maintenanceStart = new DateTime(currentDate.Year, 1, 1);
                var maintenanceEnd = new DateTime(currentDate.Year, 12, 31);

                foreach (var room in maintenanceRooms)
                {
                    // Assign 2-3 different maintenance schedules per room
                    var maintenanceCount = new Random().Next(2, 4);
                    for (int i = 0; i < maintenanceCount && i < maintenanceSchedules.Length; i++)
                    {
                        var maintenance = maintenanceSchedules[i];

                        schedules.Add(new Schedule
                        {
                            Id = Guid.NewGuid(),
                            RoomId = room.Id,
                            ScheduleType = ScheduleType.Recurring_Maintenance,
                            Title = maintenance.Name,
                            StartTime = maintenance.Time.Item1,
                            EndTime = maintenance.Time.Item2,
                            DayOfWeek = maintenance.Day,
                            StartDate = maintenanceStart,
                            EndDate = maintenanceEnd,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = adminId
                        });
                    }
                }

                // Add all schedules to database
                await context.Schedules.AddRangeAsync(schedules);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seed all initial data (Users, Campuses, Rooms, and Schedules)
        /// </summary>
        public static async Task SeedAllAsync(UniSpaceDbContext context)
        {
            await SeedUsersAsync(context);
            await SeedCampusesAsync(context);
            await SeedRoomsAsync(context);
            await SeedSchedulesAsync(context);
        }
    }
}
