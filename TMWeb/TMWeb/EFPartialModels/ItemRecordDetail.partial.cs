namespace TMWeb.EFModels
{
    public partial class ItemRecordDetail
    {
        public ItemRecordDetail(){}
        public ItemRecordDetail(ItemRecordDetail itemRecordDetail)
        {
            ItemId = itemRecordDetail.ItemId;
            RecordContentId = itemRecordDetail.RecordContentId;
            Value = itemRecordDetail.Value;
        }
        public ItemRecordDetail(ItemDetail itemDetail, ItemRecordContent itemRecordContents)
        {
            ItemId = itemDetail.Id;
            RecordContentId = itemRecordContents.Id;
            Value = string.Empty;
        }
    }
}
