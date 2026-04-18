using CSI402_Project.Models.Db;

namespace CSI402_Project.ViewModels;

public class DashboardViewModel
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public int TotalPromotions { get; set; }
    public List<Order> RecentOrders { get; set; } = new List<Order>();
    public List<Product> LowStockProducts { get; set; } = new List<Product>();
}
