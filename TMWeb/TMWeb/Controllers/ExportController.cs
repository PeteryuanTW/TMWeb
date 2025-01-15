using CommonLibrary.API.Message;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using TMWeb.EFModels;
using TMWeb.Services;

namespace TMWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportController : ControllerBase
    {
        //private readonly TMWebShopfloorService tmWebShopfloorService;
        private readonly SerilogService serilogService;
        private readonly ScriptService scriptService;
        public ExportController(IServiceScopeFactory scopeFactory)
        {
            serilogService = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<SerilogService>();
            scriptService = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ScriptService>();
        }
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Logs([FromQuery] string start, [FromQuery] string end)
        {
            var logs = await serilogService.GetSerilogDatas(DateTime.Parse(start), DateTime.Parse(end));
            string jsonString = JsonSerializer.Serialize(logs);
            var byteArr = System.Text.Encoding.UTF8.GetBytes(jsonString);
            return File(byteArr, "application/json", $"TMLogs{start}-{end}.json");
        }
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Script([FromQuery] string id)
        {
            var script = scriptService.GetScriptByIDForExport(new Guid(id));
            if (script is null)
            {
                return NotFound();
            }
            string jsonString = JsonSerializer.Serialize(script);
            var byteArr = System.Text.Encoding.UTF8.GetBytes(jsonString);
            return File(byteArr, "application/json", $"{script.ScriptName}.json");
        }

        [HttpPost("[action]")]
        //[Route("[action]")]
        public ActionResult UploadScript(IFormFile file)
        {
            return Ok();
            //try
            //{
            //    if (file == null || file.Length == 0)
            //    {
            //        return new RequestResult(4, "No file uploaded.");
            //    }
            //    using (var stream = new MemoryStream())
            //    {
            //        await file.CopyToAsync(stream);
            //        stream.Position = 0;
            //        var jsonData = await JsonSerializer.DeserializeAsync<ScriptConfig>(stream);
            //        if (jsonData is null)
            //        {
            //            return new RequestResult(4, "Invalid JSON content.");
            //        }
            //        else
            //        {
                        
            //            return await scriptService.UpsertScript(jsonData);
            //        }
            //    }
            //}
            //catch(Exception e)
            //{
            //    return new RequestResult(4, e.Message);
            //}
        }

        [HttpPost("[action]")]
        public ActionResult Upload(IFormFile myFile)
        {
            try
            {
                // Write code that saves the 'myFile' file.
                // Don't rely on or trust the FileName property without validation.
            }
            catch
            {
                return BadRequest();
            }
            return Ok();
        }
    }

}
