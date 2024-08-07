using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class WorkorderRecordConfig
{
    public Guid Id { get; set; }

    public string? WorkorderRecordCategory { get; set; }

    public virtual ICollection<Workerder> Workerders { get; set; } = new List<Workerder>();

    public virtual ICollection<WorkorderRecordContent> WorkorderRecordContents { get; set; } = new List<WorkorderRecordContent>();
}
