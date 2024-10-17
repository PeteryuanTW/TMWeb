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
        
        public Workorder(Guid id)
        {
            this.Id = id;
            Status = (int)WorkorderStatus.Init;
            CreateTime = DateTime.Now;
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
