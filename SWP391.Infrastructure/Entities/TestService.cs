using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP391.Infrastructure.Entities
{
    [Table("TestService")]
    public partial class TestService
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string? TestName { get; set; }

        // Foreign key to Appointment
        public int AppointmentId { get; set; }

        // Navigation property to Appointment
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; } = null!;

        // Foreign key to TestResult
        public int TestResultId { get; set; }

        // Navigation property to TestResult
        [ForeignKey("TestResultId")]
        public TestResult TestResult { get; set; } = null!;
    }
}