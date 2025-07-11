using Microsoft.EntityFrameworkCore;
using SWP391.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<OTP> OTPs { get; set; } = null!;
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<AdviseService> AdviseServices { get; set; } = null!;
        public DbSet<TestService> TestServices { get; set; } = null!;
        public DbSet<AdviseNote> AdviseNotes { get; set; } = null!;
        public DbSet<TestResult> TestResult { get; set; } = null!;
        public DbSet<MenstrualCycle> MenstrualCycles { get; set; } = null!;
        public DbSet<PillIntakeCycle> PillIntakeCycles { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configuration can go here
        }
    }
}
