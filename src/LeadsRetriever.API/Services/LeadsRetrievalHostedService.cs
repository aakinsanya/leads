using API.Clients;
using API.Data;
using API.Data.DbModel;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    /// <summary>
    /// A hosted service responsible for periodically retrieving lead data from an external API
    /// and saving it to the database. This service runs in the background and automatically polls
    /// the API at configured intervals to ensure that the lead data is kept up-to-date.
    /// </summary>
    /// <remarks>
    /// The <c>LeadsRetrievalHostedService</c> uses an <c>ILeadsApiClient</c> to fetch the latest leads
    /// from the external source and processes them by saving them to the database. The service runs at
    /// regular intervals defined by the configuration and handles errors gracefully by logging them
    /// and attempting to continue operations during the next scheduled run.
    public class LeadsRetrievalHostedService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILeadsApiClient _leadsApiClient;
        private readonly ILogger<LeadsRetrievalHostedService> _logger;
        private readonly int _pollIntervalMinutes;
        private readonly int _pageSize;
        private Timer? _timer;

        public LeadsRetrievalHostedService(IServiceScopeFactory serviceScopeFactory, ILeadsApiClient leadsApiClient, ILogger<LeadsRetrievalHostedService> logger, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _leadsApiClient = leadsApiClient;
            _logger = logger;
            _pollIntervalMinutes = configuration.GetValue<int>("LeadsApi:PollIntervalMinutes", 60); // Default to 60 minutes
            _pageSize = configuration.GetValue<int>("LeadsApi:PageSize", 100); // Default to 100
        } 

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LeadsRetrievalHostedService is starting.");
            _timer = new Timer(async _ => await PollAndProcessLeadsAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(_pollIntervalMinutes));
            return Task.CompletedTask;

        }

        private async Task PollAndProcessLeadsAsync()
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Polling for new leads at {Time}.", startTime);
            
            try
            {
                var lastRunTime = await GetLastRunTimeAsync();
                lastRunTime ??= DateTime.UtcNow.AddMinutes(-_pollIntervalMinutes); // Use poll interval as default last run time

                int pageNumber = 1;

                List<LeadDto> allLeads = new List<LeadDto>();

                while (true)
                {
                    var leads = await _leadsApiClient.GetLeadsAsync(pageNumber, _pageSize, lastRunTime); // Use last run time as start time

                    if (leads == null || !leads.Any())
                    {
                        break;
                    }

                    allLeads.AddRange(leads);

                    if (leads.Count() < _pageSize)
                    {
                        break;
                    }

                    pageNumber++;
                }

                if (allLeads.Any())
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var leadsService = scope.ServiceProvider.GetRequiredService<ILeadsService>();

                        await leadsService.SaveLeadsBulkAsync(allLeads);
                    }
                    _logger.LogInformation("Processed {Count} leads from {StartDate}", allLeads.Count, lastRunTime);
                }
                else
                {
                    _logger.LogInformation("No new leads found from {StartDate}", lastRunTime);
                }
                await UpdateLastRunTimeAsync(startTime, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while polling and processing leads.");
                await UpdateLastRunTimeAsync(startTime, "Failed");
            }
        }

        private async Task UpdateLastRunTimeAsync(DateTime startTime, string status)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LeadsDbContext>();
            await dbContext.LeadsRetrievalLogs.AddAsync(new LeadsRetrievalLog { LastRunTime = startTime, Status = status });
            await dbContext.SaveChangesAsync();
        }

        private async Task<DateTime?> GetLastRunTimeAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LeadsDbContext>();
            return await dbContext.LeadsRetrievalLogs.Where(log => log.Status == "Success").MaxAsync(log => log.LastRunTime);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("LeadsRetrievalHostedService is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
