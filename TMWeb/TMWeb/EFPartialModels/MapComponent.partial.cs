using TMWeb.Data;

namespace TMWeb.EFModels
{
    public enum TargetType
    {
        Station = 0,
        Machine = 1,
    }
    public partial class MapComponent
    {

        public MapComponent() { }

        public MapComponent(Guid mapId)
        {
            Id = Guid.NewGuid();
            Type = 0;
            MapId = mapId;
            PositionX = 0;
            PositionY = 0;
            Width = 10;
            Height = 10;
        }

        
    }
}
