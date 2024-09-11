using System;
using System.Collections.Generic;

namespace CommonLibrary.Auth.EFModels;

public partial class RoleAction
{
    public int RoleCode { get; set; }

    public int ActionCode { get; set; }

    public virtual Role RoleCodeNavigation { get; set; } = null!;
}
