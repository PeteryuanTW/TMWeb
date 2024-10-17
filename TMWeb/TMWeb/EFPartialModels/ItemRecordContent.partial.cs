namespace TMWeb.EFModels
{
    public partial class ItemRecordContent
    {
        public ItemRecordContent() { }

        public ItemRecordContent(Guid configID)
        {
            Id = Guid.NewGuid();
            ConfigId = configID;
        }
    }
}
