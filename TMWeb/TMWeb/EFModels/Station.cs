using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class Station
{
    public Guid Id { get; set; }

    public Guid ProcessId { get; set; }

    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public int ProcessIndex { get; set; }
    [Required]
    public int StationType { get; set; }
    [Required]
    public bool Enable { get; set; }

    public virtual ICollection<MapComponent> MapComponents { get; set; } = new List<MapComponent>();

    public virtual Process Process { get; set; } = null!;

    public virtual ICollection<StationUirecord> StationUirecords { get; set; } = new List<StationUirecord>();

    public virtual ICollection<TaskDetail> TaskDetails { get; set; } = new List<TaskDetail>();
}
