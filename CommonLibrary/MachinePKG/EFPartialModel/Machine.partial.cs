using CommonLibrary.API.Message;
using CommonLibrary.MachinePKG;
using CommonLibrary.MachinePKG.EFModel;
using NModbus;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace CommonLibrary.MachinePKG.EFModel
{


    public partial class Machine: IDisposable
    {

        protected int retryCount = 0;

        public int RetryCount => retryCount;

        public bool isAutoRetry => retryCount < MaxRetryCount;

        public Machine() { }

        public Machine(Machine machine)
        {
            Id = machine.Id;
            //ProcessId = machine.ProcessId;
            Name = machine.Name;
            Ip = machine.Ip;
            Port = machine.Port;
            ConnectionType = machine.ConnectionType;
            MaxRetryCount = machine.MaxRetryCount;
            Enabled = machine.Enabled;
            TagCategoryId = machine.TagCategoryId;
            UpdateDelay = machine.UpdateDelay;

            if (machine.hasCategory)
            {
                TagCategory = new TagCategory
                {
                    Id = machine.TagCategory.Id,
                    Name = machine.TagCategory.Name,
                    ConnectionType = machine.ConnectionType,

                    Tags = machine.TagCategory.Tags,
                };
            }

            if (machine.hasCustomStatusCategory)
            {
                LogicStatusCategory = new LogicStatusCategory
                {
                    Id = machine.LogicStatusCategoryId,
                    Name = machine.LogicStatusCategory.Name,
                    DataType = machine.LogicStatusCategory.DataType,

                    LogicStatusConditions = machine.LogicStatusCategory.LogicStatusConditions,
                };
            }

            if (machine.hasErrorCodeCategory)
            {
                ErrorCodeCategory = new ErrorCodeCategory
                {
                    Id = machine.ErrorCodeCategoryId,
                    Name = machine.ErrorCodeCategory.Name,
                    DataType = machine.ErrorCodeCategory.DataType,

                    ErrorCodeMappings = machine.ErrorCodeCategory.ErrorCodeMappings,
                };
            }
        }

        public Machine(Guid id)
        {
            this.Id = id;
        }
        public bool hasCategory => TagCategory != null;
        public bool hasTags => hasCategory && TagCategory?.Tags.Count > 0;
        public bool hasTagsUpdateByTime => hasTags && TagCategory.Tags.Any(x => x.UpdateByTime);

        public bool hasCustomStatusCategory => LogicStatusCategory != null;

        public bool hasCustomStatusCondition => hasCustomStatusCategory && LogicStatusCategory?.LogicStatusConditions.Count > 0;

        public bool hasErrorCodeCategory => ErrorCodeCategory != null;

        public bool hasErrorCodeMapping => hasErrorCodeCategory && ErrorCodeCategory?.ErrorCodeMappings.Count > 0;

        protected Status status;
        public Status MachineStatus => status;
        public string StatusStr => status.ToString();

        public bool tagUsable => status != Status.Init && status != Status.Disconnect && status != Status.TryConnecting && status != Status.Error;

        protected Status customStatus;
        public Status CustomStatus => customStatus;
        public string CustomStatusStr => customStatus.ToString();

        //private DateTime initTime;
        private DateTime lastStatusChangedTime;
        private DateTime lastTagUpdateTime;

        private bool runFlag => status != Status.Init && status != Status.Disconnect && status != Status.TryConnecting;
        public bool RunFlag => runFlag;

        public bool canManualRetryFlag => isAutoRetry?false:status is Status.Disconnect || status is Status.Error;

        private string errorMsg = string.Empty;
        public string ErrorMsg => errorMsg;

        private string errorCodeDescription = string.Empty;

        public string ErrorCodeDescription => errorCodeDescription;


        public Action<Status>? MachineStatechangedAct;
        private void MachineStatechanged() => MachineStatechangedAct?.Invoke(status);

        public Action? CustomStatusChangedAct;
        private void CustomStatusChange() => CustomStatusChangedAct?.Invoke();

        public Action? ErrorCodeDescriptionChangedAct;
        private void ErrorCodeDescriptionChange() => ErrorCodeDescriptionChangedAct?.Invoke();


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

        private Task UpdateCustomStatus()
        {
            if (hasTags && hasCustomStatusCondition)
            {
                var customStatusTag = TagCategory?.Tags.FirstOrDefault(x => (SpecialTagType)x.SpecialType == SpecialTagType.CustomStatus && x.DataType == LogicStatusCategory?.DataType);
                if (customStatusTag is not null)
                {
                    var connditionStatus = LogicStatusCategory?.LogicStatusConditions.FirstOrDefault(x => x.ConditionString == customStatusTag.ValueString);
                    if (connditionStatus is not null)
                    {
                        SetCustomStatus((Status)connditionStatus.Status);
                    }
                    else
                    {
                        SetCustomStatus(Status.Init);
                    }
                }
            }
            else
            {
                SetCustomStatus(Status.Init);
            }
            return Task.CompletedTask;
        }

        private Task UpdateErrorCode()
        {
            if (hasTags && hasErrorCodeMapping)
            {
                var customErrorCodeTag = TagCategory?.Tags.FirstOrDefault(x => (SpecialTagType)x.SpecialType == SpecialTagType.DetailCode && x.DataType == LogicStatusCategory?.DataType);
                if (customErrorCodeTag is not null)
                {
                    var errorCodeMapping = ErrorCodeCategory?.ErrorCodeMappings.FirstOrDefault(x => x.ConditionString == customErrorCodeTag.ValueString);
                    if (errorCodeMapping is not null)
                    {
                        SetErrorDescription(errorCodeMapping.Description);
                    }
                    else
                    {
                        SetErrorDescription(string.Empty);
                    }
                }
            }
            else
            {
                SetErrorDescription(string.Empty);
            }
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
                            var a = UpdateDelay;
                            if (runFlag)
                            {
                                await UpdateStatus();
                                if (hasTagsUpdateByTime)
                                {
                                    await UpdateTags();
                                    TagsStatechange();
                                }
                                await UpdateCustomStatus();
                                await UpdateErrorCode();
                            }
                            else
                            {
                                if (status == Status.Disconnect || status == Status.Init)
                                {
                                    if (MaxRetryCount == -1)
                                    {
                                        await ConnectAsync();

                                    }
                                    else
                                    {
                                        if (retryCount < MaxRetryCount)
                                        {
                                            await ConnectAsync();
                                        }
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
                            await Task.Delay(UpdateDelay);
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
            if (status != Status.Idle)
            {
                status = Status.Idle;
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

        protected void SetCustomStatus(Status status)
        {
            if (customStatus != status)
            {
                customStatus = status;
                CustomStatusChange();
            }
        }

        protected void SetErrorDescription(string description)
        {
            if (errorCodeDescription != description)
            {
                errorCodeDescription = description;
                ErrorCodeDescriptionChange();
            }
        }
        //dispose
        public void Dispose()
        {
            Enabled = false;
        }
    }
}
