namespace Ambev.DeveloperEvaluation.WebApi.Common;

public sealed class PagedResult<T>
{
    public IEnumerable<T> Data { get; init; } = [];
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
}

public static class PagedResultFactory
{
    public static PagedResult<T> Create<T>(IEnumerable<T> source, int page, int size)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        var all = source.ToList();

        return new PagedResult<T>
        {
            Data = all.Skip((page - 1) * size).Take(size),
            TotalItems = all.Count,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(all.Count / (double)size)
        };
    }
}

public static class TextFilter
{
    public static bool Matches(string value, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return true;

        var startsWithWildcard = filter.StartsWith('*');
        var endsWithWildcard = filter.EndsWith('*');
        var normalizedFilter = filter.Trim('*');

        return (startsWithWildcard, endsWithWildcard) switch
        {
            (true, true) => value.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase),
            (true, false) => value.EndsWith(normalizedFilter, StringComparison.OrdinalIgnoreCase),
            (false, true) => value.StartsWith(normalizedFilter, StringComparison.OrdinalIgnoreCase),
            _ => value.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase)
        };
    }
}
