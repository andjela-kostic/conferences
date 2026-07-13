using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ConferenceRepository : IConferenceRepository
{
    private readonly AppDbContext _context;

    public ConferenceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Conference>> GetAllWithTopicsAsync(CancellationToken cancellationToken = default)
    {
        return _context.Conferences
            .Include(c => c.Topics)
            .ToListAsync(cancellationToken);
    }

    public Task<Conference?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Conferences.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public void Add(Conference conference)
    {
        _context.Conferences.Add(conference);
    }

    public void Remove(Conference conference)
    {
        _context.Conferences.Remove(conference);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

