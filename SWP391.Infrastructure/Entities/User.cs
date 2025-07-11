using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SWP391.Infrastructure.Enums;
using System.Collections.Generic;

namespace SWP391.Infrastructure.Entities
{
    [Table("User")]
    public partial class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string FullName { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(255)]
        public string Email { get; set; } = null!;

        [StringLength(255)]
        public string Password { get; set; } = null!;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public string? PhoneNumber { get; set; }

        public RoleEnum Role { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public int? CycleLength { get; set; } 

        public int? MenstrualLength { get; set; }

        // Navigation property cho Appointments
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Navigation property cho Blogs
        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
    }
}