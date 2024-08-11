namespace TMWeb.EFModels
{
    public partial class Process
    {
        public Process() { }

        public Process(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}
