using TMWeb.Data;

namespace TMWeb.EFModels
{
    
    public partial class MapComponent
    {

        private double mouseOffsetX;
        private double mouseOffsetY;
        public double MouseOffsetX => mouseOffsetX;
        public double MouseOffsetY => mouseOffsetY;

        public bool IsRunningCommand => isRunningCommand;
        private bool isRunningCommand = false;

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

        public void SetType(int i)
        {
            if (Type != i)
            {
                TargetId = null;
            }
            Type = i;
        }

        public void SetClickOffset(double x, double y)
        {
            mouseOffsetX = x;
            mouseOffsetY = y;
        }

        public void StartCommand()
        {
            isRunningCommand = true;
        }

        public void FinishedCommand()
        {
            isRunningCommand = false;
        }
    }
}
