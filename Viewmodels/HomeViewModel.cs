using CSI402_Project.Models.Db;

namespace CSI402_Project.ViewModels;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new List<Category>();
    public List<Product> PopularProducts { get; set; } = new List<Product>();
    public List<Promotion> Promotions { get; set; } = new List<Promotion>();
}
