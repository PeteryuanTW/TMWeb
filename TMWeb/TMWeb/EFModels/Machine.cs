using CommonLibrary.Machine.EFModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class Machine
{
    public Guid Id { get; set; }

    public Guid? ProcessId { get; set; }
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [RegularExpression(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)")]
    public string Ip { get; set; } = null!;

    [Required]
    [Range(0, 65535)]
    public int Port { get; set; }

    [Required]
    public int ConnectionType { get; set; }

    [Required]
    [Range(-1, 10)]
    public int MaxRetryCount { get; set; }

    public Guid? TagCategoryId { get; set; }

    public Guid? LogicStatusCategoryId { get; set; }

    public Guid? ErrorCodeCategoryId { get; set; }

    public bool Enabled { get; set; }

    [Required]
    [Range(100, 65535)]
    public int UpdateDelay { get; set; }

    public virtual Process? Process { get; set; }

    public virtual TagCategory? TagCategory { get; set; }

    public virtual LogicStatusCategory? LogicStatusCategory { get; set; }

    public virtual ErrorCodeCategory? ErrorCodeCategory { get; set; }
}
