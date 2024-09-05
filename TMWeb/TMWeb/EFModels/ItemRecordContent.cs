using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class ItemRecordContent
{
    public Guid Id { get; set; }

    public Guid? ConfigId { get; set; }

    public string? RecordName { get; set; }

    public int? DataType { get; set; }

    public virtual ItemRecordConfig? Config { get; set; }

    public virtual ICollection<ItemRecordDetail> ItemRecordDetails { get; set; } = new List<ItemRecordDetail>();

    public virtual ICollection<StationUirecord> StationUirecords { get; set; } = new List<StationUirecord>();
}
