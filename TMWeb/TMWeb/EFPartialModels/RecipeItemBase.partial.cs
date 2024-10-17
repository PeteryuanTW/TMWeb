namespace TMWeb.EFModels
{
    public abstract partial class RecipeItemBase 
    {
        //public void SetTargetTag(Tag? t)
        //{
        //    TargetTagId = t?.Id;
        //    DataType = t?.DataType;
        //}

        public bool hasTargetTag => TargetTagCatId != null && TargetTagId != null;
        public bool hasDatatype => DataType != null;
        public bool hasTarget => hasTargetTag && hasDatatype;
        public abstract Tuple<bool, Object?> GetValue(Workorder wo);
        public abstract Tuple<bool, string> GetValueString(Workorder wo);
    }
}
