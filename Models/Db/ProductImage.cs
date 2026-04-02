using System;

namespace CSI402_Project.Models.Db;

public partial class ProductImage
{
    public int ProductImageId { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int DisplayOrder { get; set; } = 0;

    public virtual Product Product { get; set; } = null!;
}
