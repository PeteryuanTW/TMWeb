namespace TMWeb.Data.Developer
{
    public static class DeveloperHelper
    {
        private static List<DeveloperCommand> developerCommands = new List<DeveloperCommand>
        {
            new(1, "Reset Workorder",4 ,"Workorder guid string","3" ),
        };
        public static List<DeveloperCommand> DeveloperCommands => developerCommands;
}
}
