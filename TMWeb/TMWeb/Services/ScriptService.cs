using CommonLibrary.API.Message;
using Microsoft.EntityFrameworkCore;
using TMWeb.Data;
using TMWeb.EFModels;
using TMWeb.Scripts.Template;

namespace TMWeb.Services
{
    public class ScriptService
    {
        public string Root => ".\\Scripts\\Source";

        private readonly IServiceScopeFactory scopeFactory;
        private List<ScriptConfig> scriptConfigs;

        public ScriptService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            scriptConfigs = new();
            //InitAllScripts();
        }

        public List<ScriptConfig> GetAllScripts()
        {
            return scriptConfigs;
        }

        public ScriptConfig? GetScriptByID(Guid id)
        {
            return scriptConfigs.FirstOrDefault(x => x.Id == id);
        }

        public async Task<RequestResult> UpsertScript(ScriptConfig scriptConfig)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ScriptConfigs.FirstOrDefault(x => x.Id == scriptConfig.Id);
                    if (target is not null)
                    {
                        target.ScriptName = scriptConfig.ScriptName;
                        target.ClassName = scriptConfig.ClassName;
                        target.AutoCompile = scriptConfig.AutoCompile;
                        target.AutoRun = scriptConfig.AutoRun;
                        target.SuorceCode = scriptConfig.SuorceCode;
                    }
                    else
                    {
                        await dbContext.ScriptConfigs.AddAsync(scriptConfig);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert script {scriptConfig.ScriptName} success");
                }
            }
            catch (Exception e)
            {
                return new(4, e.Message);
            }
        }


        public async Task InitAllScripts()
        {
            scriptConfigs = new();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                scriptConfigs = dbContext.ScriptConfigs.AsNoTracking().ToList();
                foreach (var scriptConfig in scriptConfigs)
                {
                    if (scriptConfig.AutoCompile)
                    {
                        await ReadScriptAndCompile(scriptConfig);
                    }
                }
            }
        }

        private async Task ReadScriptAndCompile(ScriptConfig scriptConfig)
        {
            try
            {
                var code = await File.ReadAllTextAsync($"{Root}\\{scriptConfig.ScriptName}.script");
                using (var scope = scopeFactory.CreateScope())
                {
                    var loaderService = scope.ServiceProvider.GetRequiredService<ScriptLoaderService>();
                    var assembly = await loaderService.CompileToDLLAssembly(code, release: true);
                    var mytype = assembly.GetType($"TMWeb.Scripts.Source.{scriptConfig.ClassName}");
                    var sfcService = scope.ServiceProvider.GetRequiredService<TMWebShopfloorService>();
                    var instance = (ScriptBaseClass)Activator.CreateInstance(mytype, sfcService);
                    DeployScriptByID(scriptConfig.Id, instance);
                }
            }
            catch (Exception e)
            {

            }

        }

        public void DeployScriptByID(Guid id, ScriptBaseClass targetScript)
        {
            var target = GetScriptByID(id);
            if (target != null)
            {
                target.SetScript(targetScript);
            }
        }
    }
}
