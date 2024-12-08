namespace TMWeb.EFModels
{
    public partial class ErrorCodeMapping
    {
        public Guid? Id { get; set; }

        public Guid? CategoryId { get; set; }

        public string ConditionString { get; set; } = null!;

        public string Description { get; set; } = null!;

        public virtual ErrorCodeCategory Category { get; set; } = null!;

    }
}
