using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class MapConfig
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid ImageId { get; set; }

    public virtual MapImage Image { get; set; } = null!;

    public virtual ICollection<MapComponent> MapComponents { get; set; } = new List<MapComponent>();
}
