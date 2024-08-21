namespace TMWeb.EFModels
{
    public partial class MapImage
    {
        public MapImage() { }

        public MapImage(Guid id)
        {
            Id = id;
            Name = string.Empty;
            DataType = string.Empty;
            DataByte = Array.Empty<byte>();
        }
    }
}
