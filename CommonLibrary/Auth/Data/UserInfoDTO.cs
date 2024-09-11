using CommonLibrary.Auth.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Auth.Data
{
    public class UserInfoDTO
    {
        public UserInfoDTO()
        {
            userName = string.Empty;
            actionDetails = new List<ActionDetail>();
        }
        public UserInfoDTO(UserInfo userInfo)
        {
            userName = userInfo.UserName;
            role = userInfo.RoleCodeNavigation;
            actionDetails = userInfo.RoleCodeNavigation?.ActionCodes.ToList();
        }
        private string userName = string.Empty;
        public string UserName => userName;

        private Role? role;
        public Role? Role => role;

        private IEnumerable<ActionDetail> actionDetails;
        public IEnumerable<ActionDetail> ActionDetails => actionDetails;

    }
}
