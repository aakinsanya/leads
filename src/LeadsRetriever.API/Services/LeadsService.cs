using API.Data;
using API.Data.DbModel;
using API.Models;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LeadsService : ILeadsService
    {
        private readonly LeadsDbContext _dbContext;

        public LeadsService(LeadsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LeadDto?> GetLeadByEmailAsync(string email)
        {
            return await _dbContext.Leads
                .Where(dbLead => dbLead.Email == email)
                .Select(dbLead => new LeadDto
                {
                    Id = dbLead.SourceId,
                    Email = dbLead.Email,
                    Name = dbLead.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LeadDto>> GetLeadsAsync(int pageNumber, int pageSize, DateTime startDate, DateTime endDate)
        {
            if (pageNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }
            if (startDate >= endDate)
            {
                throw new ArgumentException("startDate must be before endDate.");
            }

            var query = _dbContext.Leads.AsNoTracking();
            query = query.Where(lead => lead.AddedDate >= startDate && lead.AddedDate <= endDate)
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize);

            return await query.Select(dbLead => new LeadDto { Email = dbLead.Email, Name = dbLead.Name }).ToListAsync();
        }

        public async Task SaveLeadAsync(LeadDto leadDto)
        {
            var lead = new Lead { Email = leadDto.Email, Name = leadDto.Name, SourceId = leadDto.Id, SourceCreatedDate = leadDto.DateCreated, AddedDate = DateTime.UtcNow };
            await BulkInsertOrUpdateAsync([lead]);
        }

        public async Task SaveLeadsBulkAsync(IEnumerable<LeadDto> leadsDto)
        {
            var leads = leadsDto.Select(leadDto => new Lead { Email = leadDto.Email, Name = leadDto.Name, SourceId = leadDto.Id, SourceCreatedDate = leadDto.DateCreated, AddedDate = DateTime.UtcNow });
            await BulkInsertOrUpdateAsync(leads);
        }

        private async Task BulkInsertOrUpdateAsync(IEnumerable<Lead> leads)
        {
            await _dbContext.BulkInsertOrUpdateAsync(leads, b => b.PropertiesToIncludeOnUpdate = [""]);
        }

    }
}
