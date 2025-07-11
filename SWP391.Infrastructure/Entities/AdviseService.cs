using SWP391.Infrastructure.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP391.Infrastructure.Entities
{
    [Table("AdviseService")]
    public partial class AdviseService
    {
        [Key]
        public int Id { get; set; }
        public ContactTypeEnum ContactType { get; set; }
        [StringLength(255)]
        public string? ConsulationType { get; set; }

        // Foreign key to Appointment
        public int AppointmentId { get; set; }

        // Navigation property to Appointment
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; } = null!;

        // Foreign key to AdviseNote
        public int AdviseNoteId { get; set; }

        // Navigation property to AdviseNote
        [ForeignKey("AdviseNoteId")]
        public AdviseNote AdviseNote { get; set; } = null!;
    }
}