using UniSpace.BusinessObject.Enums;

namespace UniSpace.Domain.Entities
{
    public class User : BaseEntity
    {

        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public RoleType Role { get; set; }


        public ICollection<Booking> Bookings { get; set; }
        public ICollection<RoomReport> Reports { get; set; }
    }
}
