namespace SlipVerification.Application.DTOs;

/// <summary>
/// Represents a paged result with items and pagination metadata
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the items in the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    
    /// <summary>
    /// Gets or sets the total count of items across all pages
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Gets or sets the current page number
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Gets whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    /// <summary>
    /// Gets whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
