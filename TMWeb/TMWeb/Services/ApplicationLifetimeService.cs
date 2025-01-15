using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using TMWeb.Extension;

namespace TMWeb.Services
{
    public class ApplicationLifetimeService
    {


        private readonly IHostApplicationLifetime _applicationLifetime;
        public ApplicationLifetimeService(IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
        }

        private void OnStarted()
        {
            //SerilogExtension.LogWithSeverity(LogEventLevel.Information, "app start");
        }

        private void OnStopping()
        {
            //SerilogExtension.LogWithSeverity(LogEventLevel.Information, "app stop");
        }
    }

}

