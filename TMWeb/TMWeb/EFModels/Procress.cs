using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class Procress
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Station> Stations { get; set; } = new List<Station>();

    public virtual ICollection<Workerder> Workerders { get; set; } = new List<Workerder>();
}
