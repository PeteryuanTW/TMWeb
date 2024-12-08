using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class Process
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; }

    //public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();

    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();

    public virtual ICollection<Workorder> Workorders { get; set; } = new List<Workorder>();
}
