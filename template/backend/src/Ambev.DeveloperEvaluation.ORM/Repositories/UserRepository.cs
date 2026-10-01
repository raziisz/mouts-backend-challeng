using Ambev.DeveloperEvaluation.Common.Persistence;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Queries;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DefaultContext _context;

    public UserRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

    public Task<PagedResult<User>> ListAsync(UserListQuery request, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Username)) query = ApplyTextFilter(query, request.Username, useUsername: true);
        if (!string.IsNullOrWhiteSpace(request.Email)) query = ApplyTextFilter(query, request.Email, useUsername: false, useEmail: true);
        if (!string.IsNullOrWhiteSpace(request.Phone)) query = ApplyTextFilter(query, request.Phone, useUsername: false, useEmail: false);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.Role.HasValue) query = query.Where(x => x.Role == request.Role.Value);

        query = ApplyOrder(query, request.Page.Order);
        return query.ToPagedResultAsync(request.Page, cancellationToken);
    }

    private static IQueryable<User> ApplyTextFilter(IQueryable<User> query, string filter, bool useUsername, bool useEmail = false)
    {
        var normalized = filter.Trim('*').ToLower();
        var startsWithWildcard = filter.StartsWith('*');
        var endsWithWildcard = filter.EndsWith('*');

        if (startsWithWildcard && endsWithWildcard)
            return useUsername ? query.Where(x => x.Username.ToLower().Contains(normalized)) : useEmail ? query.Where(x => x.Email.ToLower().Contains(normalized)) : query.Where(x => x.Phone.ToLower().Contains(normalized));
        if (startsWithWildcard)
            return useUsername ? query.Where(x => x.Username.ToLower().EndsWith(normalized)) : useEmail ? query.Where(x => x.Email.ToLower().EndsWith(normalized)) : query.Where(x => x.Phone.ToLower().EndsWith(normalized));
        if (endsWithWildcard)
            return useUsername ? query.Where(x => x.Username.ToLower().StartsWith(normalized)) : useEmail ? query.Where(x => x.Email.ToLower().StartsWith(normalized)) : query.Where(x => x.Phone.ToLower().StartsWith(normalized));
        return useUsername ? query.Where(x => x.Username.ToLower() == normalized) : useEmail ? query.Where(x => x.Email.ToLower() == normalized) : query.Where(x => x.Phone.ToLower() == normalized);
    }

    private static IQueryable<User> ApplyOrder(IQueryable<User> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) return query.OrderBy(x => x.Id);
        IOrderedQueryable<User>? result = null;
        foreach (var part in order.Replace("\"", string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var desc = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            var field = tokens[0].ToLowerInvariant();
            result = result is null ? Order(query, field, desc) : ThenOrder(result, field, desc);
        }
        return result ?? query.OrderBy(x => x.Id);
    }

    private static IOrderedQueryable<User> Order(IQueryable<User> query, string field, bool desc) => field switch
    {
        "username" => desc ? query.OrderByDescending(x => x.Username) : query.OrderBy(x => x.Username),
        "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
        "phone" => desc ? query.OrderByDescending(x => x.Phone) : query.OrderBy(x => x.Phone),
        "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
        "role" => desc ? query.OrderByDescending(x => x.Role) : query.OrderBy(x => x.Role),
        _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
    };

    private static IOrderedQueryable<User> ThenOrder(IOrderedQueryable<User> query, string field, bool desc) => field switch
    {
        "username" => desc ? query.ThenByDescending(x => x.Username) : query.ThenBy(x => x.Username),
        "email" => desc ? query.ThenByDescending(x => x.Email) : query.ThenBy(x => x.Email),
        "phone" => desc ? query.ThenByDescending(x => x.Phone) : query.ThenBy(x => x.Phone),
        "status" => desc ? query.ThenByDescending(x => x.Status) : query.ThenBy(x => x.Status),
        "role" => desc ? query.ThenByDescending(x => x.Role) : query.ThenBy(x => x.Role),
        _ => desc ? query.ThenByDescending(x => x.Id) : query.ThenBy(x => x.Id)
    };

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
