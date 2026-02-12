namespace Beyond8.Sale.Application.Dtos.Carts;

/// <summary>
/// Request to check which courses are already in user's cart.
/// Used by UI to determine whether to show "Add to Cart" or "Already Added" button.
/// Performance optimized: Only checks specific courseIds instead of loading entire cart.
/// </summary>
public class CheckCoursesInCartRequest
{
    /// <summary>
    /// List of course IDs to check against user's cart.
    /// Typically contains courses from current page (e.g., 20 courses per page).
    /// </summary>
    public List<Guid> CourseIds { get; set; } = [];
}
