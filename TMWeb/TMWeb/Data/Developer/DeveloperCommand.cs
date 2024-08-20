using System.Runtime.CompilerServices;

namespace TMWeb.Data.Developer
{
    public class DeveloperCommand
    {
        public int CommandCode { get; set; }
        public string CommandName { get; set; }
        public int ParameterAmount { get; set; }
        public string Hints { get; set; }
        public string ParameterType {  get; set; }

        public DeveloperCommand(int commandCode, String commandName, int parameterAmount, string hints, string parameterType)
        {
            CommandCode = commandCode;
            CommandName = commandName;
            ParameterAmount = parameterAmount;
            Hints = hints;
            ParameterType = parameterType;
        }
    }
}
