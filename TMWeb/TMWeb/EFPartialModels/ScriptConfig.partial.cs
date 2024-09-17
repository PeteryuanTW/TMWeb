using TMWeb.Scripts.Template;

namespace TMWeb.EFModels
{
    public partial class ScriptConfig
    {
        public ScriptBaseClass? script;

        public bool HasScript => script != null;

        public Action? ScriptChangedAct;

        private void ScriptChanged() => ScriptChangedAct?.Invoke();

        public void SetScript(ScriptBaseClass scriptBaseClass)
        {
            script = scriptBaseClass;
            ScriptChanged();
        }

        public void DropScript()
        {
            script = null;
            ScriptChanged();
        }
    }
}
