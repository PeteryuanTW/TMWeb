namespace TMWeb.EFModels
{
    public partial class ProcessMachineRelation
    {
        public ProcessMachineRelation() { }

        public ProcessMachineRelation(Guid? id)
        {
            Id = Guid.NewGuid();
            ProcessId = id;
        }
    }
}
