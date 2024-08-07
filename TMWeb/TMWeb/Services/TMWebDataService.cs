using Microsoft.EntityFrameworkCore;
using TMWeb.EFModels;

namespace TMWeb.Services
{
    public class TMWebDataService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public TMWebDataService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public Task<List<WorkorderRecipeConfig>> GetWorkorderRecipeConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.ToList());
            }
        }
    }

    
}
