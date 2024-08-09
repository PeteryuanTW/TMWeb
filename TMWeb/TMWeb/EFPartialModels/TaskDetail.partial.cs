namespace TMWeb.EFModels
{
    public partial class TaskDetail
    {
        public TaskDetail() { }

        public TaskDetail(Station station, ItemDetail itemDetail)
        {
            Id = Guid.NewGuid();
            ItemId = itemDetail.Id;
            StationId = station.Id;
            SerialNo = itemDetail.SerialNo;
            TargetAmount = itemDetail.TargetAmount;
            Okamount = 0;
            Ngamount = 0;
            StartTime = DateTime.Now;
        }
    }
}
