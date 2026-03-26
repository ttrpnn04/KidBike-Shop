using System;
using System.Collections.Generic;

namespace CSI402_Project.Models.Db;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? AgeRange { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
