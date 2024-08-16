namespace TMWeb.EFModels
{
    public partial class Workorder
    {
        public bool HasItemRecord => ItemRecordsCategoryId != null;
    }
}
