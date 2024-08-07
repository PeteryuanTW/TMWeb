using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using TMWeb.Components.Pages.Setting;
using TMWeb.EFModels;

namespace TMWeb.Services
{
	public class TMWebShopfloorService
	{
		private readonly IServiceScopeFactory scopeFactory;

		public TMWebShopfloorService(IServiceScopeFactory scopeFactory)
		{
			this.scopeFactory = scopeFactory;
		}
		//process
		public Task<List<Process>> GetAllProcess()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				return Task.FromResult(dbContext.Processes.Include(x => x.Stations.OrderBy(x=>x.ProcessIndex)).ToList());
			}
		}
		//workorder
		public Task<List<Workerder>> GetAllWorkorderAndRecipe()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				return Task.FromResult(dbContext.Workerders.Include(x => x.WorkorderRecordCategoryId).ToList());
			}
		}
		public Task<List<Workerder>> GetAllWorkorders()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				return Task.FromResult(dbContext.Workerders.ToList());
			}
		}
		public Task<Workerder?> GetWorkordersDetails(Guid id)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				return Task.FromResult(dbContext.Workerders.Include(x=>x.Process)
					.Include(x=>x.RecipeCategory).ThenInclude(x=>x.WorkorderRecipeContents)
					.FirstOrDefault(x=>x.Id == id));
			}
		}

		public Task<Workerder?> GetWorkorderAndRecipeByNoAndLot(string wo, string lot)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				return Task.FromResult(dbContext.Workerders.Include(x => x.WorkorderRecordCategoryId).FirstOrDefault(x => x.WorkerderNo == wo && x.Lot == lot));
			}
		}
		//recipe
		public Task<WorkorderRecipeConfig?> GetRecipeConfigsByName(string RecipeName)
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x => x.WorkorderRecipeContents).FirstOrDefault(x => x.RecipeCategory == RecipeName));
			}
		}

		public Task<List<WorkorderRecipeConfig>> GetWorkorderRecipeConfigs()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
				var a = dbContext.WorkorderRecipeConfigs.Include(x => x.WorkorderRecipeContents).ToList();
				return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x=>x.Workerders).ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
			}
		}
	}
}
