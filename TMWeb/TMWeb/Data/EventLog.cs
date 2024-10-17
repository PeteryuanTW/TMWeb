using CommonLibrary.API.Message;

namespace TMWeb.Data
{
    public class EventLog
    {
        public DateTime OccuredTime { get; init; }
        public Guid Id { get; init; }

        private RequestResult requestResult { get; init; }

        public int ReturnCode => requestResult.ReturnCode;
        public string Msg => requestResult.Msg;

        public EventLog(RequestResult requestResult)
        {
            OccuredTime = DateTime.Now;
            Id = Guid.NewGuid();
            this.requestResult = requestResult;
        }
    }
}
