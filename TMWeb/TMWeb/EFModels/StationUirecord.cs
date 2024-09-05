using System;
using System.Collections.Generic;

namespace TMWeb.EFModels;

public partial class StationUirecord
{
    public Guid Id { get; set; }

    public Guid? StationId { get; set; }

    public Guid? ItemRecordId { get; set; }

    public virtual ItemRecordContent? ItemRecord { get; set; }

    public virtual Station? Station { get; set; }
}
