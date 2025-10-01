namespace SlipVerification.Shared.Results;

/// <summary>
/// Represents a paginated result
/// </summary>
/// <typeparam name="T">The type of the items</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
    /// <summary>
    /// Gets or sets the current page number
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of items
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Gets a value indicating whether there is a previous page
    /// </summary>
    public bool HasPrevious => PageNumber > 1;
    
    /// <summary>
    /// Gets a value indicating whether there is a next page
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}
