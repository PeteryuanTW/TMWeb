using TMWeb.EFModels;

namespace TMWeb.Data.DTO
{
    public class ItemDetailDTO
    {
        public string Process { get; set; } = null!;
        public string WorkorderNo {  get; set; } = null!;
        public string Lot { get; set; } = null!;
        public string PartNo { get; set; } = null!;
        public string? SerialNo { get; set; }
        public int TargetAmount { get; set; }
        public int Okamount { get; set; }
        public int Ngamount { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? FinishedTime { get; set; }


    }
}
