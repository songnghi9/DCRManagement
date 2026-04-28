namespace DCRManagement.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public static PagedResult<T> Create(IEnumerable<T> allItems, int page, int pageSize)
    {
        var list = allItems.ToList();
        return new PagedResult<T>
        {
            TotalCount = list.Count,
            Page = page,
            PageSize = pageSize,
            Items = list.Skip((page - 1) * pageSize).Take(pageSize)
        };
    }
}