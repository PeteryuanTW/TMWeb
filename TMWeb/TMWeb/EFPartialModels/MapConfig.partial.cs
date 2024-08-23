namespace TMWeb.EFModels
{
    public partial class MapConfig
    {
        public MapConfig() { }
        public MapConfig(Guid id)
        {
            Id = id;
            Name = string.Empty;
            //ImageId = null;
        }
    }
}
