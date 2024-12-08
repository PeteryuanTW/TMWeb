namespace TMWeb.EFModels
{
    public partial class LogicStatusCondition
    {
        public LogicStatusCondition() { }

        public LogicStatusCondition(Guid? catId)
        {
            Id = Guid.NewGuid();
            CategoryId = catId;
        }
    }
}
