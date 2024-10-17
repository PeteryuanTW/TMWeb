using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels;

public partial class ItemRecordConfig
{
    public Guid Id { get; set; }
    [Required]
    public string ItemRecordCategory { get; set; } = null!;

    public virtual ICollection<ItemRecordContent> ItemRecordContents { get; set; } = new List<ItemRecordContent>();

    public virtual ICollection<Workorder> Workorders { get; set; } = new List<Workorder>();
}
