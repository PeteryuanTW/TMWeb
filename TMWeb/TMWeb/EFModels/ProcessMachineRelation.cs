using System.ComponentModel.DataAnnotations;

namespace TMWeb.EFModels
{
    public partial class ProcessMachineRelation
    {
        public Guid? Id { get; set; }
        //[Required]
        public Guid? ProcessId { get; set; }
        //[Required]
        public Guid? MachineId { get; set; }
    }
}
