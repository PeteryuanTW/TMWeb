namespace TMWeb.Scripts.Template
{
    public static class ScriptTemplate
    {
        public static string GetCSharpTemplate(string scriptName)
        {
            return $"using System;\r\nusing System.Threading.Tasks;\r\nusing TMWeb.Services;\r\nusing TMWeb.Scripts.Template;\r\n\r\nnamespace TMWeb.Scripts.Source\r\n{{\r\n    public class {scriptName} : ScriptBaseClass\r\n    {{\r\n        public {scriptName}(TMWebShopfloorService service) : base(service)\r\n        {{\r\n\r\n        }}\r\n\r\n        public override void OnStart()\r\n        {{\r\n            \r\n        }}\r\n        \r\n        public override async Task RunAction()\r\n        {{\r\n            \r\n        }}\r\n        \r\n        public override void OnStop()\r\n        {{\r\n            \r\n        }}\r\n    }}\r\n}};";
        }
    }
}
