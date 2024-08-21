using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class Workorder
{
    public Guid Id { get; set; }

    public string WorkorderNo { get; set; } = null!;

    public string Lot { get; set; } = null!;

    public int Status { get; set; }

    public Guid? RecipeCategoryId { get; set; }

    public Guid? WorkorderRecordCategoryId { get; set; }

    public Guid? ItemRecordsCategoryId { get; set; }

    public Guid? TaskRecordCategoryId { get; set; }

    public Guid ProcessId { get; set; }

    public string? PartNo { get; set; }

    public int TargetAmount { get; set; }

    public int Okamount { get; set; }

    public int Ngamount { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public virtual ICollection<ItemDetail> ItemDetails { get; set; } = new List<ItemDetail>();

    public virtual ItemRecordConfig? ItemRecordsCategory { get; set; }

    public virtual Process Process { get; set; } = null!;

    public virtual WorkorderRecipeConfig? RecipeCategory { get; set; }

    public virtual TaskRecordConfig? TaskRecordCategory { get; set; }

    public virtual WorkorderRecordConfig? WorkorderRecordCategory { get; set; }

    public virtual ICollection<WorkorderRecordDetail> WorkorderRecordDetails { get; set; } = new List<WorkorderRecordDetail>();
}
