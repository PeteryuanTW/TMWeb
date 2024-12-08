using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class Tag
{
    public Guid? Id { get; set; }

    public Guid CategoryId { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    [Range(1, 44)]
    public int DataType { get; set; }

    public bool UpdateByTime { get; set; }

    public int SpecialType { get; set; }

    public bool Bool1 { get; set; }

    public bool Bool2 { get; set; }

    public bool Bool3 { get; set; }

    public bool Bool4 { get; set; }

    public bool Bool5 { get; set; }

    public int Int1 { get; set; }

    public int Int2 { get; set; }

    public int Int3 { get; set; }

    public int Int4 { get; set; }

    public int Int5 { get; set; }

    public string? String1 { get; set; }

    public string? String2 { get; set; }

    public string? String3 { get; set; }

    public string? String4 { get; set; }

    public string? String5 { get; set; }

    public virtual TagCategory Category { get; set; } = null!;

    //public virtual ICollection<WorkorderRecipeContent> WorkorderRecipeContents { get; set; } = new List<WorkorderRecipeContent>();
}
