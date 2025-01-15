using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog.Events;
using TMWeb.EFModels;
using TMWeb.Extension;

namespace TMWeb.Services
{
    public class SerilogService
    {
        private readonly SerilogCleanupSetting serilogCleanupSetting;
        private readonly IServiceScopeFactory scopeFactory;

        public SerilogService(IOptions<SerilogCleanupSetting> serilogCleanupSetting, IServiceScopeFactory scopeFactory)
        {
            this.serilogCleanupSetting = serilogCleanupSetting.Value;
            this.scopeFactory = scopeFactory;
        }


        public async Task CleanupData()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var expiredTime = GetExpiredTime();
                await dbContext.SerilogDatas.Where(x => x.TimeStamp < expiredTime).ExecuteDeleteAsync();
                SerilogExtension.LogWithSeverity(LogEventLevel.Information, $"Cleanup serilog data before {expiredTime.ToString("g")} completed");
            }
        }

        private DateTime GetExpiredTime()
            => DateTime.Now.AddDays(-1 * serilogCleanupSetting.IntervalDays);

        public async Task<IEnumerable<SerilogData>> GetSerilogDatas(DateTime start, DateTime end)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return await dbContext.SerilogDatas.Where(x => x.TimeStamp >= start && x.TimeStamp <= end).ToListAsync();
            }
        }

    }
}
