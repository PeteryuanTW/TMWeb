namespace TMWeb.EFModels
{
    public partial class WorkorderRecipeConfig
    {
        public WorkorderRecipeConfig() { }

        public WorkorderRecipeConfig(Guid id)
        {
            this.Id = id;
        }

        public bool HasRecipes => Recipes.Count > 0;
    }
}
