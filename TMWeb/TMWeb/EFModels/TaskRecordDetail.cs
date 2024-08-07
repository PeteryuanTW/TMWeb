using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class TaskRecordDetail
{
    public Guid TaskId { get; set; }

    public Guid RecordContentId { get; set; }

    public string? Value { get; set; }

    public virtual TaskRecordContent RecordContent { get; set; } = null!;

    public virtual TaskDetail Task { get; set; } = null!;
}
