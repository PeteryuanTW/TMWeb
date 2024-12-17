using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class WorkorderRecipeConfig
{
    public Guid Id { get; set; }
    [Required]
    public string RecipeCategory { get; set; }

    public virtual ICollection<RecipeItem> Recipes { get; set; } = new List<RecipeItem>();

    public virtual ICollection<Workorder> Workorders { get; set; } = new List<Workorder>();
}
