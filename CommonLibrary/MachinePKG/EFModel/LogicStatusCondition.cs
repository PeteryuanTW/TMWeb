using System.ComponentModel.DataAnnotations;

namespace CommonLibrary.MachinePKG.EFModel
{
    public partial class LogicStatusCondition
    {
        public Guid Id { get; set; }
        public Guid? CategoryId { get; set; }
        [Required]
        public string ConditionString { get; set; } = null!;
        public int Status { get; set; }

        public virtual LogicStatusCategory Category { get; set; } = null!;
    }
}
