using Microsoft.EntityFrameworkCore;
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
            InitAllScripts();
        }

        public List<ScriptConfig> GetAllScripts()
        {
            return scriptConfigs;
        }

        public ScriptConfig? GetScriptByID(Guid id)
        {
            return scriptConfigs.FirstOrDefault(x => x.Id == id);
        }

        public Task InitAllScripts()
        {
            scriptConfigs = new();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                scriptConfigs = dbContext.ScriptConfigs.AsNoTracking().ToList();
            }
            return Task.CompletedTask;
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
