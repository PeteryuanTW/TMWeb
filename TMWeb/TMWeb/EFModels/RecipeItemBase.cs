using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels
{
    public abstract partial class RecipeItemBase
    {
        public Guid Id { get; set; }

        public Guid ConfigId { get; set; }
        [Required(ErrorMessage ="Null is invalid")]
        public string RecipeItemName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Null is invalid")]
        public int TriggerTiming { get; set; }
        [Required(ErrorMessage = "Null is invalid")]
        public Guid? TargetTagCatId { get; set; }
        [Required(ErrorMessage = "Null is invalid")]
        public Guid? TargetTagId { get; set; }
        [Required(ErrorMessage = "Null is invalid")]
        [Range(1, 44)]
        public int DataType { get; set; }
        public virtual WorkorderRecipeConfig? Config { get; set; }
    }
}
