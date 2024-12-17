namespace TMWeb.EFModels
{
    public partial class RecipeItem
    {
        public RecipeItem(Guid configId)
        {
            Id = Guid.NewGuid();
            ConfigId = configId;
        }

        public bool hasTargetTag => TargetTagCatId != null && TargetTagId != null;
        public bool hasDatatype => DataType != null;
        public bool hasTarget => hasTargetTag && hasDatatype;
        public Tuple<bool, Object?> GetValue(Workorder wo)
        {
            return new(false, new Object());
        }
        public Tuple<bool, string> GetValueString(Workorder wo)
        {
            return new(false,string.Empty);
        }
    }
}
