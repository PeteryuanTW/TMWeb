using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels
{
    public partial class StaticRecipe : RecipeItemBase
    {
        
        [Required]
        public string ValueString { get; set; }
    }
}
