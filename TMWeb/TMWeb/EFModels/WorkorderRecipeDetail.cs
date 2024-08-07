using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class WorkorderRecipeDetail
{
    public Guid WorkerderId { get; set; }

    public Guid RecipeContentId { get; set; }

    public string? Value { get; set; }

    public virtual WorkorderRecipeContent RecipeContent { get; set; } = null!;

    public virtual Workerder Workerder { get; set; } = null!;
}
