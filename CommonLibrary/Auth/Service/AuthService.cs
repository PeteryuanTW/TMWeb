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
        private readonly int delay = 300;


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
        public void StartProcessing()
        {
            isProcessing = true;
            AuthStatusChanged();
        }

        public void StopProcessing()
        {
            isProcessing = false;
            AuthStatusChanged();
        }
        public bool IsRole(string role)
        {
            if (RetivedUserInfo)
            {
                if (userInfoDTO.Role != null)
                {
                    return userInfoDTO.Role.RoleName == role;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task RetriveUserInfo()
        {
            StartProcessing();
            if (!retivedUserInfo)
            {
                var cookie = await cookieService.GetAsync(cookieName);
                if (cookie != null)
                {
                    var token = cookie.Value.ToString();
                    await Login(token);
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
            StopProcessing();
        }

        public Task Login(string token)
        {
            StartProcessing();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                var user = dbContext.UserInfos.Include(x => x.RoleCodeNavigation).ThenInclude(x => x.ActionCodes)
                    .AsSplitQuery().AsNoTracking()
                    .FirstOrDefault(x => x.Token == new Guid(token));
                if (user != null)
                {
                    SetUserInfo(user);
                    AuthStatusChanged();
                }
            }
            StopProcessing();
            return Task.CompletedTask;
        }
        public async Task<RequestResult> Login(LoginDataDTO loginDataDTO)
        {
            StartProcessing();
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    var user = dbContext.UserInfos.Include(x => x.RoleCodeNavigation).ThenInclude(x => x.ActionCodes)
                        .AsSplitQuery().AsNoTracking()
                        .FirstOrDefault(x => x.UserName == loginDataDTO.UserName);
                    if (user != null)
                    {
                        if (BCryptHelper.CheckPassword(loginDataDTO.Password, user.HashPassword))
                        {
                            SetUserInfo(user);
                            var userData = dbContext.UserInfos.FirstOrDefault(x => x.UserName == loginDataDTO.UserName);
                            Guid newToken = Guid.NewGuid();
                            userData.Token = newToken;
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
                StopProcessing();
            }
        }

        private void SetUserInfo(UserInfo userInfo)
        {
            userInfoDTO = new(userInfo);
        }
        public async Task<RequestResult> Logout()
        {
            StartProcessing();
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
                StopProcessing();
                AuthStatusChanged();
                return new(2, $"logout success");
            }
            else
            {
                StopProcessing();
                return new(3, $"no auth yet");
            }
            
        }
    }
}
