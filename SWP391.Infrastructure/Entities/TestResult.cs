using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace SWP391.Infrastructure.Entities
{
    [Table("TestResult")]
    public partial class TestResult
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string? Result { get; set; }
        [StringLength(255)]
        public string? Suggestion { get; set; }
        [StringLength(255)]
        public string? Note { get; set; }

        // Navigation property cho TestServices
        public ICollection<TestService> TestServices { get; set; } = new List<TestService>();
    }
}