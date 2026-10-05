namespace Ambev.DeveloperEvaluation.Common.Persistence;

public sealed record PageQuery(int Page = 1, int Size = 10, string? Order = null)
{
    public int NormalizedPage => Math.Max(1, Page);
    public int NormalizedSize => Math.Clamp(Size, 1, 100);
    public int Offset => (NormalizedPage - 1) * NormalizedSize;
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }

    public static PagedResult<T> Create(IReadOnlyList<T> data, int totalItems, PageQuery page) => new()
    {
        Data = data,
        TotalItems = totalItems,
        CurrentPage = page.NormalizedPage,
        TotalPages = (int)Math.Ceiling(totalItems / (double)page.NormalizedSize)
    };
}
