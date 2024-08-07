using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class WorkorderRecordContent
{
    public Guid Id { get; set; }

    public Guid? ConfigId { get; set; }

    public string? RecordName { get; set; }

    public int? DataType { get; set; }

    public virtual WorkorderRecordConfig? Config { get; set; }

    public virtual ICollection<WorkorderRecordDetail> WorkorderRecordDetails { get; set; } = new List<WorkorderRecordDetail>();
}
