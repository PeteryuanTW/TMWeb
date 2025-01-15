using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMWeb.EFModels;

public partial class ScriptConfig
{
    public Guid Id { get; set; }
    [Required]
    public string ScriptName { get; set; } = null!;
    [Range(100, 5000)]
    public int DelayMilliseconds { get; set; }
    [Range(10, 100)]
    public int MaxLogCount { get; set; }
    public bool AutoCompile { get; set; }
    public bool AutoRun { get; set; }
    public string SuorceCode { get; set; } = null!;

}
