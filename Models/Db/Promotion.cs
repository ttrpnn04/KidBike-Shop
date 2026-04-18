using System;
using System.Collections.Generic;

namespace CSI402_Project.Models.Db;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? Name { get; set; }

    public string? Type { get; set; }

    public decimal? DiscountValue { get; set; }

    public string? ConditionType { get; set; }

    public decimal? ConditionAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
