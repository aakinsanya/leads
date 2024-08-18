using API.Models;

namespace API.Services
{
    /// <summary>
    /// Defines a contract for managing lead data operations within the system.
    /// </summary>
    /// <remarks>
    /// The <c>ILeadsService</c> interface provides methods for retrieving and storing lead information.
    /// It includes operations for fetching individual leads by email, retrieving paginated lists of leads,
    /// and saving leads both individually and in bulk. Implementations of this interface should handle
    /// the necessary business logic and data access to ensure that lead data is accurately managed.
    /// </remarks>
    public interface ILeadsService
    {
        /// <summary>
        /// Retrieves a lead by their email address.
        /// </summary>
        /// <param name="email">The email address of the lead to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <c>LeadDto</c> if found, or <c>null</c> if not.</returns>
        Task<LeadDto?> GetLeadByEmailAsync(string email);

        /// <summary>
        /// Retrieves a paginated list of leads within a specified date range.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of leads per page.</param>
        /// <param name="startDate">The start date for filtering leads by their added date.</param>
        /// <param name="endDate">The end date for filtering leads by their added date.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <c>LeadDto</c> objects.</returns>
        Task<IEnumerable<LeadDto>> GetLeadsAsync(int pageNumber, int pageSize, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Saves a single lead to the database.
        /// </summary>
        /// <param name="lead">The lead data transfer object containing information about the lead to be saved.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SaveLeadAsync(LeadDto lead);

        /// <summary>
        /// Saves multiple leads to the database in a bulk operation.
        /// </summary>
        /// <param name="leads">A collection of lead data transfer objects to be saved.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task SaveLeadsBulkAsync(IEnumerable<LeadDto> leads);
    }
}
