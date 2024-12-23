using Azure;
using CommonLibrary.API.Message;
using CommonLibrary.MachinePKG.AnalysisData;
using CommonLibrary.MachinePKG.EFModel;
using CommonLibrary.MachinePKG.MachineData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.Service
{
    public class MachineService : IMachineService
    {
        private readonly IServiceScopeFactory scopeFactory;
        public MachineService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }


        #region machine
        private List<Machine> machines = new();

        Action<Guid, DataEditMode>? IMachineService.MachineConfigChangedAct { get; set; }

        List<Machine> IMachineService.Machines => machines;

        Task<List<Machine>> IMachineService.GetAllMachinesConfig()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.Machines.AsNoTracking().ToList());
            }
        }

        async Task IMachineService.InitAllMachinesFromDB()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                var tmp = dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags)
                    .Include(x => x.LogicStatusCategory).ThenInclude(x => x.LogicStatusConditions)
                    .Include(x => x.ErrorCodeCategory).ThenInclude(x => x.ErrorCodeMappings)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .ToList();
                machines = tmp.Select(x => (this as IMachineService).InitMachineToDerivesClass(x)).ToList();
                List<Task> tasks = new();
                foreach (Machine machine in machines)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        machine.InitMachine();
                        if (machine.Enabled)
                        {
                            machine.StartUpdating();
                        }
                    }));

                }
                await Task.WhenAll(tasks);
            }
        }

        Machine? IMachineService.InitMachineFromDBById(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                var tmp = dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags)
                    .Include(x => x.LogicStatusCategory).ThenInclude(x => x.LogicStatusConditions)
                    .Include(x => x.ErrorCodeCategory).ThenInclude(x => x.ErrorCodeMappings)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == id);
                tmp = (this as IMachineService).InitMachineToDerivesClass(tmp);
                tmp.InitMachine();
                if (tmp.Enabled)
                {
                    tmp.StartUpdating();
                }
                return tmp;
            }
        }

        async Task<RequestResult> IMachineService.UpsertMachineConfig(Machine machine)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var target = dbContext.Machines.FirstOrDefault(x => x.Id == machine.Id);
                    bool exist = target is not null;
                    if (exist)
                    {
                        target.Name = machine.Name;
                        //target.ProcessId = machine.ProcessId;
                        target.Ip = machine.Ip;
                        target.Port = machine.Port;
                        target.ConnectionType = machine.ConnectionType;
                        target.MaxRetryCount = machine.MaxRetryCount;
                        target.TagCategoryId = machine.TagCategoryId;
                        target.LogicStatusCategoryId = machine.LogicStatusCategoryId;
                        target.ErrorCodeCategoryId = machine.ErrorCodeCategoryId;
                        target.Enabled = machine.Enabled;
                        target.UpdateDelay = machine.UpdateDelay;
                        target.RecordStatusChanged = machine.RecordStatusChanged;
                    }
                    else
                    {
                        await dbContext.Machines.AddAsync(machine);
                    }
                    await dbContext.SaveChangesAsync();
                    DataEditMode dataEditMode = exist ? DataEditMode.Update : DataEditMode.Insert;
                    await (this as IMachineService).RefreshMachine(machine, dataEditMode);
                    return new(2, $"upsert machine {machine.Name} success");
                }
                catch (Exception e)
                {
                    return new(4, $"upsert machine {machine.Name} fail({e.Message})");
                }

            }
        }

        async Task<RequestResult> IMachineService.DeleteMachine(Machine machine)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var target = dbContext.Machines.FirstOrDefault(x => x.Id == machine.Id);
                    if (target != null)
                    {
                        dbContext.Remove(target);
                        await dbContext.SaveChangesAsync();
                        await (this as IMachineService).RefreshMachine(target, DataEditMode.Delete);
                        return new(2, $"Delete machine {machine.Name} success");
                    }
                    else
                    {
                        return new(4, $"Machine {machine.Name} not found");
                    }

                }
                catch (Exception e)
                {
                    return new(4, $"Delete machine {machine.Name} fail({e.Message})");
                }

            }
        }

        Task<Machine?> IMachineService.GetMachineByID(Guid? id)
        {
            return Task.FromResult(machines.FirstOrDefault(x => x.Id == id));
        }

        Task<Machine?> IMachineService.GetMachineByName(string name)
        {
            return Task.FromResult(machines.FirstOrDefault(x => x.Name == name));
        }

        Machine IMachineService.InitMachineToDerivesClass(Machine machine)
        {
            Machine res;
            switch (machine.ConnectionType)
            {
                case 0:
                    res = new ModbusTCPMachine(machine);
                    break;
                case 1:
                    res = new TMRobotModbusTCP(machine);
                    break;
                case 2:
                    res = machine;
                    break;
                case 10:
                    res = new WebAPIMachine(machine);
                    break;
                case 20:
                    res = new ConveyorMachine(machine);
                    break;
                case 21:
                    res = new WrappingMachine(machine);
                    break;
                case 78:
                    res = new RegalscanRFIDMachine(machine);
                    break;
                default:
                    res = machine;
                    break;
            }
            res.MachineStatechangedRecordAct += ((IMachineService)this).MachineStatusChangedRecord;
            return res;
        }

        void IMachineService.MachineConfigChanged(Guid id, DataEditMode mode)
        {
            (this as IMachineService).MachineConfigChangedAct?.Invoke(id, mode);
        }

        async Task IMachineService.RefreshMachine(Machine machine, DataEditMode dataEditMode)
        {
            var target = await (this as IMachineService).GetMachineByID(machine.Id);
            if (target != null)
            {
                //update or delete
                target.MachineStatechangedRecordAct += ((IMachineService)this).MachineStatusChangedRecord;
                machines.Remove(target);
                target.Dispose();

                if (dataEditMode != DataEditMode.Delete)
                {
                    machines.Add((this as IMachineService).InitMachineFromDBById(machine.Id));
                }
                else
                {
                }
            }
            else
            {
                machines.Add((this as IMachineService).InitMachineFromDBById(machine.Id));
            }
            (this as IMachineService).MachineConfigChanged(machine.Id, dataEditMode);
        }

        async void IMachineService.MachineStatusChangedRecord(Machine machine, MachineStatusRecordType machineStatusRecordType)
        {
            if (!machine.RecordStatusChanged)
            {
                return;
            }
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var newRocord = new MachineStatusLog
                    {
                        Id = Guid.NewGuid(),
                        MachineID = machine.Id,
                        Status = (int)machine.MachineStatus,
                        LogTime = DateTime.Now,
                    };
                    await dbContext.MachineStatusLogs.AddAsync(newRocord);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region utilization

        Task<List<MachineStatusLog>> IMachineService.GetMachineStatusLogByID(MachineUtilizationDTO machineUtilizationDTO)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.MachineStatusLogs.Where(x=>x.MachineID == machineUtilizationDTO.MachineID && x.LogTime>= machineUtilizationDTO.Start && x.LogTime<= machineUtilizationDTO.End).OrderBy(x=>x.LogTime).AsNoTracking().ToList());
            }
        }

        async IAsyncEnumerable<MachineStatusInterval> IMachineService.CalculateMachineStatusIntervalByOrderedLog(List<MachineStatusLog> machineStatusLogs, ushort delayMilliSec, IProgress<int>? progress)
        {
            int totalCount = machineStatusLogs.Count();
            progress?.Report(0);
            for (int i = 0; i < totalCount; i++)
            {
                if (i == totalCount - 1)
                {
                    //res.Add(new(machineStatusLogs[i].LogTime, DateTime.Now, (Status)machineStatusLogs[i].Status));
                    yield return new(machineStatusLogs[i].LogTime, DateTime.Now, (Status)machineStatusLogs[i].Status);
                }
                else
                {
                    //res.Add(new(machineStatusLogs[i].LogTime, machineStatusLogs[i + 1].LogTime, (Status)machineStatusLogs[i].Status));
                    yield return new(machineStatusLogs[i].LogTime, machineStatusLogs[i + 1].LogTime, (Status)machineStatusLogs[i].Status);
                }
                await Task.Delay(delayMilliSec);
                progress?.Report(i * 100 / totalCount);
            }
        }

        #endregion

        #region tag
        Task<List<TagCategory>> IMachineService.GetAllTagCategories()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.TagCategories.AsNoTracking().ToList());
            }
        }

        Task<List<TagCategory>> IMachineService.GetAllTagCategoriesWithTags()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.TagCategories.Include(x => x.Tags).AsNoTracking().ToList());
            }
        }

        List<Tag> IMachineService.GetTagsByCatId(Guid? catID)
        {
            if (catID is null)
            {
                return new List<Tag>();
            }
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                var targetCat = dbContext.TagCategories.Include(x => x.Tags).AsNoTracking().FirstOrDefault(x => x.Id == catID);
                if (targetCat is not null)
                {
                    return targetCat.Tags.ToList();
                }
                else
                {
                    return new List<Tag>();
                }
            }
        }

        int IMachineService.GetTagTypeCodeByIds(Guid? catID, Guid? tagID)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                var targetTag = dbContext.Tags.FirstOrDefault(x => x.CategoryId == catID && x.Id == tagID);
                return targetTag is null ? 0 : targetTag.DataType;
            }
        }

        Task<List<TagCategory>> IMachineService.GetCategoryByConnectionType(int connectionType)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.TagCategories.Where(x => x.ConnectionType == connectionType).ToList());
            }
        }

        async Task<RequestResult> IMachineService.UpsertTagCategory(TagCategory tagCategory)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetTagCat = dbContext.TagCategories.FirstOrDefault(x => x.Id == tagCategory.Id);
                    if (targetTagCat != null)
                    {
                        targetTagCat.Name = tagCategory.Name;
                        targetTagCat.ConnectionType = tagCategory.ConnectionType;
                    }
                    else
                    {
                        await dbContext.TagCategories.AddAsync(tagCategory);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert tag category {tagCategory.Name} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.DeleteTagCategory(TagCategory tagCategory)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetTagCat = dbContext.TagCategories.Include(x => x.Tags).FirstOrDefault(x => x.Id == tagCategory.Id);
                    if (targetTagCat != null)
                    {
                        dbContext.TagCategories.Remove(targetTagCat);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete tag category {targetTagCat.Name} success");
                    }
                    else
                    {
                        return new(4, $"Tag category {targetTagCat.Name} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.UpsertTag(Tag tag)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetTag = dbContext.Tags.FirstOrDefault(x => x.Id == tag.Id);
                    if (targetTag != null)
                    {
                        targetTag.Name = tag.Name;
                        targetTag.DataType = tag.DataType;
                        targetTag.UpdateByTime = tag.UpdateByTime;
                        targetTag.SpecialType = tag.SpecialType;

                        targetTag.Bool1 = tag.Bool1;
                        targetTag.Bool2 = tag.Bool2;
                        targetTag.Bool3 = tag.Bool3;
                        targetTag.Bool4 = tag.Bool4;
                        targetTag.Bool5 = tag.Bool5;

                        targetTag.Int1 = tag.Int1;
                        targetTag.Int2 = tag.Int2;
                        targetTag.Int3 = tag.Int3;
                        targetTag.Int4 = tag.Int4;
                        targetTag.Int5 = tag.Int5;

                        targetTag.String1 = tag.String1;
                        targetTag.String2 = tag.String2;
                        targetTag.String3 = tag.String3;
                        targetTag.String4 = tag.String4;
                        targetTag.String5 = tag.String5;
                    }
                    else
                    {
                        await dbContext.Tags.AddAsync(tag);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert tag {tag.Name} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.DeleteTag(Tag tag)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetTag = dbContext.Tags.FirstOrDefault(x => x.Id == tag.Id);
                    if (targetTag != null)
                    {
                        dbContext.Tags.Remove(targetTag);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete tag {tag.Name} success");
                    }
                    else
                    {
                        return new(4, $"Tag {tag.Name} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<Tag?> IMachineService.GetMachineTag(string machineName, string tagName)
        {
            Machine? targetMachine = await (this as IMachineService).GetMachineByName(machineName);
            if (targetMachine != null)
            {
                if (targetMachine.hasCategory)
                {
                    Tag? targetTag = targetMachine.TagCategory.Tags.FirstOrDefault(x => x.Name == tagName);
                    if (targetTag != null)
                    {
                        if (!targetTag.UpdateByTime)
                        {
                            await targetMachine.UpdateTag(targetTag);
                        }
                        return targetTag;
                    }
                }
            }
            return null;
        }

        async Task<RequestResult> IMachineService.SetMachineTag(string machineName, string tagName, object val)
        {
            Machine? targetMachine = await (this as IMachineService).GetMachineByName(machineName);
            if (targetMachine != null)
            {
                if (targetMachine.hasCategory)
                {
                    Tag? targetTag = targetMachine.TagCategory.Tags.FirstOrDefault(x => x.Name == tagName);
                    if (targetTag != null)
                    {
                        return await targetMachine.SetTag(targetTag.Name, val);
                    }
                    else
                    {
                        return new(4, $"Tag {tagName} not found in machine {machineName}");
                    }
                }
                else
                {
                    return new(4, $"Machine tag category not set");
                }
            }
            else
            {
                return new(4, $"Machine {machineName} not found");
            }
        }


        #endregion

        #region StatusCondition

        Task<List<LogicStatusCategory>> IMachineService.GetCustomStatusAndConditions()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.LogicStatusCategories.Include(x => x.LogicStatusConditions).AsNoTracking().ToList());
            }
        }

        async Task<RequestResult> IMachineService.UpsertCustomStatusCategory(LogicStatusCategory tagCategory)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetTagCat = dbContext.LogicStatusCategories.FirstOrDefault(x => x.Id == tagCategory.Id);
                    if (targetTagCat != null)
                    {
                        targetTagCat.Name = tagCategory.Name;
                        targetTagCat.DataType = tagCategory.DataType;
                    }
                    else
                    {
                        await dbContext.LogicStatusCategories.AddAsync(tagCategory);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert custom status category {tagCategory.Name} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.DeleteCustomStatusCategory(LogicStatusCategory tagCategory)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targeLogicStatusCat = dbContext.LogicStatusCategories.FirstOrDefault(x => x.Id == tagCategory.Id);
                    if (targeLogicStatusCat != null)
                    {
                        dbContext.LogicStatusCategories.Remove(targeLogicStatusCat);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete tag category {targeLogicStatusCat.Name} success");
                    }
                    else
                    {
                        return new(4, $"Tag category {tagCategory.Name} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.UpsertCustomStatusCondition(LogicStatusCondition condition)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetCondition = dbContext.LogicStatusCondictions.FirstOrDefault(x => x.Id == condition.Id);
                    if (targetCondition != null)
                    {
                        targetCondition.ConditionString = condition.ConditionString;
                        targetCondition.Status = condition.Status;
                    }
                    else
                    {
                        await dbContext.LogicStatusCondictions.AddAsync(condition);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert custom status condition {condition.ConditionString} as {(Status)condition.Status} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.DeleteCustomStatusCondition(LogicStatusCondition condition)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetCondition = dbContext.LogicStatusCondictions.FirstOrDefault(x => x.Id == condition.Id);
                    if (targetCondition != null)
                    {
                        dbContext.LogicStatusCondictions.Remove(targetCondition);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete tag category {targetCondition.ConditionString} as {(Status)targetCondition.Status} success");
                    }
                    else
                    {
                        return new(4, $"Custom condition {condition.ConditionString} as {(Status)condition.Status} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        #endregion

        #region error code

        Task<List<ErrorCodeCategory>> IMachineService.GetErrorCodeTables()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                return Task.FromResult(dbContext.ErrorCodeCategories.Include(x => x.ErrorCodeMappings).AsNoTracking().ToList());
            }
        }

        async Task<RequestResult> IMachineService.UpsertErrorCodeCategory(ErrorCodeCategory errorCodeCategory)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetErrorCodeCat = dbContext.ErrorCodeCategories.FirstOrDefault(x => x.Id == errorCodeCategory.Id);
                    if (targetErrorCodeCat != null)
                    {
                        targetErrorCodeCat.Name = errorCodeCategory.Name;
                        targetErrorCodeCat.DataType = errorCodeCategory.DataType;
                    }
                    else
                    {
                        await dbContext.ErrorCodeCategories.AddAsync(errorCodeCategory);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert Error Code category {errorCodeCategory.Name} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.DeleteErrorCodeCategory(ErrorCodeCategory errorCodeCategory)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targeErrorCodeCat = dbContext.ErrorCodeCategories.FirstOrDefault(x => x.Id == errorCodeCategory.Id);
                    if (targeErrorCodeCat != null)
                    {
                        dbContext.ErrorCodeCategories.Remove(targeErrorCodeCat);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete error code category {targeErrorCodeCat.Name} success");
                    }
                    else
                    {
                        return new(4, $"Error code category {errorCodeCategory.Name} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.UpsertErrorCodeMapping(ErrorCodeMapping errorCodeMapping)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetCodeMapping = dbContext.ErrorCodeMappings.FirstOrDefault(x => x.Id == errorCodeMapping.Id);
                    if (targetCodeMapping != null)
                    {
                        targetCodeMapping.ConditionString = errorCodeMapping.ConditionString;
                        targetCodeMapping.Description = errorCodeMapping.Description;
                    }
                    else
                    {
                        await dbContext.ErrorCodeMappings.AddAsync(errorCodeMapping);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert error code {errorCodeMapping.ConditionString} : {errorCodeMapping.Description} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        async Task<RequestResult> IMachineService.DeleteErrorCodeMapping(ErrorCodeMapping errorCodeMapping)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MachineDBContext>();
                    var targetErrorCodeMapping = dbContext.ErrorCodeMappings.FirstOrDefault(x => x.Id == errorCodeMapping.Id);
                    if (targetErrorCodeMapping != null)
                    {
                        dbContext.ErrorCodeMappings.Remove(targetErrorCodeMapping);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete error code mapping {targetErrorCodeMapping.ConditionString}: {targetErrorCodeMapping.Description} success");
                    }
                    else
                    {
                        return new(4, $"Error code mapping {errorCodeMapping.ConditionString}: {errorCodeMapping.Description} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }



        #endregion
    }
}
