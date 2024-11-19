using TMWeb.Services;

namespace TMWeb.Scripts.Template
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
        protected readonly TMWebShopfloorService tmWebShopfloorService;
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
                        await Task.Delay(500);
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
        private void Error(string s)
        {
            status = ScriptStatus.Error;
            StatusChanged();
            WriteLog($"Script error({s})");
        }

        public void Resume()
        {
            status = ScriptStatus.Running;
            StatusChanged();
            WriteLog($"Script resume");
        }

        public void Stop()
        {
            status = ScriptStatus.Stop;
            StatusChanged();
            WriteLog($"Script stop");
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
