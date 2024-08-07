using Microsoft.AspNetCore.Mvc;
using TMWeb.Components.Pages.Setting;
using TMWeb.EFModels;
using TMWeb.Services;

namespace TMWeb.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HomeController : Controller
	{
		private readonly IServiceScopeFactory scopeFactory;
		public HomeController(IServiceScopeFactory scopeFactory)
		{
			this.scopeFactory = scopeFactory;
		}
		[HttpGet]
		public IActionResult GetAll()
		{
			using (var scope = scopeFactory.CreateScope())
			{
				var shopfloorService = scope.ServiceProvider.GetRequiredService<TMWebShopfloorService>();
				return Ok(shopfloorService.GetWorkorderRecipeConfigs());
			}
			
		}
	}
}
