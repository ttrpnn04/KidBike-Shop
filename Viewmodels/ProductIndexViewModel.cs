using CSI402_Project.Models.Db;

namespace CSI402_Project.ViewModels;

public class ProductIndexViewModel
{
    public List<Product> Products { get; set; } = new List<Product>();
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? PriceRange { get; set; }
    public string SortBy { get; set; } = "default";
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
    public string? CategoryName { get; set; }
}
