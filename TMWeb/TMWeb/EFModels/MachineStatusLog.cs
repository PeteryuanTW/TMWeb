namespace TMWeb.EFModels
{
    public partial class MachineStatusLog
    {
        public Guid Id { get; set; }

        public Guid MachineID { get; set; }

        public int Status { get; set; }

        public DateTime LogTime { get; set; }
    }
}
