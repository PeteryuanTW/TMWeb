using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class Machine
{
    public Guid Id { get; set; }

    public Guid? ProcessId { get; set; }

    public string Name { get; set; } = null!;

    public string Ip { get; set; } = null!;

    public int Port { get; set; }

    public int ConnectionType { get; set; }

    public Guid? TagCategoryId { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<MapComponent> MapComponents { get; set; } = new List<MapComponent>();

    public virtual Process? Process { get; set; }

    public virtual TagCategory? TagCategory { get; set; }
}
