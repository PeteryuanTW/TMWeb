namespace TMWeb.EFModels
{
    public enum MachineConnectType
    {
        ModbusTCP = 0,
        TMRobot = 1,
        ModbusTCPother = 2,
        WebAPI = 10,

    }
    public enum MachineStatus
    {
        Init,
        Disconnect,
        Running,
        Idel,
        Error,
    }
    public partial class Machine
    {
        public bool hasTags => TagCategory != null;
        public string TagCategoryName => hasTags ? TagCategory?.Name : string.Empty;

        public string ConnectionTypeStr => ((MachineConnectType)ConnectionType).ToString();

        protected MachineStatus status;
        public MachineStatus Status => status;
        public string StatusStr => status.ToString();

        private bool runFlag = false;
        public bool RunFlag => runFlag;

        private string errorMsg = string.Empty;
        public string ErrorMsg => errorMsg;

        public virtual void InitMachine()
        {
            status = MachineStatus.Init;
            errorMsg = string.Empty;

        }
        public virtual Task ConnectAsync()
        {
            status = MachineStatus.Init;
            errorMsg = string.Empty;
            return Task.CompletedTask;
        }
        protected virtual Task UpdateStatus()
        {
            return Task.CompletedTask;
        }
        private void StartUpdating()
        {
            try
            {
                new Thread(async () =>
                {
                    while (runFlag)
                    {
                        await UpdateStatus();
                        await Task.Delay(500);
                    }
                });

            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }
        protected void Idel()
        {
            status = MachineStatus.Idel;
            errorMsg = string.Empty;
        }
        public void Running()
        {
            runFlag = true;
            status = MachineStatus.Running;
            errorMsg = string.Empty;
            StartUpdating();
        }
        protected void Stop()
        {
            runFlag = false;
        }
        protected void Disconnect(string msg)
        {
            status = MachineStatus.Disconnect;
            if (!string.IsNullOrEmpty(msg))
            {
                errorMsg = msg;
            }
            else
            {
                errorMsg = string.Empty;
            }
            Stop();
        }
        protected void Error(string msg)
        {
            Stop();
            status = MachineStatus.Error;
            errorMsg = msg;
        }
    }
}
