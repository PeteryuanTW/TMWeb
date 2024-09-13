using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class ScriptConfig
{
    public Guid Id { get; set; }

    public string ScriptName { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public bool Enable { get; set; }
}
