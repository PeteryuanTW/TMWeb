namespace TMWeb.EFModels
{
    public partial class ItemDetail
    {
        public bool HasWorkorder => Workorders != null;
        public string? WorkorderNo => HasWorkorder ? Workorders?.WorkorderNo : string.Empty;
        public string? WorkorderLot => HasWorkorder ? Workorders?.Lot : string.Empty;

        public ItemDetail() { }
        public ItemDetail(Guid workorderID, string serialNo)
        {
            Id = Guid.NewGuid();
            WorkordersId = workorderID;
            SerialNo = serialNo;
            TargetAmount = 1;
            Okamount = 0;
            Ngamount = 0;
            StartTime = DateTime.Now;
        }
        public ItemDetail(Workorder wo, string serialNo)
        {
            Id = Guid.NewGuid();
            WorkordersId = wo.Id;
            SerialNo = serialNo;
            TargetAmount = 1;
            Okamount = 0;
            Ngamount = 0;
            StartTime = DateTime.Now;
        }
        public ItemDetail(Workorder wo)
        {
            Id = Guid.NewGuid();
            WorkordersId = wo.Id;
            SerialNo = string.Empty;
            TargetAmount = 1;
            Okamount = 0;
            Ngamount = 0;
            StartTime = DateTime.Now;
        }
    }
}
