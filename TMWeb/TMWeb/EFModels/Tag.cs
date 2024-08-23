using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class Tag
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public int DataType { get; set; }

    public bool UpdateByTime { get; set; }

    public bool IsHeartBeat { get; set; }

    public string? Param1 { get; set; }

    public string? Param2 { get; set; }

    public string? Param3 { get; set; }

    public string? Param4 { get; set; }

    public string? Param5 { get; set; }

    public virtual TagCategory Category { get; set; } = null!;
}
