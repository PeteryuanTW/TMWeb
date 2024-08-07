using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class TagCategory
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public int? ConnectionType { get; set; }

    public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
