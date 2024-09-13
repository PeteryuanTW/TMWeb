namespace TMWeb.Scripts.Template
{
    public static class ScriptTemplate
    {
        public static string GetCSharpTemplate(string className)
        {
            return $"using System;\r\nusing System.Threading.Tasks;\r\nusing CommonLibrary.TagPack;\r\nusing CommonLibrary.SignalRPack;\r\nnamespace ServiceManager.Scripts.Script\r\n{{\r\n    public class {className} : ScriptFunctionsClient\r\n    {{               \r\n        public override void OnStart()\r\n        {{\r\n            ConnectToSignalRServer(\"172.25.90.248\", 6060);\r\n        }}\r\n        \r\n        public override async Task RunAction()\r\n        {{\r\n            \r\n        }}\r\n        \r\n        public override void OnStop()\r\n        {{\r\n            \r\n        }}\r\n    }}\r\n}};";
        }
    }
}
