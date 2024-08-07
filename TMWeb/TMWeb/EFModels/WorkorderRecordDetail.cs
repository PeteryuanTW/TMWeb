using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class WorkorderRecordDetail
{
    public Guid WorkerderId { get; set; }

    public Guid RecordContentId { get; set; }

    public string? Value { get; set; }

    public virtual WorkorderRecordContent RecordContent { get; set; } = null!;

    public virtual Workerder Workerder { get; set; } = null!;
}
