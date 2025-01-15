using CommonLibrary.Auth.EFModels;
using CommonLibrary.Auth;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CommonLibrary.MachinePKG.Service;
using CommonLibrary.MachinePKG.EFModel;
using Microsoft.AspNetCore.Builder;

namespace CommonLibrary.MachinePKG
{
    public static class MachineExtention
    {
        public static IHostApplicationBuilder AddMachineService(this IHostApplicationBuilder builder, string dbConnectionStringName = "DefaultConnection")
        {
            builder.Services.AddDbContextFactory<MachineDBContext>(options =>
            {
                var a = builder.Configuration.GetConnectionString(dbConnectionStringName);
                options.UseSqlServer(builder.Configuration.GetConnectionString(dbConnectionStringName));
            });

            builder.Services.AddSingleton<IMachineService, MachineService>();
            builder.Services.AddLocalization();


            return builder;
        }

        public static WebApplication RunMachineService(this WebApplication webApplication)
        {
            var machineServiceInstance = webApplication.Services.GetRequiredService<IMachineService>();
            machineServiceInstance.InitAllMachinesFromDB();
            return webApplication;

        }
    }
}
