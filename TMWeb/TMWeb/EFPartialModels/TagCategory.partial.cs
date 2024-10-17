namespace TMWeb.EFModels
{
    public partial class TagCategory
    {
        public TagCategory()
        {

        }

        public TagCategory(Guid CategoryID)
        {
            Id = CategoryID;
        }

    public int TagCount => Tags.Count;
    }
}
