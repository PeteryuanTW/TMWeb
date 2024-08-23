using TMWeb.Data;

namespace TMWeb.EFModels
{
    public enum MachineConnectType
    {
        ModbusTCP = 0,
        TMRobot = 1,
        ModbusTCPother = 2,
        WebAPI = 10,

    }
    
    public partial class Machine
    {
        public bool hasCategory => TagCategory != null;
        public bool hasTags => hasCategory && TagCategory?.Tags.Count > 0;
        public bool hasTagsUpdateByTime => hasTags && TagCategory.Tags.Any(x => x.UpdateByTime);

        protected Status status;
        public Status Status => status;
        public string StatusStr => status.ToString();

        //public bool 

        private DateTime initTime;
        private DateTime lastStatusChangedTime;
        private DateTime lastTagUpdateTime;

        private bool runFlag = false;
        public bool RunFlag => runFlag;

        private string errorMsg = string.Empty;
        public string ErrorMsg => errorMsg;

        public Action? MachineStatechangedAct;
        private void MachineStatechanged() => MachineStatechangedAct?.Invoke();

        public Action? TagsStatechangedAct;
        protected void TagsStatechange() => TagsStatechangedAct?.Invoke();



        public void InitMachine()
        {
            status = Status.Init;
            if (hasTags)
            {
                foreach (var item in TagCategory.Tags)
                {
                    item.Init();
                }
            }
            Init();
        }
        public virtual Task ConnectAsync()
        {
            Init();
            return Task.CompletedTask;
        }
        public virtual Task UpdateTag(Tag tag)
        {
            return Task.CompletedTask;
        }
        public virtual Task SetTag(Tag tag, object val)
        {
            return Task.CompletedTask;
        }
        private async Task UpdateTags()
        {
            foreach (Tag tag in TagCategory.Tags)
            {
                if (tag.UpdateByTime)
                {
                    await UpdateTag(tag);
                }
            }
            lastTagUpdateTime = DateTime.Now;
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
                        if (hasTagsUpdateByTime)
                        {
                            await UpdateTags();
                            TagsStatechange();
                        }
                        await UpdateStatus();
                        await Task.Delay(500);
                    }
                }).Start();

            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }

        protected void Init()
        {
            status = Status.Init;
            errorMsg = string.Empty;
            lastStatusChangedTime = DateTime.Now;
            MachineStatechanged();
        }
        protected void Idel()
        {
            status = Status.Idel;
            errorMsg = string.Empty;
            lastStatusChangedTime = DateTime.Now;
            MachineStatechanged();
        }
        public void Running()
        {
            runFlag = true;
            status = Status.Running;
            errorMsg = string.Empty;
            lastStatusChangedTime = DateTime.Now;
            StartUpdating();
            MachineStatechanged();
        }
        protected void Stop()
        {
            runFlag = false;
            lastStatusChangedTime = DateTime.Now;
            MachineStatechanged();
        }
        protected void Disconnect(string msg)
        {
            status = Status.Stop;
            lastStatusChangedTime = DateTime.Now;
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
            
            status = Status.Error;
            errorMsg = msg;
            lastStatusChangedTime = DateTime.Now;
            Stop();
        }
    }
}
