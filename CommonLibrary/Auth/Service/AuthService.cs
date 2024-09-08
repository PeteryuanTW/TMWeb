using CommonLibrary.Auth.EFModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BitzArt.Blazor.Cookies;
namespace TMWeb.Services
{
    public class AuthService
    {
        private string cookieName = "TMMAuthCookie";

        private readonly IServiceScopeFactory scopeFactory;
        private readonly ICookieService cookieService;

        private string userName = string.Empty;
        public string UserName => userName;

        public string role = string.Empty;

        public ICollection<string> actions = new List<string>();

        public bool isAuth => !string.IsNullOrEmpty(userName);

        private bool retivedUserInfo = false;

        public System.Action? AuthStatusChangedAct;
        private void AuthStatusChanged() => AuthStatusChangedAct?.Invoke();

        public bool RetivedUserInfo => retivedUserInfo;
        public AuthService(string cookieName, IServiceScopeFactory scopeFactory, ICookieService cookieService)
        {
            this.cookieName = cookieName;
            this.scopeFactory = scopeFactory;
            this.cookieService = cookieService;
        }

        public async Task RetriveUserInfo()
        {
            if (!retivedUserInfo)
            {
                var cookie = await cookieService.GetAsync(cookieName);
                if (cookie != null)
                {
                    var token = cookie.Value.ToString();
                    Login(token);
                }
                else
                {
                    await Logout();
                }
                retivedUserInfo = true;
                AuthStatusChanged();
            }
            else
            {

            }

        }

        public void Login(string token)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var user = dbContext.UserInfos.Include(x => x.Role).ThenInclude(x => x.ActionCodes).FirstOrDefault(x => x.Token == new Guid(token));
                if (user != null)
                {
                    SetUserInfo(user);
                    AuthStatusChanged();
                }
            }
        }

        public async Task Login(string userName, string password)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var user = dbContext.UserInfos.Include(x => x.Role).ThenInclude(x => x.ActionCodes).FirstOrDefault(x => x.UserName == userName && x.HashPassword == password);
                if (user != null)
                {
                    SetUserInfo(user);
                    Guid newToken = Guid.NewGuid();
                    user.Token = newToken;
                    await dbContext.SaveChangesAsync();
                    await cookieService.SetAsync(cookieName, newToken.ToString(), DateTime.Now.AddHours(1), CancellationToken.None);
                    AuthStatusChanged();
                }
            }
        }

        private void SetUserInfo(UserInfo userInfo)
        {
            userName = userInfo.UserName;
            role = userInfo.Role.RoleName;
            actions = userInfo.Role.ActionCodes.Select(x => x.Name).ToList();
        }

        public async Task Logout()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var user = dbContext.UserInfos.FirstOrDefault(x => x.UserName == userName);
                if (user != null)
                {
                    user.Token = null;
                    await dbContext.SaveChangesAsync();
                    AuthStatusChanged();
                }
            }
            await cookieService.RemoveAsync(cookieName);
            AuthStatusChanged();
        }
    }
}
