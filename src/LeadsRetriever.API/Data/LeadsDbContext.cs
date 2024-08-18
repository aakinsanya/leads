using API.Data.DbModel;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LeadsDbContext: DbContext
    {
        public DbSet<Lead> Leads { get; set; }
        public DbSet<LeadsRetrievalLog> LeadsRetrievalLogs { get; set; }

        public LeadsDbContext(DbContextOptions<LeadsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the Lead entity
            modelBuilder.Entity<Lead>(entity =>
            {
                entity.HasKey(e => e.Email);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Email)
                      .IsRequired()                      
                      .HasMaxLength(100);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(256);

                entity.Property(e => e.SourceCreatedDate)
                      .HasColumnType("datetime")
                      .IsRequired(false);

                entity.Property(e => e.SourceId)
                      .IsRequired(false);

                entity.Property(e => e.AddedDate)
                    .HasColumnType("datetime")
                    .IsRequired(false)
                    .HasDefaultValueSql("GETDATE()");
            });


            // Configuring the LeadsRetrievalLog entity
            modelBuilder.Entity<LeadsRetrievalLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.LastRunTime);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LastRunTime)
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.Property(e => e.Status)
                      .IsRequired();
            });

        }
    }
}
