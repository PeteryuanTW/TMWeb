
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TMWeb.EFModels;

namespace TMWeb.Services
{
    public class SerilogDBCleanupService : IHostedService, IDisposable
    {
        private readonly SerilogCleanupSetting serilogCleanupSetting;
        private Timer? timer;
        private readonly IServiceScopeFactory scopeFactory;

        public SerilogDBCleanupService(IOptions<SerilogCleanupSetting> serilogCleanupSetting, IServiceScopeFactory scopeFactory)
        {
            this.serilogCleanupSetting = serilogCleanupSetting.Value;
            this.scopeFactory = scopeFactory;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(CleanupDataCallback, null, TimeSpan.Zero, TimeSpan.FromDays(serilogCleanupSetting.ExecuteIntervalDays));
            return Task.CompletedTask;
        }
        private void CleanupDataCallback(object state)
        {
            CleanupData().GetAwaiter().GetResult();
        }
        private async Task CleanupData()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var serilogService = scope.ServiceProvider.GetRequiredService<SerilogService>();
                await serilogService.CleanupData();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }


    public class SerilogCleanupSetting
    {
        public int IntervalDays { get; set; }

        public int ExecuteIntervalDays { get; set; }
    }

}
