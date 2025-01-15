namespace TMWeb.EFModels
{
    public class SerilogData
    {
        public int Id { get; set; }
        public string? Msg { get; set; }
        public DateTime TimeStamp { get; set; }
        public int? Severity { get; set; }
        public string? Caller { get; set; }
        public string? Method { get; set; }
        public int? Row { get; set; }
        public int? Column { get; set; }



    }
}
