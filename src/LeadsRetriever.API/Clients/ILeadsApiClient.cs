using API.Models;

namespace API.Clients
{
    /// <summary>
    /// Defines the contract for a client that interacts with an external leads API.
    /// </summary>
    /// <remarks>
    /// The <c>ILeadsApiClient</c> interface provides methods for retrieving lead data from an external API.
    /// Implementations of this interface are expected to handle HTTP communications and data deserialization.
    /// This interface is designed to abstract the communication with the leads API, enabling different
    /// implementations or mocking in unit tests.
    /// </remarks>
    public interface ILeadsApiClient
    {
        /// <summary>
        /// Retrieves a paginated list of leads from the external API.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of leads per page.</param>
        /// <param name="startDate">The optional start date to filter leads by their creation date.</param>
        /// <param name="endDate">The optional end date to filter leads by their creation date.</param>
        /// <returns>A collection of <c>LeadDto</c> objects representing the leads.</returns>
        Task<IEnumerable<LeadDto>> GetLeadsAsync(int pageNumber, int pageSize, DateTime? startDate = null, DateTime? endDate = null);
    }
}
