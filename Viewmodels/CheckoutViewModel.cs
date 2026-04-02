using CSI402_Project.Models.Db;

namespace CSI402_Project.ViewModels;

public class CheckoutViewModel
{
    public List<Cart> CartItems { get; set; } = new List<Cart>();
    public User User { get; set; } = new User();
}
