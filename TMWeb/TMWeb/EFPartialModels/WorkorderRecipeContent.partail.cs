namespace TMWeb.EFModels
{
    public partial class WorkorderRecipeContent
    {
        public Object? RetriveValue()
        {
            switch (DataType)
            {
                case 1:
                    return Convert.ToBoolean(Value);
                case 2:
                    return Convert.ToUInt16(Value);
                default:
                    return null;
            }
        }
    }
}
