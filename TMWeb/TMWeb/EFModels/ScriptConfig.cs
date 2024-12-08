using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMWeb.EFModels;

public partial class ScriptConfig
{
    public Guid Id { get; set; }
    [Required]
    public string ScriptName { get; set; } = null!;
    public bool AutoCompile { get; set; }
    public bool AutoRun { get; set; }
    public string SuorceCode { get; set; } = null!;
}
