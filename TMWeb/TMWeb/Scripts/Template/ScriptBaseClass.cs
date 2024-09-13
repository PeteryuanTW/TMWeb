using TMWeb.Services;

namespace TMWeb
{
    public enum ScriptStatus
    {
        Init,
        Running,
        Pause,
        Error,
        Stop,
    }
    public class ScriptBaseClass
    {
        private readonly TMWebShopfloorService tmWebShopfloorService;
        private ScriptStatus status;
        public ScriptStatus Status => status;

        private string _Log = string.Empty;
        public string Log => _Log;
        private int logCount = 0;

        public Action? StatusChangedAct;
        private void StatusChanged() => StatusChangedAct?.Invoke();

        public ScriptBaseClass(TMWebShopfloorService service)
        {
            tmWebShopfloorService = service;
            status = ScriptStatus.Init;
        }
        public virtual void OnStart()
        {

        }
        public virtual Task RunAction()
        {
            return Task.CompletedTask;
        }
        public virtual void OnStop()
        {

        }

        public async Task StartScript()
        {
            status = ScriptStatus.Running;
            StatusChanged();
            WriteLog($"Script start");
            OnStart();
            new Thread(async () =>
            {
                while (status == ScriptStatus.Running || status == ScriptStatus.Pause || status == ScriptStatus.Error)
                {
                    try
                    {
                        if (status == ScriptStatus.Running)
                        {
                            await RunAction();
                        }
                        else
                        {

                        }
                    }
                    catch (Exception e)
                    {
                        Error(e.Message);
                        break;
                    }
                    finally
                    {
                        await Task.Delay(1000);
                    }

                }
            }).Start();
        }

        public void Pause()
        {
            status = ScriptStatus.Pause;
            StatusChanged();
            WriteLog($"Script pause");
        }
        public void Error(string s)
        {
            status = ScriptStatus.Error;
            StatusChanged();
            WriteLog($"Script error({s})");
        }
        public void WriteLog(string log)
        {
            _Log += $"[{DateTime.Now.ToString("HH:mm:ss:ff")}]: {log}\r\n";
            logCount++;
            if (logCount > 20)
            {
                _Log = string.Join("\r\n", _Log.Split("\r\n").Skip(1).ToList());
            }
            StatusChanged();
        }
    }
}
