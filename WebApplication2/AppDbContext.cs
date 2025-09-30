using Microsoft.EntityFrameworkCore;
namespace Models;



    public class AppDbContext : DbContext
    {
        public DbSet<CommentAttachment> CommentAttachments { get; set; } = null!;
        public DbSet<Defect> Defects { get; set; } = null!;
        public DbSet<DefectAttachment> DefectAttachments { get; set; } = null!;
        public DbSet<DefectComment> DefectComments { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }

