using API.Models;
using API.Services;
using Leads.Tests.Data;

namespace Leads.Tests
{
    public class LeadsServiceTests
    {
        private readonly TestLeadsDbContext _context;
        private readonly LeadsService _leadsService;

        public LeadsServiceTests()
        {
            _context = TestLeadsDbContext.Create();
            _leadsService = new LeadsService(_context);
        }

        [Fact]
        public async Task GetLeadByEmailAsync_ShouldReturnCorrectLead()
        {
            // Arrange
            var email = "lead1@test.com";

            // Act
            var result = await _leadsService.GetLeadByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal("Lead 1", result.Name);
        }

        [Fact]
        public async Task GetLeadsAsync_ShouldFilterAndReturnLeadsInRange()
        {
            // Act
            var result = await _leadsService.GetLeadsAsync(1, 10, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

            // Assert
            Assert.Equal(2, result.Count());
        }


        [Fact]
        public async Task GetLeadsAsync_ShouldThrowArgumentOutOfRangeException_WhenPageNumberIsZeroOrNegative()
        {
            // Arrange
            int invalidPageNumber = 0; // or a negative number
            int pageSize = 10;
            DateTime startDate = DateTime.UtcNow.AddDays(-1);
            DateTime endDate = DateTime.UtcNow;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _leadsService.GetLeadsAsync(invalidPageNumber, pageSize, startDate, endDate)
            );

            Assert.Equal("pageNumber", exception.ParamName);
        }


        [Fact]
        public async Task GetLeadsAsync_ShouldThrowArgumentOutOfRangeException_WhenPageSizeIsZeroOrNegative()
        {
            // Arrange
            int pageNumber = 1;
            int invalidPageSize = 0; // or a negative number
            DateTime startDate = DateTime.UtcNow.AddDays(-1);
            DateTime endDate = DateTime.UtcNow;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _leadsService.GetLeadsAsync(pageNumber, invalidPageSize, startDate, endDate)
            );

            Assert.Equal("pageSize", exception.ParamName);
        }

        [Fact]
        public async Task SaveLeadAsync_ShouldAddNewLeadToTheDatabase()
        {
            // Arrange
            var leadDto = new LeadDto
            {
                Email = "lead5@test.com",
                Name = "Lead 5",
                Id = "5",
                DateCreated = DateTime.UtcNow
            };

            // Act
            await _leadsService.SaveLeadAsync(leadDto);
            var result = await _leadsService.GetLeadByEmailAsync(leadDto.Email);

            // Assert
            Assert.Equal("Lead 5", result?.Name);
        }

        [Fact]
        public async Task SaveLeadsBulkAsync_ShouldAddMultipleNewLeadsToTheDatabase()
        {
            // Arrange
            var leadDto = new List<LeadDto>
            {
                new LeadDto { 
                    Email = "lead6@test.com",
                    Name = "Lead 6",
                    Id = "6",
                    DateCreated = DateTime.UtcNow
                },
                new LeadDto {
                    Email = "lead7@test.com",
                    Name = "Lead 7",
                    Id = "7",
                    DateCreated = DateTime.UtcNow
                }
            };

            // Act
            await _leadsService.SaveLeadsBulkAsync(leadDto);
            var result1 = await _leadsService.GetLeadByEmailAsync("lead6@test.com");
            var result2 = await _leadsService.GetLeadByEmailAsync("lead7@test.com");

            // Assert
            Assert.Equal("Lead 6", result1?.Name);
            Assert.Equal("Lead 7", result2?.Name);
        }

        [Fact]
        public async Task SaveLeads_ShouldInsertOnlyOne_WhenDuplicatesExists()
        {
            // Arrange
            var leadDto = new List<LeadDto>
            {
                new LeadDto {
                    Email = "lead8@test.com",
                    Name = "Lead 8",
                    Id = "8",
                    DateCreated = DateTime.UtcNow
                },
                new LeadDto {
                    Email = "lead8@test.com",
                    Name = "Lead 8",
                    Id = "8",
                    DateCreated = DateTime.UtcNow
                }
            };

            // Act
            await _leadsService.SaveLeadsBulkAsync(leadDto);
            var result = await _leadsService.GetLeadByEmailAsync("lead8@test.com");

            // Assert
            Assert.Equal("Lead 8", result?.Name);
        }
    }
}
