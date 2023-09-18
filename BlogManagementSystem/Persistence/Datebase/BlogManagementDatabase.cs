using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Datebase
{
    public class BlogManagementDatabase : DbContext
    {
        public BlogManagementDatabase(DbContextOptions<BlogManagementDatabase> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
        public DbSet<Blog> Blog { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Likes> Like { get; set; }
        public DbSet<Tag> Tag { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Likes>()
                .HasKey(x => new { x.UserId, x.BlogId });

            modelBuilder.Entity<Tag>()
                .HasKey(x => new { x.UserId, x.BlogId });

            modelBuilder.Entity<Blog>()
                .HasOne(x => x.User);
        }
    }
}
