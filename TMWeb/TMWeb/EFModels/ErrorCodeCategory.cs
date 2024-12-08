namespace TMWeb.EFModels
{
    public partial class ErrorCodeCategory
    {
        public Guid? Id { get; set; }

        public string Name { get; set; } = null!;

        public int DataType { get; set; }

        public virtual ICollection<Machine> Machines { get; set; } = new List<Machine>();

        public virtual ICollection<ErrorCodeMapping> ErrorCodeMappings { get; set; } = new List<ErrorCodeMapping>();
    }
}
