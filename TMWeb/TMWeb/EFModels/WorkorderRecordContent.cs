using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class WorkorderRecordContent
{
    public Guid Id { get; set; }

    public Guid? ConfigId { get; set; }
    [Required]
    public string? RecordName { get; set; }
    [Required]
    public int? DataType { get; set; }

    public virtual WorkorderRecordConfig? Config { get; set; }

    public virtual ICollection<WorkorderRecordDetail> WorkorderRecordDetails { get; set; } = new List<WorkorderRecordDetail>();
}
