using API.Data.DbModel;
using API.Models;
using API.Services;
using API.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API;

public static class Endpoints
{
    /// <summary>
    /// Configures the routing for lead-related API endpoints by mapping them to their respective handlers.
    /// </summary>
    public static void MapLeadEndpoints(this IEndpointRouteBuilder routes)
    {
        var leadsGroup = routes.MapGroup("/api/leads").WithTags(nameof(Lead));

        /// <summary>
        /// Retrieves a paginated list of leads within a specified date range.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of leads per page.</param>
        /// <param name="startDate">The start date to filter leads by their added date.</param>
        /// <param name="endDate">The end date to filter leads by their added date.</param>
        /// <param name="leadService">The service responsible for lead management operations.</param>
        /// <returns>A paginated list of leads within the specified date range.</returns>
        leadsGroup.MapGet("/", async Task<Results<Ok<IEnumerable<LeadDto>>, BadRequest<string>>> ([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromServices] ILeadsService leadService) =>
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return TypedResults.BadRequest("Page number and page size must be greater than zero.");
            }
            var leads = await leadService.GetLeadsAsync(pageNumber, pageSize, startDate, endDate);
            return TypedResults.Ok(leads);
        })
        .WithName("GetLeads")
        .WithOpenApi();

        /// <summary>
        /// Retrieves a lead by their email address.
        /// </summary>
        /// <param name="email">The email address of the lead to retrieve.</param>
        /// <param name="leadService">The service responsible for lead management operations.</param>
        /// <returns>The lead associated with the specified email address, or a 404 status if not found.</returns>
        leadsGroup.MapGet("/{email}", async Task<Results<Ok<LeadDto>, BadRequest<string>, NotFound>> (string email, [FromServices] ILeadsService leadService) =>
        {
            if (!email.IsValidEmail())
            {
                return TypedResults.BadRequest("Invalid email format.");
            }
            var lead = await leadService.GetLeadByEmailAsync(email);
            if (lead == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(lead);
        })
        .WithName("GetLeadByEmail")
        .WithOpenApi();

        /// <summary>
        /// Processes a new lead received via a webhook.
        /// </summary>
        /// <param name="payload">The payload received from the webhook, containing lead data.</param>
        /// <param name="leadsService">The service responsible for saving lead data.</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <returns>A result indicating success or failure of the operation.</returns>
        routes.MapPost("/webhook/leads", async ([FromBody] WebhookPayload payload, [FromServices] ILeadsService leadsService, [FromServices] ILogger<Program> logger) =>
        {
            if (payload == null || payload.Action != "lead.added" || payload.Lead == null)
            {
                logger.LogWarning("Received invalid webhook payload.");
                return Results.BadRequest("Invalid payload.");
            }

            try
            {
                await leadsService.SaveLeadAsync(payload.Lead);
                logger.LogInformation("Successfully processed lead with ID {LeadId} via webhook.", payload.Lead.Id);
                return Results.Ok("Success");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process lead with ID {LeadId} via webhook.", payload.Lead.Id);
                return Results.StatusCode(500);
            }
        })
        .WithName("NewLeadWebhook")
        .WithOpenApi();
    }
}
