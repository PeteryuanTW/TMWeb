using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class TaskRecordConfig
{
    public Guid Id { get; set; }

    public string? TaskRecordsCategory { get; set; }

    public virtual ICollection<TaskRecordContent> TaskRecordContents { get; set; } = new List<TaskRecordContent>();

    public virtual ICollection<Workerder> Workerders { get; set; } = new List<Workerder>();
}
