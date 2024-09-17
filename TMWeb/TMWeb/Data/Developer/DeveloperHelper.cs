namespace TMWeb.Data.Developer
{
    public static class DeveloperHelper
    {
        private static List<DeveloperCommand> developerCommands = new List<DeveloperCommand>
        {
            new(1, "Reset Workorder",1 ,"Workorder guid string","3" ),
            new(2, "Get Hashed Password",1 ,"password","3" ),
        };
        public static List<DeveloperCommand> DeveloperCommands => developerCommands;
}
}
