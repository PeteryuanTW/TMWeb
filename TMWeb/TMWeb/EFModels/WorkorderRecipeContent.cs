using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class WorkorderRecipeContent
{
    public Guid Id { get; set; }

    public Guid? ConfigId { get; set; }

    public string? RecipeName { get; set; }

    public int? DataType { get; set; }

    public string? Value { get; set; }

    public virtual WorkorderRecipeConfig? Config { get; set; }
}
