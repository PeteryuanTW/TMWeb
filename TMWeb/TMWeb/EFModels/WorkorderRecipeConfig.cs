using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class WorkorderRecipeConfig
{
    public Guid Id { get; set; }

    public string? RecipeCategory { get; set; }

    public virtual ICollection<Workerder> Workerders { get; set; } = new List<Workerder>();

    public virtual ICollection<WorkorderRecipeContent> WorkorderRecipeContents { get; set; } = new List<WorkorderRecipeContent>();
}
