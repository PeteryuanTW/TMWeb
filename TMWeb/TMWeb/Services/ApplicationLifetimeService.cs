using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace TMWeb.Services
{
    public class ApplicationLifetimeService
    {


        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<TMWebShopfloorService> logger;
        public ApplicationLifetimeService(IHostApplicationLifetime applicationLifetime, ILogger<TMWebShopfloorService> logger)
        {
            _applicationLifetime = applicationLifetime;
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            this.logger = logger;
        }

        private void OnStarted()
        {
            // Your startup logic here
            logger.LogWarning("app start");
            //Console.WriteLine("Application has started.");
        }

        private void OnStopping()
        {
            logger.LogWarning("app shutdown");

            // Your cleanup logic here
            //Console.WriteLine("Application is shutting down...");
        }
    }

}

