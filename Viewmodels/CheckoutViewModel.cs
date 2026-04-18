using CSI402_Project.Models.Db;

namespace CSI402_Project.ViewModels;

public class CheckoutViewModel
{
    public List<Cart> CartItems { get; set; } = new List<Cart>();
    public User User { get; set; } = new User();

    // Promotion fields
    public string? PromoCode { get; set; }
    public int? AppliedPromotionId { get; set; }
    public Promotion? AppliedPromotion { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal Subtotal => CartItems.Sum(c => (c.Product?.Price ?? 0) * (c.Quantity ?? 0));
    public decimal TotalAmount => Subtotal - DiscountAmount;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}
