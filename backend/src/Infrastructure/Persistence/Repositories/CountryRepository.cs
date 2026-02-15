using Microsoft.EntityFrameworkCore;
using SaaS.Domain.Entities;
using SaaS.Domain.Interfaces;
using SaaS.Infrastructure.Persistence;

namespace SaaS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Country entity
/// </summary>
public class CountryRepository : ICountryRepository
{
    private readonly ApplicationDbContext _context;

    public CountryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await _context.Countries
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Country?> GetByCodeAsync(string code)
    {
        return await _context.Countries
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper());
    }

    public async Task<Country?> GetByIdAsync(Guid id)
    {
        return await _context.Countries
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Country>> GetActiveCountriesAsync()
    {
        return await _context.Countries
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
