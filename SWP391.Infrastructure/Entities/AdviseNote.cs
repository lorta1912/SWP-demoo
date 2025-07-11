using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SWP391.Infrastructure.Entities
{
    [Table("AdviseNote")]
    public partial class AdviseNote
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string? Note { get; set; }
        [StringLength(255)]
        public string? Sugggestion { get; set; }

        // Navigation property cho AdviseServices
        public ICollection<AdviseService> AdviseServices { get; set; } = new List<AdviseService>();
    }
}