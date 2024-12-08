namespace CommonLibrary.MachinePKG.EFModel
{
    public partial class ErrorCodeMapping
    {
        public ErrorCodeMapping() { }

        public ErrorCodeMapping(Guid? id)
        {
            Id = Guid.NewGuid();
            CategoryId = id;
        }
    }
}
