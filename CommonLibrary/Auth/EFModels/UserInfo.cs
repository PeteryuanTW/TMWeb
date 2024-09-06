using System;
using System.Collections.Generic;

namespace CommonLibrary.Auth.EFModels;

public partial class UserInfo
{
    public Guid Id { get; set; }

    public string? UserName { get; set; }

    public string? HashPassword { get; set; }

    public int? RoleCode { get; set; }

    public string? Email { get; set; }

    public Guid? Token { get; set; }

    public virtual Role? Role { get; set; }
}
