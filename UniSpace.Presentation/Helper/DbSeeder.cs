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

        /// <summary>
        /// Seed all initial data (Users, Campuses, and Rooms)
        /// </summary>
        public static async Task SeedAllAsync(UniSpaceDbContext context)
        {
            await SeedUsersAsync(context);
            await SeedCampusesAsync(context);
            await SeedRoomsAsync(context);
        }
    }
}
