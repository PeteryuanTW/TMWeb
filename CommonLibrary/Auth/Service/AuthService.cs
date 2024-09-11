using CommonLibrary.Auth.EFModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BitzArt.Blazor.Cookies;
using CommonLibrary.API.Message;
using CommonLibrary.Auth.Data;

namespace CommonLibrary.Auth
{
    public class AuthService
    {
        private string cookieName = "TMMAuthCookie";

        private readonly IServiceScopeFactory scopeFactory;
        private readonly ICookieService cookieService;

        private UserInfoDTO userInfoDTO = new();
        public UserInfoDTO UserInfoDTO => userInfoDTO;

        public bool isAuth => !string.IsNullOrEmpty(userInfoDTO.UserName);

        private bool retivedUserInfo = false;

        public bool IsProcessing => isProcessing;
        private bool isProcessing = false;

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
            isProcessing = true;
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
            isProcessing = false;
        }

        public void Login(string token)
        {
            isProcessing = true;
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var user = dbContext.UserInfos.Include(x => x.RoleCodeNavigation).ThenInclude(x => x.ActionCodes).FirstOrDefault(x => x.Token == new Guid(token));
                if (user != null)
                {
                    SetUserInfo(user);
                    AuthStatusChanged();
                }
            }
            isProcessing = false;
        }

        public async Task<RequestResult> Login(LoginDataDTO loginDataDTO)
        {
            isProcessing = true;
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    var user = dbContext.UserInfos.Include(x => x.RoleCodeNavigation).ThenInclude(x => x.ActionCodes).FirstOrDefault(x => x.UserName == loginDataDTO.UserName);
                    if (user != null)
                    {
                        if (user.HashPassword == loginDataDTO.Password)
                        {
                            SetUserInfo(user);
                            Guid newToken = Guid.NewGuid();
                            user.Token = newToken;
                            await dbContext.SaveChangesAsync();
                            await cookieService.SetAsync(cookieName, newToken.ToString(), DateTime.Now.AddHours(1), CancellationToken.None);
                            AuthStatusChanged();
                            return new(2, $"login success");

                        }
                        else
                        {
                            return new(4, $"user {loginDataDTO.UserName} password not match");
                        }

                    }
                    else
                    {
                        return new(4, $"user {loginDataDTO.UserName} is not exist");
                    }
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
            finally
            {
                isProcessing = false;
            }
        }

        private void SetUserInfo(UserInfo userInfo)
        {
            userInfoDTO = new(userInfo);
        }

        public async Task<RequestResult> Logout()
        {
            isProcessing = true;
            if (isAuth)
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    var user = dbContext.UserInfos.FirstOrDefault(x => x.UserName == userInfoDTO.UserName);
                    if (user != null)
                    {
                        user.Token = null;
                        await dbContext.SaveChangesAsync();
                    }
                }
                await cookieService.RemoveAsync(cookieName);
                userInfoDTO = new();
                isProcessing = false;
                AuthStatusChanged();
                return new(2, $"logout success");
            }
            else
            {
                isProcessing = false;
                return new(3, $"no auth yet");
            }
            
        }
    }
}
