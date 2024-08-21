using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class MapComponent
{
    public Guid Id { get; set; }

    public int Type { get; set; }

    public Guid MapId { get; set; }

    public int PositionX { get; set; }

    public int PositionY { get; set; }

    public int Height { get; set; }

    public int Width { get; set; }

    public virtual MapConfig Map { get; set; } = null!;
}
