using CSI402_Project.Models.Db;

namespace CSI402_Project.ViewModels;

public class ProductFormViewModel
{
    public Product Product { get; set; } = new Product();
    public List<Category> Categories { get; set; } = new List<Category>();
}
