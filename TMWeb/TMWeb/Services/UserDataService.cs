using CommonLibrary.Auth.EFModels;
using Microsoft.EntityFrameworkCore;
using TMWeb.EFModels;

namespace TMWeb.Services
{
    public class UserDataService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public UserDataService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public Task<List<UserInfo>> GetAllUsers()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                return Task.FromResult(dbContext.UserInfos.Include(x=>x.Role).ThenInclude(x=>x.ActionCodes).ToList());
            }
        }
    }
}
