using SaaS.Domain.Entities;

namespace SaaS.Domain.Interfaces;

/// <summary>
/// Repository interface for Country entity
/// </summary>
public interface ICountryRepository
{
    /// <summary>
    /// Get all countries
    /// </summary>
    Task<IEnumerable<Country>> GetAllAsync();

    /// <summary>
    /// Get country by ISO alpha-2 code
    /// </summary>
    Task<Country?> GetByCodeAsync(string code);

    /// <summary>
    /// Get country by ID
    /// </summary>
    Task<Country?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all active countries
    /// </summary>
    Task<IEnumerable<Country>> GetActiveCountriesAsync();
}
