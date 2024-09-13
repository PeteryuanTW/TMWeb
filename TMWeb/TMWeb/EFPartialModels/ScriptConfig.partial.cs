using TMWeb.Scripts;

namespace TMWeb.EFModels
{
    public partial class ScriptConfig
    {
        public ScriptBaseClass? script;

        public bool HasScript => script != null;


        public void SetScript(ScriptBaseClass scriptBaseClass)
        {
            script = scriptBaseClass;
        }
    }
}
