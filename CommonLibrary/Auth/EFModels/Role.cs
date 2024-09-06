using System;
using System.Collections.Generic;

namespace CommonLibrary.Auth.EFModels;

public partial class Role
{
    public int Code { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<UserInfo> UserInfos { get; set; } = new List<UserInfo>();

    public virtual ICollection<Action> ActionCodes { get; set; } = new List<Action>();
}
