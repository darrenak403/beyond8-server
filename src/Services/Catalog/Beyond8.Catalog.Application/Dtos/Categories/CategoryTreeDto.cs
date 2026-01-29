namespace Beyond8.Catalog.Application.Dtos.Categories
{
    public class CategoryTreeDto : CategorySimpleResponse
    {
        public int Level { get; set; } = 0;
        public List<CategoryTreeDto> SubCategories { get; set; } = [];
    }

}
