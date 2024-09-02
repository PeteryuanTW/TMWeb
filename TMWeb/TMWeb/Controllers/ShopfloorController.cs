using Microsoft.AspNetCore.Mvc;
using TMWeb.Components.Pages.Setting;
using TMWeb.EFModels;
using TMWeb.Services;
using CommonLibrary.API.Parameter;
using CommonLibrary.API.Message;
using Microsoft.Extensions.Logging;
using Serilog;

namespace TMWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
	public class ShopfloorController : Controller
	{
		private readonly TMWebShopfloorService shopfloorService;
        private readonly ILogger<ShopfloorController> logger;
        public ShopfloorController(ILogger<ShopfloorController> shopfloorControllerlogger, TMWebShopfloorService tmWebShopfloorService)
		{
            logger = shopfloorControllerlogger;
            shopfloorService = tmWebShopfloorService;
		}

        private ActionResult RequestResultToActionResult(RequestResult requestResult)
        {
            switch (requestResult.ReturnCode)
            {
                case 1:
                case 2:
                    return Ok(requestResult.Msg);
                case 3:
                case 4:
                    return BadRequest(requestResult.Msg);
                default:
                    return StatusCode(500, $"not support request result returnCode {requestResult.ReturnCode}");
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> StationInWithSerialNo([FromBody]StationInParam1 stationInParam1)
        {
            var res = await shopfloorService.StationInByNameAndSerialNo(stationInParam1.StationName, stationInParam1.SerialNo);
            return RequestResultToActionResult(res);
        }
        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> WriteItemRecord([FromBody] ItemRecordParam1 itemRecordParam1)
        {
            var res = await shopfloorService.WriteItemRecord(itemRecordParam1.SerialNo, itemRecordParam1.RecordName, itemRecordParam1.RecordValue);
            return RequestResultToActionResult(res);
        }
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult> GetMapConfig()
        {
            
            return Ok(await shopfloorService.GetAllMapConfigs());
        }

    }
}
