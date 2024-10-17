using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels
{
    public partial class CustomRecipe : RecipeItemBase
    {
        [Required]
        public Guid? TargetRecordCatID {  get; set; }
        [Required]
        public Guid? TargetRecordID { get; set; }
    }
}
