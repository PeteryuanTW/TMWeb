using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class Station
{
    public Guid Id { get; set; }

    public Guid? ProcessId { get; set; }

    public string? Name { get; set; }

    public int? ProcessIndex { get; set; }

    public int? StationType { get; set; }

    public virtual Process? Process { get; set; }

    public virtual ICollection<TaskDetail> TaskDetails { get; set; } = new List<TaskDetail>();
}
