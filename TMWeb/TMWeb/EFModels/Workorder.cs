using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TMWeb.Data.CustomAttribute;

namespace TMWeb.EFModels;

public partial class Workorder
{
    public Guid Id { get; set; }

    [Required]
    [PublicProperty]
    public string WorkorderNo { get; set; } = null!;
    [Required]
    [PublicProperty]
    public string Lot { get; set; } = null!;

    public int Status { get; set; }

    public Guid? RecipeCategoryId { get; set; }

    public Guid? WorkorderRecordCategoryId { get; set; }

    public Guid? ItemRecordsCategoryId { get; set; }

    public Guid? TaskRecordCategoryId { get; set; }

    [Required]
    public Guid? ProcessId { get; set; }
    [PublicProperty]
    public string? PartNo { get; set; }
    [PublicProperty]
    [Range(1, Int16.MaxValue)]
    public int TargetAmount { get; set; }
    public int Okamount { get; set; }

    public int Ngamount { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public DateTime CreateTime { get; set; }

    public virtual ICollection<ItemDetail> ItemDetails { get; set; } = new List<ItemDetail>();

    public virtual ItemRecordConfig? ItemRecordsCategory { get; set; }

    public virtual Process Process { get; set; } = null!;

    public virtual WorkorderRecipeConfig? RecipeCategory { get; set; }

    public virtual TaskRecordConfig? TaskRecordCategory { get; set; }

    public virtual WorkorderRecordConfig? WorkorderRecordCategory { get; set; }

    public virtual ICollection<WorkorderRecordDetail> WorkorderRecordDetails { get; set; } = new List<WorkorderRecordDetail>();
}
