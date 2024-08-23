using TMWeb.Data;

namespace TMWeb.EFModels
{
    
    /// <summary>
    /// station type:
    /// *0: 1 wo 1 serial
    /// *1: 1 wo, n serial
    /// *2: 1 wo, no serial
    /// 
    /// *3: n wo, 1 serial
    /// *4: n wo, n serial
    /// *5: n wo, no serial
    /// </summary>
    public partial class Station
    {
        public Station() { }

        public Station(Process process, string name)
        {
            Process = process;
            ProcessId = process.Id;
            Id = Guid.NewGuid();
            ProcessIndex = 0;
            StationType = 0;
            Name = name;
        }



        private Status stationStatus = Status.Uninit;
        public Status Status => stationStatus;

        protected void UIUpdate()
        {
            UIUpdateAct?.Invoke();
        }

        public Action? UIUpdateAct;

        private string errorMsg = String.Empty;
        public string ErrorMsg => errorMsg;
        public void InitStation()
        {
            stationStatus = Status.Init;
            errorMsg = String.Empty;
        }
        public virtual bool SetWorkorder(Workorder wo)
        {
            return true;

        }
        public virtual bool ClearWorkorder()
        {
            return true;
        }
        public virtual void Run()
        {
            stationStatus = Status.Running;
        }
        public virtual void Pause() { }
        public virtual void Stop() { }
        public virtual void Error(string msg)
        {
            errorMsg = msg;
            stationStatus = Status.Error;
        }

    }
}
