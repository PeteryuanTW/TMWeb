using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class ItemRecordContent
{
    public Guid Id { get; set; }

    public Guid? ConfigId { get; set; }
    [Required]
    public string? RecordName { get; set; }
    [Required]
    public int? DataType { get; set; }

    public virtual ItemRecordConfig? Config { get; set; }

    public virtual ICollection<ItemRecordDetail> ItemRecordDetails { get; set; } = new List<ItemRecordDetail>();

    public virtual ICollection<StationUirecord> StationUirecords { get; set; } = new List<StationUirecord>();
}
