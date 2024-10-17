
using Microsoft.EntityFrameworkCore;

namespace TMWeb.Services
{
    public class HostedService : IHostedService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public HostedService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var sfService = scope.ServiceProvider.GetRequiredService<TMWebShopfloorService>();
                await sfService.InitAll();
                var scrpService = scope.ServiceProvider.GetRequiredService<ScriptService>();
                await scrpService.InitAllScripts();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
