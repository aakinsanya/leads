using API.Data;
using API.Data.DbModel;
using Microsoft.EntityFrameworkCore;

namespace Leads.Tests.Data
{
    internal class TestLeadsDbContext: LeadsDbContext
    {
        internal TestLeadsDbContext(DbContextOptions<LeadsDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        internal static TestLeadsDbContext Create()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LeadsDbContext>();            
            optionsBuilder.UseSqlite("DataSource=:memory:");
            var context = new TestLeadsDbContext(optionsBuilder.Options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data specifically for testing
            modelBuilder.Entity<Lead>().HasData(
                new Lead
                {
                    Email = "lead1@test.com",
                    Name = "Lead 1",                    
                    AddedDate = DateTime.UtcNow.AddMinutes(-120)
                },
                new Lead
                {
                    Email = "lead2@test.com",
                    Name = "Lead 2",
                    AddedDate = DateTime.UtcNow.AddMinutes(-120)
                },
                new Lead
                {
                    Email = "lead3@test.com",
                    Name = "Lead 3",
                    AddedDate = DateTime.UtcNow.AddDays(-2)
                },
                new Lead
                {
                    Email = "lead4@test.com",
                    Name = "Lead 4",
                    AddedDate = DateTime.UtcNow.AddDays(-2)
                }
            );
        }
    }
}
