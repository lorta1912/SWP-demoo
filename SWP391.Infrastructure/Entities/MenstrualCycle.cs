using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SWP391.Infrastructure.Entities
{
    [Table("MenstrualCycle")]
    public partial class MenstrualCycle
    {
        [Key]
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;        
    }
}
