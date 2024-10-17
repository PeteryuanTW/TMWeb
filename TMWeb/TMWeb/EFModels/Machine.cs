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

    public Guid? TagCategoryId { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<MapComponent> MapComponents { get; set; } = new List<MapComponent>();

    public virtual Process? Process { get; set; }

    public virtual TagCategory? TagCategory { get; set; }
}
