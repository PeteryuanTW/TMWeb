namespace CommonLibrary.MachinePKG.EFModel
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
