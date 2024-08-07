using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class TaskRecordContent
{
    public Guid Id { get; set; }

    public Guid? ConfigId { get; set; }

    public string? RecordName { get; set; }

    public int? DataType { get; set; }

    public virtual TaskRecordConfig? Config { get; set; }

    public virtual ICollection<TaskRecordDetail> TaskRecordDetails { get; set; } = new List<TaskRecordDetail>();
}
