using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class ItemRecordConfig
{
    public Guid Id { get; set; }

    public string ItemRecordCategory { get; set; } = null!;

    public virtual ICollection<ItemRecordContent> ItemRecordContents { get; set; } = new List<ItemRecordContent>();

    public virtual ICollection<Workorder> Workorders { get; set; } = new List<Workorder>();
}
