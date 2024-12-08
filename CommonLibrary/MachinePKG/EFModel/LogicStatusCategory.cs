using System.ComponentModel.DataAnnotations;

namespace CommonLibrary.MachinePKG.EFModel
{
    public partial class LogicStatusCategory
    {
        public Guid? Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        [Range(1, 4)]
        public int DataType { get; set; }

        public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();

        public virtual ICollection<LogicStatusCondition> LogicStatusConditions { get; set; } = new List<LogicStatusCondition>();
    }
}
