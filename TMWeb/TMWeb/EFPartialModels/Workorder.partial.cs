namespace TMWeb.EFModels
{
    public enum WorkorderStatus
    {
        Init = 0,
        Running = 1,
        Stop = 2,
    }
    public partial class Workorder
    {
        public Workorder() { }
        public Workorder(Guid processId)
        {
            Id = new Guid();
            ProcessId = processId;
            Status = 0;
            TargetAmount = 0;
            Okamount = 0;
            Ngamount = 0;
        }


        public bool HasRecipe => RecipeCategoryId != null;
        public bool RecipeIncluded => RecipeCategory != null;
        public bool HasWorkorderRecord => WorkorderRecordCategoryId != null;
        public bool WorkorderRecordIncluded => WorkorderRecordCategory != null;
        public bool HasItemRecord => ItemRecordsCategoryId != null;
        public bool ItemRecordIncluded => ItemRecordsCategory != null;
        public bool HasTaskRecord => TaskRecordCategoryId != null;
        public bool TaskRecordIncluded => TaskRecordCategory != null;
    }
}
