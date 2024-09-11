using System;
using System.Collections.Generic;

namespace CommonLibrary.Auth.EFModels;

public partial class ActionDetail
{
    public int Code { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Role> RoleCodes { get; set; } = new List<Role>();
}
