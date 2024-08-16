using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class Process
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();

    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();

    public virtual ICollection<Workorder> Workorders { get; set; } = new List<Workorder>();
}
