using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class TagCategory
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public int ConnectionType { get; set; }

    public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    //public virtual ICollection<WorkorderRecipeContent> WorkorderRecipeContents { get; set; } = new List<WorkorderRecipeContent>();
}
