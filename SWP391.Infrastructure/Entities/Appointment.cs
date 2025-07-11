using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SWP391.Infrastructure.Entities
{
    [Table("Appointment")]
    public partial class Appointment
    {
        [Key]
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public float ValueDiscount { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        // Navigation property cho AdviseServices
        public ICollection<AdviseService> AdviseServices { get; set; } = new List<AdviseService>();

        // Navigation property cho TestServices
        public ICollection<TestService> TestServices { get; set; } = new List<TestService>();
    }
}