using BitzArt.Blazor.Cookies;
using CommonLibrary.Auth.EFModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Auth
{
    public static class UserExtention
    {
        public static IHostApplicationBuilder AddUserCookieService(this IHostApplicationBuilder builder, string cookieName = "UserToken", string dbConnectionStringName = "DefaultConnection")
        {
            builder.AddBlazorCookies();
            builder.Services.AddDbContextFactory<UserDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString(dbConnectionStringName));
            });
            builder.Services.AddScoped<AuthService>(p =>
            {
                return new AuthService(cookieName, p.GetRequiredService<IServiceScopeFactory>(), p.GetRequiredService<ICookieService>());
            });
            return builder;
        }
    }
}
