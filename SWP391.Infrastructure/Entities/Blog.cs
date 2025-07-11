using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWP391.Infrastructure.Entities
{
    [Table("Blog")]
    public partial class Blog
    {
        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string? Title { get; set; }
        [StringLength(255)]
        public string? Content { get; set; }
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // Foreign key to User
        public int UserId { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}