using CommonLibrary.API.Message;
using CommonLibrary.MachinePKG;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using TMWeb.Data;
using TMWeb.EFModels;
using TMWeb.Scripts.Template;

namespace TMWeb.Services
{
    public class ScriptService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private List<ScriptConfig> scriptConfigs;

        public ScriptService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            scriptConfigs = new();
        }

        public Task<List<ScriptConfig>> GetAllScripts()
        {
            return Task.FromResult(scriptConfigs);
        }

        public Task<List<ScriptConfig>> GetAllScriptsConfig()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.ScriptConfigs.AsNoTracking().ToList());
            }
        }

        public ScriptConfig? GetScriptByID(Guid id)
        {
            return scriptConfigs.FirstOrDefault(x => x.Id == id);
        }
        public ScriptConfig? GetScriptByIDForExport(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.ScriptConfigs.AsNoTracking().FirstOrDefault(x => x.Id == id);
                return target;

            }
        }
        public async Task<RequestResult> UpsertScript(ScriptConfig scriptConfig)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ScriptConfigs.FirstOrDefault(x => x.Id == scriptConfig.Id);
                    bool exist = target is not null;
                    if (exist)
                    {
                        target.ScriptName = scriptConfig.ScriptName;
                        target.DelayMilliseconds = scriptConfig.DelayMilliseconds;
                        target.MaxLogCount = scriptConfig.MaxLogCount;
                        target.AutoCompile = scriptConfig.AutoCompile;
                        target.AutoRun = scriptConfig.AutoRun;
                        target.SuorceCode = scriptConfig.SuorceCode;
                    }
                    else
                    {
                        scriptConfig.SuorceCode = ScriptTemplate.GetCSharpTemplate(scriptConfig.ScriptName);
                        await dbContext.ScriptConfigs.AddAsync(scriptConfig);
                    }
                    await dbContext.SaveChangesAsync();
                    DataEditMode dataEditMode = exist ? DataEditMode.Update : DataEditMode.Insert;
                    await RefreshScript(scriptConfig, dataEditMode);
                    return new(2, $"Upsert script {scriptConfig.ScriptName} success");
                }
            }
            catch (Exception e)
            {
                return new(4, e.Message);
            }
        }
        public async Task<RequestResult> DeleteScript(ScriptConfig script)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ScriptConfigs.FirstOrDefault(x => x.Id == script.Id);
                    if (target != null)
                    {
                        dbContext.Remove(target);
                        await dbContext.SaveChangesAsync();
                        await RefreshScript(target, DataEditMode.Delete);
                        return new(2, $"Delete machine {target.ScriptName} success");
                    }
                    else
                    {
                        return new(4, $"Machine {script.ScriptName} not found");
                    }

                }
                catch (Exception e)
                {
                    return new(4, $"Delete machine {script.ScriptName} fail({e.Message})");
                }

            }
        }
        private async Task RefreshScript(ScriptConfig script, DataEditMode dataEditMode)
        {
            var target = GetScriptByID(script.Id);
            if (target != null)
            {
                scriptConfigs.Remove(target);
                target.Dispose();
                if (dataEditMode != DataEditMode.Delete)
                {
                    await InitScript(script);
                    scriptConfigs.Add(script);
                }
                else
                {
                }
            }
            else
            {
                await InitScript(script);
                scriptConfigs.Add(script);
            }
            ScriptConfigChanged(script.Id, dataEditMode);
        }

        public Action<Guid, DataEditMode>? ScriptConfigChangedAct;
        private void ScriptConfigChanged(Guid id, DataEditMode mode)
        {
            ScriptConfigChangedAct?.Invoke(id, mode);
        }






        public async Task<RequestResult> UpdateScriptCode(ScriptConfig scriptConfig)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ScriptConfigs.FirstOrDefault(x => x.Id == scriptConfig.Id);
                    if (target is not null)
                    {
                        target.SuorceCode = scriptConfig.SuorceCode;
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Update script code {scriptConfig.ScriptName} success");
                    }
                    else
                    {
                        return new(2, $"Script code {scriptConfig.ScriptName} not found");
                    }
                    
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
                    await InitScript(scriptConfig);
                }
            }
        }

        public async Task InitScript(ScriptConfig scriptConfig)
        {
            if (scriptConfig.AutoCompile)
            {
                await ReadScriptAndCompile(scriptConfig);
            }
        }

        private async Task ReadScriptAndCompile(ScriptConfig scriptConfig)
        {
            try
            {
                var code = scriptConfig.SuorceCode;//await File.ReadAllTextAsync($"{Root}\\{scriptConfig.ScriptName}.script");
                using (var scope = scopeFactory.CreateScope())
                {
                    var loaderService = scope.ServiceProvider.GetRequiredService<ScriptLoaderService>();
                    var assembly = await loaderService.CompileToDLLAssembly(code, release: true);
                    var mytype = assembly.GetType($"TMWeb.Scripts.Source.{scriptConfig.ScriptName}");
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
