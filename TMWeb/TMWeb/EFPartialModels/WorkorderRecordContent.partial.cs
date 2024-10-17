namespace TMWeb.EFModels
{
    public partial class WorkorderRecordContent
    {
        public WorkorderRecordContent() { }

        public WorkorderRecordContent(Guid confidID)
        {
            Id = Guid.NewGuid();
            ConfigId = confidID;
        }
    }
}
