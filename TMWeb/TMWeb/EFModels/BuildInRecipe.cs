using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels
{
    public partial class BuildInRecipe: RecipeItemBase
    {
        [Required]
        public string TargetProp { get; set; }

        
    }
}
