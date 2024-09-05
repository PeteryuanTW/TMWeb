using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class TaskDetail
{
    public Guid Id { get; set; }

    public Guid? ItemId { get; set; }

    public Guid? StationId { get; set; }

    public string? SerialNo { get; set; }

    public int TargetAmount { get; set; }

    public int Okamount { get; set; }

    public int Ngamount { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public virtual ItemDetail? Item { get; set; }

    public virtual Station? Station { get; set; }

    public virtual ICollection<TaskRecordDetail> TaskRecordDetails { get; set; } = new List<TaskRecordDetail>();
}
