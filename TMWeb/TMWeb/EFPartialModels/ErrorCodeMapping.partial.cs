namespace TMWeb.EFModels
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
