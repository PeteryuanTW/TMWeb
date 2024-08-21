using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class MapImage
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public byte[] DataByte { get; set; } = null!;

    public string DataType { get; set; } = null!;

    public virtual ICollection<MapConfig> MapConfigs { get; set; } = new List<MapConfig>();
}
