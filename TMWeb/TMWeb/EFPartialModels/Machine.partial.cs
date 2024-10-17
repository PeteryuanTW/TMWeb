using CommonLibrary.API.Message;
using NModbus;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using TMWeb.Data;

namespace TMWeb.EFModels
{


    public partial class Machine : IDisposable
    {
        public int MaxRetryCount => 5;

        protected int retryCount = 0;

        public int RetryCount => retryCount;

        public Machine() { }

        public Machine(Guid id)
        {
            this.Id = id;
        }
        public bool hasCategory => TagCategory != null;
        public bool hasTags => hasCategory && TagCategory?.Tags.Count > 0;
        public bool hasTagsUpdateByTime => hasTags && TagCategory.Tags.Any(x => x.UpdateByTime);

        protected Status status;
        public Status Status => status;
        public string StatusStr => status.ToString();

        public bool tagUsable => status != Status.Init && status != Status.Disconnect && status != Status.TryConnecting && status != Status.Error;

        //public bool 

        private DateTime initTime;
        private DateTime lastStatusChangedTime;
        private DateTime lastTagUpdateTime;

        private bool runFlag => status != Status.Init && status != Status.Disconnect && status != Status.TryConnecting;
        public bool RunFlag => runFlag;

        private string errorMsg = string.Empty;
        public string ErrorMsg => errorMsg;

        public Action<Status>? MachineStatechangedAct;
        private void MachineStatechanged() => MachineStatechangedAct?.Invoke(status);

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
        public virtual Task<RequestResult> UpdateTag(Tag tag)
        {
            return Task.FromResult(new RequestResult(3, "Not implement yet"));
        }
        public virtual Task<RequestResult> SetTag(string tagName, object val)
        {
            return Task.FromResult(new RequestResult(3, "Not implement yet"));
        }
        public virtual Task<RequestResult> SetTag(Tag tag, object val)
        {
            return Task.FromResult(new RequestResult(3, "Not implement yet"));
        }
        private async Task UpdateTags()
        {
            foreach (Tag tag in TagCategory.Tags)
            {
                if (tag.UpdateByTime)
                {
                    var res = await UpdateTag(tag);
                    //Console.WriteLine(res.ReturnCode+": "+res.Msg);
                }
            }
            lastTagUpdateTime = DateTime.Now;
        }
        protected virtual Task UpdateStatus()
        {
            return Task.CompletedTask;
        }
        public void StartUpdating()
        {
            try
            {
                new Thread(async () =>
                {
                    while (Enabled)
                    {
                        try
                        {
                            if (runFlag)
                            {
                                await UpdateStatus();
                                if (hasTagsUpdateByTime)
                                {
                                    await UpdateTags();
                                    TagsStatechange();
                                }

                            }
                            else
                            {
                                if (status == Status.Disconnect || status == Status.Init)
                                {
                                    if (retryCount < MaxRetryCount)
                                    {
                                        await ConnectAsync();
                                    }
                                }
                                else if (status == Status.TryConnecting)
                                {

                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            Disconnect(ex.Message);
                        }
                        catch (SocketException e)
                        {
                            Disconnect(e.Message);
                        }
                        catch (Exception e)
                        {
                            Error(e.Message);
                        }
                        finally
                        {
                            await Task.Delay(500);
                        }
                    }
                }
                ).Start();

            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }

        protected void Init()
        {
            if (status != Status.Init)
            {
                status = Status.Init;
                errorMsg = string.Empty;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
        protected void Idel()
        {
            if (status != Status.Idel)
            {
                status = Status.Idel;
                errorMsg = string.Empty;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
        protected void TryConnecting()
        {
            if (status != Status.TryConnecting)
            {
                status = Status.TryConnecting;
                errorMsg = string.Empty;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
        public void Running()
        {
            if (status != Status.Running)
            {
                status = Status.Running;
                errorMsg = string.Empty;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
        protected void Pause()
        {
            if (status != Status.Pause)
            {
                status = Status.Pause;
                errorMsg = string.Empty;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
        protected void Stop()
        {
            if (status != Status.Stop)
            {
                status = Status.Stop;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
        protected void Disconnect(string msg)
        {
            if (status != Status.Disconnect)
            {
                status = Status.Disconnect;
                lastStatusChangedTime = DateTime.Now;
                if (!string.IsNullOrEmpty(msg))
                {
                    errorMsg = msg;
                }
                else
                {
                    errorMsg = string.Empty;
                }
                MachineStatechanged();
            }
        }
        protected void Error(string msg)
        {
            if (status != Status.Error)
            {
                status = Status.Error;
                errorMsg = msg;
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }




        //dispose
        public void Dispose()
        {
            Enabled = false;
        }
    }
}
