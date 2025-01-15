using TMWeb.Services;
using Newtonsoft.Json;

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

        private int maxLogCount = 20;
        public int MaxLogCount => maxLogCount;
        public void SetMaxLogCount(int count)
            =>maxLogCount = count;

        private int delayMilliseconds = 1000;
        public int DelayMilliseconds => delayMilliseconds;
        public void SetdelayMilliseconds(int ms)
            => delayMilliseconds = ms;

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
        public virtual async Task OnStartAsync()
        {
            await Task.CompletedTask;
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
            await OnStartAsync();
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
                        await Task.Delay(delayMilliseconds);
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
            OnStop();
            status = ScriptStatus.Stop;
            StatusChanged();
            WriteLog($"Script stop");
        }
        public void WriteLog(string log)
        {
            _Log += $"[{DateTime.Now.ToString("HH:mm:ss:ff")}]: {log}\r\n";
            logCount++;
            if (logCount > maxLogCount)
            {
                _Log = string.Join("\r\n", _Log.Split("\r\n").Skip(1).ToList());
            }
            StatusChanged();
        }
    }
}
