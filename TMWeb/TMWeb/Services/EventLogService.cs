using CommonLibrary.API.Message;
using TMWeb.Data;

namespace TMWeb.Services
{
    public class EventLogService
    {
        private List<EventLog> eventLogs;
        public List<EventLog> EventLogs => eventLogs;
        public EventLogService()
        {
            eventLogs = new List<EventLog>()
            {
                new EventLog(new RequestResult(1, "info test")),
                new EventLog(new RequestResult(2, "success test")),
                new EventLog(new RequestResult(3, "warning test")),
                new EventLog(new RequestResult(4, "error test")),
            };
        }

        public Task AddEventLog(RequestResult requestResult)
        {
            eventLogs.Add(new EventLog(requestResult));
            OnEventLogChanged();
            return Task.CompletedTask;
        }
        public Action? EventLogChangedAct;
        private void OnEventLogChanged() => EventLogChangedAct?.Invoke();

    }
}
