using Microsoft.EntityFrameworkCore;
using CommonLibrary.API.Message;
using TMWeb.Data;
using TMWeb.EFModels;
using TMWeb.Data.DTO;
using CommonLibrary.MachinePKG;
using CommonLibrary.MachinePKG.EFModel;
using CommonLibrary.MachinePKG.MachineData;
using Serilog.Filters;

namespace TMWeb.Services
{
    public class TMWebShopfloorService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<TMWebShopfloorService> logger;
        public TMWebShopfloorService(ILogger<TMWebShopfloorService> tmWebShopfloorServicelogger, IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            logger = tmWebShopfloorServicelogger;
            //_ = InitAll();
        }

        public async Task InitAll()
        {
            InitAllStations();
            //await InitAllMachinesFromDB();
        }


        #region process
        public Task<List<Process>> GetAllProcess()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.AsNoTracking().ToList());
            }
        }

        public async Task<RequestResult> UpsertProcess(Process process)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    Process? targetProcess = dbContext.Processes.Include(x => x.Stations).FirstOrDefault(x => x.Id == process.Id);
                    if (targetProcess != null)
                    {
                        targetProcess.Name = process.Name;
                    }
                    else
                    {
                        var a = await dbContext.Processes.AddAsync(process);
                    }
                    await dbContext.SaveChangesAsync();
                    return new RequestResult(2, $"Upsert process {process.Name} success");
                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, $"Upsert process {process.Name} fail({ex.Message})");
            }


        }

        public async Task<RequestResult> DeleteProcess(Process process)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    Process? targetProcess = dbContext.Processes.Include(x => x.Stations).FirstOrDefault(x => x.Id == process.Id);
                    if (targetProcess != null)
                    {
                        dbContext.Remove(targetProcess);
                        await dbContext.SaveChangesAsync();
                        return new RequestResult(2, $"Delete process {targetProcess.Name} success");
                    }
                    else
                    {
                        return new RequestResult(4, $"Process {process.Name} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, $"Delete process {process.Name} fail({ex.Message})");
            }
        }

        public Task<List<Process>> GetAllProcessAndStations()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.Include(x => x.Stations.OrderBy(x => x.ProcessIndex).ThenBy(x => x.Name)).AsNoTracking().ToList());
            }
        }
        public Task<Process?> GetProcessByName(string processName)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.FirstOrDefault(x => x.Name == processName));
            }
        }
        public Task<Process?> GetProcessByID(Guid? processID)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.FirstOrDefault(x => x.Id == processID));
            }
        }

        private async Task<bool> CheckStationIsFirstInProcess(Station station)
        {
            var stations = await GetStationsByProcessID(station.ProcessId);
            return stations.Min(x => x.ProcessIndex) == station.ProcessIndex;
        }
        private async Task<bool> CheckStationIsLastInProcess(Station station)
        {
            var stations = await GetStationsByProcessID(station.ProcessId);
            return stations.Where(x => x.Enable == true).Max(x => x.ProcessIndex) == station.ProcessIndex;
        }

        #endregion

        #region station
        private List<Station> stations = new List<Station>();
        public List<Station> Stations => stations;
        public Task<List<Station>> GetAllStationsConfig()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Stations.AsNoTracking().ToList());
            }
        }

        public async Task<RequestResult> UpsertStation(Station station)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    Station? target = dbContext.Stations.FirstOrDefault(x => x.Id == station.Id);
                    if (target != null)
                    {
                        target.ProcessId = station.ProcessId;
                        target.Name = station.Name;
                        target.ProcessIndex = station.ProcessIndex;
                        target.StationType = station.StationType;
                        target.Enable = station.Enable;

                    }
                    else
                    {
                        await dbContext.Stations.AddAsync(station);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert station {station.Name} success");
                }
                catch (Exception ex)
                {
                    return new RequestResult(4, $"Upsert station {station.Name} fail({ex.Message})");
                }

            }
        }

        public async Task<RequestResult> DeleteStation(Station station)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    Station? targetStation = dbContext.Stations.FirstOrDefault(x => x.Id == station.Id);
                    if (targetStation != null)
                    {
                        dbContext.Remove(targetStation);
                        await dbContext.SaveChangesAsync();
                        return new RequestResult(2, $"Delete station {targetStation.Name} success");
                    }
                    else
                    {
                        return new RequestResult(4, $"Process {station.Name} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, $"Delete station {station.Name} fail({ex.Message})");
            }
        }

        public async Task<List<Station>> GetStationsByProcessName(string processName)
        {
            Process? targetProcess = await GetProcessByName(processName);
            if (targetProcess != null)
            {
                return Stations.Where(x => x.ProcessId == targetProcess.Id).OrderBy(x => x.ProcessIndex).ThenBy(x => x.Name).ToList();
            }
            return new();
        }
        public async Task<List<Station>> GetStationsByProcessID(Guid? processID)
        {
            Process? targetProcess = await GetProcessByID(processID);
            if (targetProcess != null)
            {
                return Stations.Where(x => x.ProcessId == targetProcess.Id).ToList();
            }
            return new();
        }
        public Task<Station?> GetStationsByName(string stationName)
        {
            return Task.FromResult(stations.FirstOrDefault(x => x.Name == stationName));
        }
        public Task<Station?> GetStationsById(Guid? id)
        {
            return Task.FromResult(stations.FirstOrDefault(x => x.Id == id));
        }
        public void InitAllStations()
        {
            stations = new();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var stationbases = dbContext.Stations.Include(x => x.Process)
                    .Include(x => x.StationUirecords).ThenInclude(x => x.ItemRecord).AsNoTracking().ToList();
                foreach (var stationbase in stationbases)
                {
                    switch (stationbase.StationType)
                    {
                        case 0:
                            stations.Add(new StationSingleWorkorderSingleSerial(stationbase));
                            break;
                        case 1:
                            stations.Add(new StationSingleWorkorderMutipleSerial(stationbase));
                            break;
                        case 2:
                            stations.Add(new StationSingleWorkorderNoSerial(stationbase));
                            break;
                        default:
                            break;
                    }
                }
                stations.ForEach(x => x.InitStation());
            }
        }
        public async Task<bool> StationSetWorkorder(string stationName, string wo, string lot)//station name, workorder no and lot
        {
            Workorder? targetWorkorder = await GetWorkorderByNoAndLot(wo, lot);
            Station? targetStation = await GetStationsByName(stationName);
            if (targetWorkorder != null && targetStation != null)
            {
                var res = targetStation.SetWorkorder(targetWorkorder);
                StationChanged(targetStation);
                return res;
            }
            return false;
        }
        public async Task RunStationByName(string stationName)//station name, workorder no and lot
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                targetStation.Run();
                StationChanged(targetStation);
            }
        }
        public async Task<RequestResult> StationInByNameAndSerialNo(string stationName, string serialNo)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationStatus == Status.Running)
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                        case 1:
                            try
                            {
                                StationSingleWorkorder? stationSingleWorkorder = targetStation as StationSingleWorkorder;
                                var itemDetail = await GetOrGenerateItemDetailByWorkorderAndSerialNo(stationSingleWorkorder.Workerder, serialNo);
                                stationSingleWorkorder?.AddItemDetail(itemDetail);
                                var taskDetail = await GenerateTaskDetailByItem(targetStation, itemDetail);
                                stationSingleWorkorder?.AddTaskDetail(taskDetail);
                                return new(2, $"{stationName} stationIn with {serialNo} success");
                            }
                            catch (Exception ex)
                            {
                                return new(4, ex.Message);
                            }
                        default:
                            return new(3, $"station {stationName} deosn't support this command");
                    }

                }
                else
                {
                    return new(3, $"station {stationName} isn't running");
                }
            }
            else
            {
                return new(3, $"station {stationName} not found");
            }
        }
        public async Task StationInByNameAndAmount(string stationName, int amount)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationType <= 2)//single workorder
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                        case 1:
                            break;
                        case 2:
                            StationSingleWorkorderNoSerial? stationSingleWorkorderNoSerial = targetStation as StationSingleWorkorderNoSerial;
                            
                            var itemDetail = await GetOrGenerateItemDetailByWorkorderWithoutSerialNo(stationSingleWorkorderNoSerial.Workerder);
                            stationSingleWorkorderNoSerial?.AddItemDetail(itemDetail);
                            ItemDetailUpdate(itemDetail.Id);

                            var taskDetail = await GenerateTaskDetailByItem(targetStation, itemDetail);
                            stationSingleWorkorderNoSerial?.AddTaskDetail(taskDetail);
                            
                            stationSingleWorkorderNoSerial?.StationInWithAmount(amount);
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        public async Task StationOutBySerialNo(string stationName, string serialNo, bool pass)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationType <= 2)//single workorder
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                            StationSingleWorkorder? StationSingleWorkorder = targetStation as StationSingleWorkorder;
                            TaskDetail? taskDetail = StationSingleWorkorder?.RemoveTaskDetail();

                            if (taskDetail != null)
                            {
                                if (pass)
                                {
                                    taskDetail.Okamount = 1;
                                }
                                else
                                {
                                    taskDetail.Ngamount = 1;
                                }
                                taskDetail.FinishedTime = DateTime.Now;
                                await UpdateTaskDetailWhenStationOut(taskDetail);

                                ItemDetail? targetItemDetail = StationSingleWorkorder?.RemoveItemDetail();
                                Workorder? workorder = StationSingleWorkorder?.Workerder;
                                bool isLast = await CheckStationIsLastInProcess(targetStation);
                                if (!pass)
                                {
                                    await SummaryItemFromTaskWithSerialNoWhenNg(targetItemDetail);
                                    await SummaryWorkorderFromUtemWithSerialNoWhenNg(workorder);
                                }
                                else
                                {
                                    if (isLast)
                                    {
                                        await SummaryItemFromTaskWithSerialNoWhenOk(targetItemDetail);
                                        await SummaryWorkorderFromUtemWithSerialNoWhenOk(workorder);
                                    }
                                }


                            }
                            break;
                        case 1:
                            StationSingleWorkorderMutipleSerial? stationSingleWorkorderMutipleSerial = targetStation as StationSingleWorkorderMutipleSerial;
                            TaskDetail? taskDetail2 = stationSingleWorkorderMutipleSerial?.RemoveTaskDetail(serialNo);
                            if (taskDetail2 != null)
                            {
                                if (pass)
                                {
                                    taskDetail2.Okamount = 1;
                                }
                                else
                                {
                                    taskDetail2.Ngamount = 1;
                                }
                                taskDetail2.FinishedTime = DateTime.Now;
                                await UpdateTaskDetailWhenStationOut(taskDetail2);

                                ItemDetail? targetItemDetail = stationSingleWorkorderMutipleSerial?.RemoveItemDetail();
                                Workorder? workorder = stationSingleWorkorderMutipleSerial?.Workerder;

                                bool isLast = await CheckStationIsLastInProcess(targetStation);
                                if (!pass)
                                {
                                    await SummaryItemFromTaskWithSerialNoWhenNg(targetItemDetail);
                                    await SummaryWorkorderFromUtemWithSerialNoWhenNg(workorder);
                                }
                                else
                                {
                                    if (isLast)
                                    {
                                        await SummaryItemFromTaskWithSerialNoWhenOk(targetItemDetail);
                                        await SummaryWorkorderFromUtemWithSerialNoWhenOk(workorder);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        public async Task<RequestResult> StationOutByFIFO(string stationName, bool pass)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationStatus == Status.Running)
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                        case 1:
                            try
                            {
                                StationSingleWorkorder? StationSingleWorkorder = targetStation as StationSingleWorkorder;
                                TaskDetail? taskDetail = StationSingleWorkorder?.RemoveTaskDetail();
                                if (taskDetail != null)
                                {
                                    if (pass)
                                    {
                                        taskDetail.Okamount = 1;
                                    }
                                    else
                                    {
                                        taskDetail.Ngamount = 1;
                                    }
                                    taskDetail.FinishedTime = DateTime.Now;
                                    await UpdateTaskDetailWhenStationOut(taskDetail);

                                    ItemDetail? targetItemDetail = StationSingleWorkorder?.RemoveItemDetail();
                                    Workorder? workorder = StationSingleWorkorder?.Workerder;
                                    bool isLast = await CheckStationIsLastInProcess(targetStation);
                                    if (!pass)
                                    {
                                        await SummaryItemFromTaskWithSerialNoWhenNg(targetItemDetail);
                                        await SummaryWorkorderFromUtemWithSerialNoWhenNg(workorder);
                                    }
                                    else
                                    {
                                        if (isLast)
                                        {
                                            await SummaryItemFromTaskWithSerialNoWhenOk(targetItemDetail);
                                            await SummaryWorkorderFromUtemWithSerialNoWhenOk(workorder);
                                        }
                                    }
                                    return new(2, $"{stationName} stationout with FIFO success");
                                }
                                else
                                {
                                    return new(3, $"station {stationName} stationout by FIFO but taskDetail not found");
                                }
                            }
                            catch (Exception ex)
                            {
                                return new(4, ex.Message);
                            }
                        default:
                            return new(3, $"station {stationName} deosn't support this command");
                    }
                }
                else
                {
                    return new(3, $"station {stationName} isn't running");
                }
            }
            else
            {
                return new(3, $"station {stationName} not found");
            }
        }
        public async Task StationOutByAmount(string stationName, int ok, int ng)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationType <= 2)//single workorder
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                        case 1:
                            break;
                        case 2:
                            StationSingleWorkorderNoSerial? stationSingleWorkorderNoSerial = targetStation as StationSingleWorkorderNoSerial;
                            TaskDetail? taskDetail = stationSingleWorkorderNoSerial?.TaskDetail;
                            if (taskDetail != null)
                            {
                                stationSingleWorkorderNoSerial.StationOutWithAmount(ok, ng);
                                //taskDetail.Okamount += ok;
                                //taskDetail.Ngamount += ng;
                                //taskDetail.FinishedTime = DateTime.Now;
                                await UpdateTaskDetailWhenStationOut(taskDetail);

                                bool isLast = await CheckStationIsLastInProcess(targetStation);
                                if (isLast)
                                {
                                    ItemDetail? targetItemDetail = stationSingleWorkorderNoSerial.ItemDetail;
                                    await SummaryItemFromTaskWithoutSerialNo(targetItemDetail);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        public async Task SummaryItemFromTaskWithSerialNoWhenOk(ItemDetail itemDetail)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                ItemDetail? targetItem = dbContext.ItemDetails.Include(x => x.TaskDetails).FirstOrDefault(x => x.Id == itemDetail.Id);
                if (targetItem != null)
                {
                    targetItem.FinishedTime = DateTime.Now;
                    if (targetItem.TaskDetails.Sum(x => x.Okamount) > 0)
                    {
                        targetItem.Okamount = targetItem.TaskDetails.Max(x => x.Okamount);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
        public async Task SummaryItemFromTaskWithSerialNoWhenNg(ItemDetail itemDetail)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                ItemDetail? targetItem = dbContext.ItemDetails.Include(x => x.TaskDetails).FirstOrDefault(x => x.Id == itemDetail.Id);
                if (targetItem != null)
                {
                    targetItem.FinishedTime = DateTime.Now;
                    if (targetItem.TaskDetails.Sum(x => x.Ngamount) > 0)
                    {
                        targetItem.Ngamount = targetItem.TaskDetails.Max(x => x.Ngamount);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task SummaryWorkorderFromUtemWithSerialNoWhenOk(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                Workorder? wo = dbContext.Workorders.Include(x => x.ItemDetails).FirstOrDefault(x => x.Id == workorder.Id);
                if (wo != null)
                {
                    if (wo.ItemDetails.Sum(x => x.Okamount) > 0)
                    {
                        wo.Okamount = wo.ItemDetails.Sum(x => x.Okamount);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task SummaryWorkorderFromUtemWithSerialNoWhenNg(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                Workorder? wo = dbContext.Workorders.Include(x => x.ItemDetails).FirstOrDefault(x => x.Id == workorder.Id);
                if (wo != null)
                {
                    if (wo.ItemDetails.Sum(x => x.Ngamount) > 0)
                    {
                        wo.Ngamount = wo.ItemDetails.Sum(x => x.Ngamount);
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
        }
        public async Task SummaryItemFromTaskWithoutSerialNo(ItemDetail itemDetail)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                ItemDetail? targetItem = dbContext.ItemDetails.Include(x => x.TaskDetails).FirstOrDefault(x => x.Id == itemDetail.Id);
                if (targetItem != null)
                {
                    targetItem.FinishedTime = DateTime.Now;
                    targetItem.Ngamount = targetItem.TaskDetails.Sum(x => x.Ngamount);
                    targetItem.Okamount = targetItem.TaskDetails.Min(x => x.Okamount);

                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task StopStationByName(string stationName)//station name, workorder no and lot
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                targetStation.Stop();
                StationChanged(targetStation);
            }
        }


        public async Task<bool> StationClearWorkorder(string stationName)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                return targetStation.ClearWorkorder();
            }
            return false;
        }
        public Action<Station>? StationChangedAct;
        private void StationChanged(Station station) => StationChangedAct?.Invoke(station);


        #endregion

        #region machine

        //private List<Machine> machines = new();
        //Task<List<MachineBase>> IMachineService.GetAllMachinesConfig()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.Machines.AsNoTracking().ToList());
        //    }
        //}

        //public Task<List<Machine>> GetAllMachinesConfig()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.Machines.AsNoTracking().ToList());
        //    }
        //}

        //Task<List<MachineBase>> IMachineService.GetAllMachinesConfig()
        //{
        //    throw new NotImplementedException();
        //}
        //public List<Machine> Machines => machines;

        //public async Task InitAllMachinesFromDB()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        var tmp = dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags)
        //            .Include(x=>x.LogicStatusCategory).ThenInclude(x=>x.LogicStatusConditions)
        //            .Include(x=>x.ErrorCodeCategory).ThenInclude(x=>x.ErrorCodeMappings)
        //            .AsSplitQuery()
        //            .AsNoTracking()
        //            .ToList();
        //        machines = tmp.Select(x => InitMachineToDerivesClass(x)).ToList();
        //        List<Task> tasks = new();
        //        foreach (Machine machine in machines)
        //        {
        //            tasks.Add(Task.Run(() =>
        //            {
        //                machine.InitMachine();
        //                if (machine.Enabled)
        //                {
        //                    machine.StartUpdating();
        //                }
        //            }));

        //        }
        //        await Task.WhenAll(tasks);
        //    }
        //}
        //public Machine? InitMachineFromDBById(Guid id)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        var tmp = dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags)
        //            .Include(x=>x.LogicStatusCategory).ThenInclude(x=>x.LogicStatusConditions)
        //            .Include(x => x.ErrorCodeCategory).ThenInclude(x => x.ErrorCodeMappings)
        //            .AsSplitQuery()
        //            .AsNoTracking()
        //            .FirstOrDefault(x => x.Id == id);
        //        tmp = InitMachineToDerivesClass(tmp);
        //        tmp.InitMachine();
        //        if (tmp.Enabled)
        //        {
        //            tmp.StartUpdating();
        //        }
        //        return tmp;
        //    }
        //}
        //public Machine InitMachineToDerivesClass(Machine machine)
        //{
        //    switch (machine.ConnectionType)
        //    {
        //        case 0:
        //            return new ModbusTCPMachine(machine);
        //        case 1:
        //            return new TMRobotModbusTCP(machine);
        //        case 2:
        //        case 10:
        //        default:
        //            return machine;
        //    }
        //}
        //public async Task<RequestResult> UpsertMachineConfig(Machine machine)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        try
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var target = dbContext.Machines.FirstOrDefault(x => x.Id == machine.Id);
        //            bool exist = target is not null;
        //            if (exist)
        //            {
        //                target.Name = machine.Name;
        //                //target.ProcessId = machine.ProcessId;
        //                target.Ip = machine.Ip;
        //                target.Port = machine.Port;
        //                target.ConnectionType = machine.ConnectionType;
        //                target.MaxRetryCount = machine.MaxRetryCount;
        //                target.TagCategoryId = machine.TagCategoryId;
        //                target.LogicStatusCategoryId = machine.LogicStatusCategoryId;
        //                target.ErrorCodeCategoryId = machine.ErrorCodeCategoryId;
        //                target.Enabled = machine.Enabled;
        //                target.UpdateDelay = machine.UpdateDelay;
        //            }
        //            else
        //            {
        //                await dbContext.Machines.AddAsync(machine);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            DataEditMode dataEditMode = exist ? DataEditMode.Update : DataEditMode.Insert;
        //            await RefreshMachine(machine, dataEditMode);
        //            return new(2, $"upsert machine {machine.Name} success");
        //        }
        //        catch (Exception e)
        //        {
        //            return new(4, $"upsert machine {machine.Name} fail({e.Message})");
        //        }

        //    }
        //}
        //public async Task<RequestResult> DeleteMachine(Machine machine)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        try
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var target = dbContext.Machines.FirstOrDefault(x => x.Id == machine.Id);
        //            if (target != null)
        //            {
        //                dbContext.Remove(target);
        //                await dbContext.SaveChangesAsync();
        //                await RefreshMachine(target, DataEditMode.Delete);
        //                return new(2, $"Delete machine {machine.Name} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Machine {machine.Name} not found");
        //            }

        //        }
        //        catch (Exception e)
        //        {
        //            return new(4, $"Delete machine {machine.Name} fail({e.Message})");
        //        }

        //    }
        //}
        //public async Task RefreshMachine(Machine machine, DataEditMode dataEditMode)
        //{
        //    var target = await GetMachineByID(machine.Id);
        //    if (target != null)
        //    {
        //        //update or delete
        //        machines.Remove(target);
        //        target.Dispose();
        //        if (dataEditMode != DataEditMode.Delete)
        //        {
        //            machines.Add(InitMachineFromDBById(machine.Id));
        //        }
        //        else
        //        {
        //        }
        //    }
        //    else
        //    {
        //        machines.Add(InitMachineFromDBById(machine.Id));
        //    }
        //    MachineConfigChanged(machine.Id, dataEditMode);
        //}

        //public Action<Guid, DataEditMode>? MachineConfigChangedAct;
        //public void MachineConfigChanged(Guid id, DataEditMode mode)
        //{
        //    MachineConfigChangedAct?.Invoke(id, mode);
        //}
        //public async Task<List<Machine>> GetMachineByProcessName(string processName)
        //{
        //    Process? target = await GetProcessByName(processName);
        //    return machines.Where(x => x.ProcessId == target.Id).ToList();
        //}

        public Task<List<Machine>> GetMachinesWithoutRelationAndCerrent(Guid? currentId)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var existRelationMachinesId = dbContext.ProcessMachineRelations.Select(x=>x.MachineId).ToList();
                var machineService = scope.ServiceProvider.GetRequiredService<IMachineService>();
                return Task.FromResult(machineService.Machines.Where(x => !existRelationMachinesId.Contains(x.Id) || x.Id == currentId).ToList());
            }
        }

        public async Task<List<Machine>> GetMachineByProcessID(Guid? id)
        {
            var targetMachineIDs = await GetProcessMachineRelationByID(id);
            using (var scope = scopeFactory.CreateScope())
            {
                var machineService = scope.ServiceProvider.GetRequiredService<IMachineService>();
                return machineService.Machines.Where(x => targetMachineIDs.Select(x => x.MachineId).Contains(x.Id)).ToList();
            }   
        }
        public Task<List<ProcessMachineRelation>> GetProcessMachineRelationByID(Guid? id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var targetMachinesId = dbContext.ProcessMachineRelations.Where(x => x.ProcessId == id);
                return Task.FromResult(targetMachinesId.ToList());
            }
        }
        public async Task<RequestResult> UpsertProcessMachineRelation(ProcessMachineRelation processMachineRelation)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var targetRelation = dbContext.ProcessMachineRelations.FirstOrDefault(x => x.Id == processMachineRelation.Id);
                    if (targetRelation != null)
                    {
                        targetRelation.MachineId = processMachineRelation.MachineId;
                    }
                    else
                    {
                        await dbContext.ProcessMachineRelations.AddAsync(processMachineRelation);
                    }
                    await dbContext.SaveChangesAsync();
                    return new RequestResult(2, $"Upsert process station relation success");
                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, $"Upsert process station relation fail({ex.Message})");
            }
        }

        public async Task<RequestResult> DeleteProcessMachineRelation(ProcessMachineRelation processMachineRelation)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var targetRelation = dbContext.ProcessMachineRelations.FirstOrDefault(x => x.Id == processMachineRelation.Id);
                    if (targetRelation != null)
                    {
                        dbContext.Remove(targetRelation);
                        await dbContext.SaveChangesAsync();
                        return new RequestResult(2, $"Delete process staion relation success");
                    }
                    else
                    {
                        return new RequestResult(4, $"Process staion relation not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, $"Delete process staion relation fail({ex.Message})");
            }
        }
        //public Task<Machine?> GetMachineByID(Guid? id)
        //{
        //    return Task.FromResult(machines.FirstOrDefault(x => x.Id == id));
        //}
        //public Task<Machine?> GetMachineByName(string name)
        //{
        //    return Task.FromResult(machines.FirstOrDefault(x => x.Name == name));
        //}
        #endregion

        #region tag

        //public Task<List<TagCategory>> GetAllTagCategories()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.TagCategories.AsNoTracking().ToList());
        //    }
        //}

        //public Task<List<TagCategory>> GetAllTagCategoriesWithTags()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.TagCategories.Include(x => x.Tags).AsNoTracking().ToList());
        //    }
        //}

        //public List<Tag> GetTagsByCatId(Guid? catID)
        //{
        //    if (catID is null)
        //    {
        //        return new List<Tag>();
        //    }
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        var targetCat = dbContext.TagCategories.Include(x => x.Tags).AsNoTracking().FirstOrDefault(x=>x.Id == catID);
        //        if (targetCat is not null)
        //        {
        //            return targetCat.Tags.ToList();
        //        }
        //        else
        //        {
        //            return new List<Tag>();
        //        }
        //    }
        //}

        //public int GetTagTypeCodeByIds(Guid? catID, Guid? tagID)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        var targetTag = dbContext.Tags.FirstOrDefault(x => x.CategoryId == catID && x.Id == tagID);
        //        return targetTag is null ? 0 : targetTag.DataType;
        //    }
        //}

        //public Task<List<TagCategory>> GetCategoryByConnectionType(int connectionType)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.TagCategories.Where(x => x.ConnectionType == connectionType).ToList());
        //    }
        //}

        //public async Task<RequestResult> UpsertTagCategory(TagCategory tagCategory)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetTagCat = dbContext.TagCategories.FirstOrDefault(x => x.Id == tagCategory.Id);
        //            if (targetTagCat != null)
        //            {
        //                targetTagCat.Name = tagCategory.Name;
        //                targetTagCat.ConnectionType = tagCategory.ConnectionType;
        //            }
        //            else
        //            {
        //                await dbContext.TagCategories.AddAsync(tagCategory);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            return new(2, $"Upsert tag category {tagCategory.Name} success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> DeleteTagCategory(TagCategory tagCategory)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetTagCat = dbContext.TagCategories.Include(x => x.Tags).FirstOrDefault(x => x.Id == tagCategory.Id);
        //            if (targetTagCat != null)
        //            {
        //                dbContext.TagCategories.Remove(targetTagCat);
        //                await dbContext.SaveChangesAsync();
        //                return new(2, $"Delete tag category {targetTagCat.Name} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Tag category {targetTagCat.Name} not found");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> UpsertTag(Tag tag)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetTag = dbContext.Tags.FirstOrDefault(x => x.Id == tag.Id);
        //            if (targetTag != null)
        //            {
        //                targetTag.Name = tag.Name;
        //                targetTag.DataType = tag.DataType;
        //                targetTag.UpdateByTime = tag.UpdateByTime;
        //                targetTag.SpecialType = tag.SpecialType;

        //                targetTag.Bool1 = tag.Bool1;
        //                targetTag.Bool2 = tag.Bool2;
        //                targetTag.Bool3 = tag.Bool3;
        //                targetTag.Bool4 = tag.Bool4;
        //                targetTag.Bool5 = tag.Bool5;

        //                targetTag.Int1 = tag.Int1;
        //                targetTag.Int2 = tag.Int2;
        //                targetTag.Int3 = tag.Int3;
        //                targetTag.Int4 = tag.Int4;
        //                targetTag.Int5 = tag.Int5;

        //                targetTag.String1 = tag.String1;
        //                targetTag.String2 = tag.String2;
        //                targetTag.String3 = tag.String3;
        //                targetTag.String4 = tag.String4;
        //                targetTag.String5 = tag.String5;
        //            }
        //            else
        //            {
        //                await dbContext.Tags.AddAsync(tag);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            return new(2, $"Upsert tag {tag.Name} success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }

        //}

        //public async Task<RequestResult> DeleteTag(Tag tag)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetTag = dbContext.Tags.FirstOrDefault(x => x.Id == tag.Id);
        //            if (targetTag != null)
        //            {
        //                dbContext.Tags.Remove(targetTag);
        //                await dbContext.SaveChangesAsync();
        //                return new(2, $"Delete tag {tag.Name} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Tag {tag.Name} not found");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }

        //}

        //public async Task<Tag?> GetMachineTag(string machineName, string tagName)
        //{
        //    Machine? targetMachine = await GetMachineByName(machineName);
        //    if (targetMachine != null)
        //    {
        //        if (targetMachine.hasCategory)
        //        {
        //            Tag? targetTag = targetMachine.TagCategory.Tags.FirstOrDefault(x => x.Name == tagName);
        //            if (targetTag != null)
        //            {
        //                if (!targetTag.UpdateByTime)
        //                {
        //                    await targetMachine.UpdateTag(targetTag);
        //                }
        //                return targetTag;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //public async Task<RequestResult> SetMachineTag(string machineName, string tagName, Object val)
        //{
        //    Machine? targetMachine = await GetMachineByName(machineName);
        //    if (targetMachine != null)
        //    {
        //        if (targetMachine.hasCategory)
        //        {
        //            Tag? targetTag = targetMachine.TagCategory.Tags.FirstOrDefault(x => x.Name == tagName);
        //            if (targetTag != null)
        //            {
        //                return await targetMachine.SetTag(targetTag.Name, val);
        //            }
        //            else
        //            {
        //                return new(4, $"Tag {tagName} not found in machine {machineName}");
        //            }
        //        }
        //        else
        //        {
        //            return new(4, $"Machine tag category not set");
        //        }
        //    }
        //    else
        //    {
        //        return new(4, $"Machine {machineName} not found");
        //    }
        //}

        #endregion

        #region StatusCondition

        //public Task<List<LogicStatusCategory>> GetCustomStatusAndConditions()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.LogicStatusCategories.Include(x => x.LogicStatusConditions).AsNoTracking().ToList());
        //    }
        //}

        //public async Task<RequestResult> UpsertCustomStatusCategory(LogicStatusCategory tagCategory)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetTagCat = dbContext.LogicStatusCategories.FirstOrDefault(x => x.Id == tagCategory.Id);
        //            if (targetTagCat != null)
        //            {
        //                targetTagCat.Name = tagCategory.Name;
        //                targetTagCat.DataType = tagCategory.DataType;
        //            }
        //            else
        //            {
        //                await dbContext.LogicStatusCategories.AddAsync(tagCategory);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            return new(2, $"Upsert custom status category {tagCategory.Name} success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> DeleteCustomStatusCategory(LogicStatusCategory tagCategory)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targeLogicStatusCat = dbContext.LogicStatusCategories.FirstOrDefault(x => x.Id == tagCategory.Id);
        //            if (targeLogicStatusCat != null)
        //            {
        //                dbContext.LogicStatusCategories.Remove(targeLogicStatusCat);
        //                await dbContext.SaveChangesAsync();
        //                return new(2, $"Delete tag category {targeLogicStatusCat.Name} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Tag category {tagCategory.Name} not found");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> UpsertCustomStatusCondition(LogicStatusCondition condition)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetCondition = dbContext.LogicStatusCondictions.FirstOrDefault(x => x.Id == condition.Id);
        //            if (targetCondition != null)
        //            {
        //                targetCondition.ConditionString = condition.ConditionString;
        //                targetCondition.Status = condition.Status;
        //            }
        //            else
        //            {
        //                await dbContext.LogicStatusCondictions.AddAsync(condition);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            return new(2, $"Upsert custom status condition {condition.ConditionString} as {(Status)condition.Status} success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> DeleteCustomStatusCondition(LogicStatusCondition condition)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetCondition = dbContext.LogicStatusCondictions.FirstOrDefault(x => x.Id == condition.Id);
        //            if (targetCondition != null)
        //            {
        //                dbContext.LogicStatusCondictions.Remove(targetCondition);
        //                await dbContext.SaveChangesAsync();
        //                return new(2, $"Delete tag category {targetCondition.ConditionString} as {(Status)targetCondition.Status} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Custom condition {condition.ConditionString} as {(Status)condition.Status} not found");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        #endregion

        #region Error code

        //public Task<List<ErrorCodeCategory>> GetErrorCodeTables()
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        return Task.FromResult(dbContext.ErrorCodeCategories.Include(x => x.ErrorCodeMappings).AsNoTracking().ToList());
        //    }
        //}

        //public async Task<RequestResult> UpsertErrorCodeCategory(ErrorCodeCategory errorCodeCategory)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetErrorCodeCat = dbContext.ErrorCodeCategories.FirstOrDefault(x => x.Id == errorCodeCategory.Id);
        //            if (targetErrorCodeCat != null)
        //            {
        //                targetErrorCodeCat.Name = errorCodeCategory.Name;
        //                targetErrorCodeCat.DataType = errorCodeCategory.DataType;
        //            }
        //            else
        //            {
        //                await dbContext.ErrorCodeCategories.AddAsync(errorCodeCategory);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            return new(2, $"Upsert Error Code category {errorCodeCategory.Name} success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> DeleteErrorCodeCategory(ErrorCodeCategory errorCodeCategory)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targeErrorCodeCat = dbContext.ErrorCodeCategories.FirstOrDefault(x => x.Id == errorCodeCategory.Id);
        //            if (targeErrorCodeCat != null)
        //            {
        //                dbContext.ErrorCodeCategories.Remove(targeErrorCodeCat);
        //                await dbContext.SaveChangesAsync();
        //                return new(2, $"Delete error code category {targeErrorCodeCat.Name} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Error code category {errorCodeCategory.Name} not found");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> UpsertErrorCodeMapping(ErrorCodeMapping errorCodeMapping)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetCodeMapping = dbContext.ErrorCodeMappings.FirstOrDefault(x => x.Id == errorCodeMapping.Id);
        //            if (targetCodeMapping != null)
        //            {
        //                targetCodeMapping.ConditionString = errorCodeMapping.ConditionString;
        //                targetCodeMapping.Description = errorCodeMapping.Description;
        //            }
        //            else
        //            {
        //                await dbContext.ErrorCodeMappings.AddAsync(errorCodeMapping);
        //            }
        //            await dbContext.SaveChangesAsync();
        //            return new(2, $"Upsert error code {errorCodeMapping.ConditionString} : {errorCodeMapping.Description} success");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        //public async Task<RequestResult> DeleteErrorCodeMapping(ErrorCodeMapping errorCodeMapping)
        //{
        //    try
        //    {
        //        using (var scope = scopeFactory.CreateScope())
        //        {
        //            var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //            var targetErrorCodeMapping = dbContext.ErrorCodeMappings.FirstOrDefault(x => x.Id == errorCodeMapping.Id);
        //            if (targetErrorCodeMapping != null)
        //            {
        //                dbContext.ErrorCodeMappings.Remove(targetErrorCodeMapping);
        //                await dbContext.SaveChangesAsync();
        //                return new(2, $"Delete error code mapping {targetErrorCodeMapping.ConditionString}: {targetErrorCodeMapping.Description} success");
        //            }
        //            else
        //            {
        //                return new(4, $"Error code mapping {errorCodeMapping.ConditionString}: {errorCodeMapping.Description} not found");
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new(4, ex.Message);
        //    }
        //}

        #endregion

        #region workorder
        public Task<List<Workorder>> GetAllWorkorderAndRecipe()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.Include(x => x.WorkorderRecordCategoryId).ToList());
            }
        }
        public Task<List<Workorder>> GetAllWorkorders()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.Include(x => x.Process).ToList());
            }
        }
        public Task<List<Workorder>> GetWorkordersByStatus(List<int> targetStatus)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.Where(x => targetStatus.Contains(x.Status)).ToList());
            }
        }
        public Task<List<Workorder>> GetWorkordersByProcessAndStatus(Guid processID, List<int> targetStatus)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders
                    .Include(x => x.WorkorderRecordCategory).ThenInclude(x => x.WorkorderRecordContents)
                    .Include(x => x.ItemRecordsCategory).ThenInclude(x => x.ItemRecordContents)
                    .Include(x => x.TaskRecordCategory).ThenInclude(x => x.TaskRecordContents)
                    .Where(x => x.ProcessId == processID && targetStatus.Contains(x.Status)).ToList()
                    );
            }
        }
        public Task<List<ItemDetailDTO>> GetItemDetailDTOInInterval(DateTime startTime, DateTime endTime)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var woWithItem = dbContext.Workorders.Include(x => x.Process)
                    //.Include(x => x.RecipeCategory).ThenInclude(x => x.Recipes)
                    //.Include(x => x.WorkorderRecordCategory)//.ThenInclude(x => x.WorkorderRecordContents).ThenInclude(x => x.WorkorderRecordDetails.Where(x=>x.WorkerderId == id))
                    //.Include(x => x.WorkorderRecordDetails).ThenInclude(x => x.RecordContent)
                    //.Include(x => x.ItemRecordsCategory)//.ThenInclude(x => x.ItemRecordContents).ThenInclude(x => x.ItemRecordDetails)
                    //.Include(x => x.TaskRecordCategory)//.ThenInclude(x => x.TaskRecordContents).ThenInclude(x => x.TaskRecordDetails)
                    .Include(x => x.ItemDetails)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .Where(x => x.CreateTime >= startTime && x.CreateTime <= endTime).ToList();
                var res = new List<ItemDetailDTO>();
                foreach (var wo in woWithItem)
                {
                    foreach (var item in wo.ItemDetails)
                    {
                        res.Add(new ItemDetailDTO
                        {
                            Process = wo.Process.Name,
                            WorkorderNo = wo.WorkorderNo,
                            Lot = wo.Lot,
                            PartNo = wo.PartNo,
                            SerialNo = item.SerialNo,
                            TargetAmount = item.TargetAmount,
                            Okamount = item.Okamount,
                            Ngamount = item.Ngamount,
                            StartTime = item.StartTime,
                            FinishedTime = item.FinishedTime,
                        });
                    }
                }
                return Task.FromResult(res);
            }
        }
        public Task<Workorder?> GetWorkordersDetailsForConfig(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.Include(x => x.Process)
                    .Include(x => x.RecipeCategory).ThenInclude(x => x.Recipes)
                    .Include(x => x.WorkorderRecordCategory)//.ThenInclude(x => x.WorkorderRecordContents).ThenInclude(x => x.WorkorderRecordDetails.Where(x=>x.WorkerderId == id))
                    .Include(x => x.WorkorderRecordDetails).ThenInclude(x=>x.RecordContent)
                    .Include(x => x.ItemRecordsCategory).ThenInclude(x => x.ItemRecordContents)//.ThenInclude(x => x.ItemRecordDetails)
                    .Include(x => x.TaskRecordCategory)//.ThenInclude(x => x.TaskRecordContents).ThenInclude(x => x.TaskRecordDetails)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.Id == id));
            }
        }
        public Task<Workorder?> GetWorkorderByNoAndLot(string wo, string lot)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.FirstOrDefault(x => x.WorkorderNo == wo && x.Lot == lot));
            }
        }
        public async Task<RequestResult> UpsertWorkorderConfig(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.Workorders.FirstOrDefault(x => x.Id == workorder.Id);
                    if (target != null)
                    {
                        target.ProcessId = workorder.ProcessId;
                        target.WorkorderNo = workorder.WorkorderNo;
                        target.Lot = workorder.Lot;
                        target.PartNo = workorder.PartNo;
                        target.RecipeCategoryId = workorder.RecipeCategoryId;
                        target.WorkorderRecordCategoryId = workorder.WorkorderRecordCategoryId;
                        target.ItemRecordsCategoryId = workorder.ItemRecordsCategoryId;
                        target.TaskRecordCategoryId = workorder.TaskRecordCategoryId;
                        target.TargetAmount = workorder.TargetAmount;
                    }
                    else
                    {
                        await dbContext.AddAsync(workorder);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert workorder {workorder.WorkorderNo}/{workorder.Lot} success");
                }
                catch (Exception e)
                {
                    return new(4, $"Upsert workorder {workorder.WorkorderNo}/{workorder.Lot} fail({e.Message})");
                }

            }
        }
        public async Task<RequestResult> DeleteWorkorder(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.Workorders.Include(x => x.WorkorderRecordDetails)
                        .Include(x => x.ItemDetails).ThenInclude(x => x.TaskDetails)
                        .FirstOrDefault(x => x.Id == workorder.Id);
                    if (target != null)
                    {
                        dbContext.Workorders.Remove(target);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete workorder {workorder.WorkorderNo}/{workorder.Lot} success");
                    }
                    else
                    {
                        return new(4, $"Workorder {workorder.WorkorderNo}/{workorder.Lot} not found");
                    }
                }
                catch (Exception e)
                {
                    return new(4, $"Delete workorder {workorder.WorkorderNo}/{workorder.Lot} fail({e.Message})");
                }


            }
        }
        private async Task<RequestResult> SetWorkorderStatusStartByID(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var targetWo = dbContext.Workorders.FirstOrDefault(x => x.Id == id);
                if (targetWo is not null)
                {
                    if (targetWo.Status == (int)WorkorderStatus.Init)
                    {
                        targetWo.Status = (int)WorkorderStatus.Running;
                        targetWo.StartTime = DateTime.Now;
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Start workorder success");
                    }
                    else
                    {
                        return new(4, $"Workorder is not at Init status");
                    }
                }
                else
                {
                    return new(4, $"Workorder not found");
                }
            }
        }
        public async Task<RequestResult> StartWorkorderByID(Guid id)
        {
            var wo = await GetWorkordersDetailsForConfig(id);
            if (wo is not null)
            {
                var recipeRes = await DeployWorkorderRecipeInProcess(wo);
                if (!recipeRes.IsSuccess)
                {
                    return recipeRes;
                }
                else
                {
                    var stations = await GetStationsByProcessID(wo.ProcessId);
                    bool stationsNotReady = stations.Any(x => x.StationStatus != (int)WorkorderStatus.Init);
                    if (stationsNotReady)
                    {
                        return new(4, $"Not all stations ready");
                    }
                    else
                    {
                        foreach (var station in stations)
                        {
                            var deployRes = station.SetWorkorder(wo);
                            if (!deployRes)
                            {
                                return new(4, $"Deploy workorder to station {station.Name} fail");
                            }
                            station.Run();
                        }
                        return await SetWorkorderStatusStartByID(id);
                    }
                }
            }
            else
            {
                return new(4, "Workorder not found");
            }
        }
        #endregion

        #region recipe

        public Task<List<WorkorderRecipeConfig>> GetAllRecipes()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x => x.Recipes).ToList());
            }
        }
        public async Task<RequestResult> UpsertRecipeConfig(WorkorderRecipeConfig recipeConfig)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var targetRecipeConfig = dbContext.WorkorderRecipeConfigs.FirstOrDefault(x => x.Id == recipeConfig.Id);
                    if (targetRecipeConfig != null)
                    {
                        targetRecipeConfig.RecipeCategory = recipeConfig.RecipeCategory;
                    }
                    else
                    {
                        await dbContext.WorkorderRecipeConfigs.AddAsync(recipeConfig);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert tag category {recipeConfig.RecipeCategory} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }

        public async Task<RequestResult> DeleteRecipeConfig(WorkorderRecipeConfig recipeConfig)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var targetTagCat = dbContext.WorkorderRecipeConfigs.Include(x => x.Recipes).FirstOrDefault(x => x.Id == recipeConfig.Id);
                    if (targetTagCat != null)
                    {
                        dbContext.WorkorderRecipeConfigs.Remove(targetTagCat);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete recipe config {targetTagCat.RecipeCategory} success");
                    }
                    else
                    {
                        return new(4, $"Recipe config {targetTagCat.RecipeCategory} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }
        }
        public async Task<RequestResult> UpsertRecipeItem(RecipeItemBase recipeItemBase)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var targetRecipeItem = dbContext.RecipeBases.FirstOrDefault(x => x.Id == recipeItemBase.Id);
                    if (targetRecipeItem != null)
                    {
                        targetRecipeItem.RecipeItemName = recipeItemBase.RecipeItemName;
                        targetRecipeItem.DataType = recipeItemBase.DataType;
                        targetRecipeItem.TriggerTiming = recipeItemBase.TriggerTiming;
                        targetRecipeItem.TargetTagCatId = recipeItemBase.TargetTagCatId;
                        targetRecipeItem.TargetTagId = recipeItemBase.TargetTagId;
                        if (recipeItemBase.GetType() == typeof(StaticRecipe))
                        {
                            StaticRecipe target = targetRecipeItem as StaticRecipe;
                            StaticRecipe newVal = recipeItemBase as StaticRecipe;

                            if (target != null && newVal != null)
                            {
                                //target.RecipeItemName = newVal.RecipeItemName;
                                //target.DataType = newVal.DataType;
                                target.ValueString = newVal.ValueString;
                                //target.TriggerTiming = newVal.TriggerTiming;
                                //target.TargetTagCatId = newVal.TargetTagCatId;
                                //target.TargetTagId = newVal.TargetTagId;
                            }
                            else
                            {
                                return new(4, $"type casting error");
                            }
                        }
                        else if (recipeItemBase.GetType() == typeof(BuildInRecipe))
                        {
                            BuildInRecipe target = targetRecipeItem as BuildInRecipe;
                            BuildInRecipe newVal = recipeItemBase as BuildInRecipe;

                            if (target != null && newVal != null)
                            {
                                //target.RecipeItemName = newVal.RecipeItemName;
                                //target.DataType = newVal.DataType;
                                //target.TriggerTiming = newVal.TriggerTiming;
                                //target.TargetTagCatId = newVal.TargetTagCatId;
                                //target.TargetTagId = newVal.TargetTagId;
                                target.TargetProp = newVal.TargetProp;
                            }
                            else
                            {
                                return new(4, $"type casting error");
                            }
                        }
                        else if (recipeItemBase.GetType() == typeof(CustomRecipe))
                        {
                            CustomRecipe target = targetRecipeItem as CustomRecipe;
                            CustomRecipe newVal = recipeItemBase as CustomRecipe;

                            if (target != null && newVal != null)
                            {
                                target.TargetRecordCatID = newVal.TargetRecordCatID;
                                target.TargetRecordID = newVal.TargetRecordID;
                            }
                            else
                            {
                                return new(4, $"type casting error");
                            }
                        }
                        else
                        {
                            return new(4, $"type error {recipeItemBase.GetType().Name}");
                        }
                    }
                    else
                    {
                        await dbContext.RecipeBases.AddAsync(recipeItemBase);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert recipe item {recipeItemBase.RecipeItemName} success");
                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }

        }
        public async Task<RequestResult> DeleteRecipeItem(RecipeItemBase recipeItemBase)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var targetRecipeItem = dbContext.RecipeBases.FirstOrDefault(x => x.Id == recipeItemBase.Id);
                    if (targetRecipeItem != null)
                    {
                        dbContext.RecipeBases.Remove(targetRecipeItem);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete tag {recipeItemBase.RecipeItemName} success");
                    }
                    else
                    {
                        return new(4, $"Tag {recipeItemBase.RecipeItemName} not found");
                    }

                }
            }
            catch (Exception ex)
            {
                return new(4, ex.Message);
            }

        }
        public Task<List<WorkorderRecipeConfig>> GetWorkorderRecipeConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }
        public Task<List<WorkorderRecipeConfig>> GetWorkorderRecipeConfigsAndContent()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var a = dbContext.WorkorderRecipeConfigs.Include(x => x.Recipes).ToList();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x => x.Workorders).ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public async Task<RequestResult> DeployWorkorderRecipeInProcess(Workorder wo)
        {
            var targetProcess = await GetProcessByID(wo.ProcessId);
            if (targetProcess is not null)
            {
                var machines = await GetMachineByProcessID(targetProcess.Id);
                if (!machines.Exists(x => x.MachineStatus != Status.Idle && x.MachineStatus != Status.Running))
                {
                    if (wo.HasRecipe)
                    {
                        foreach (var recipe in wo.RecipeCategory?.Recipes)
                        {
                            var recipeRes = recipe.GetValue(wo);
                            if (recipeRes.Item1 && recipeRes.Item2 is not null)
                            {
                                var machineHasRecipe = machines.Where(x => x.TagCategoryId == recipe.TargetTagCatId);
                                foreach (var machine in machineHasRecipe)
                                {
                                    var targetTag = machine.TagCategory.Tags.FirstOrDefault(x => x.Id == recipe.TargetTagId);
                                    if (targetTag is not null)
                                    {
                                        using (var scope = scopeFactory.CreateScope())
                                        {
                                            var machineService = scope.ServiceProvider.GetRequiredService<IMachineService>();
                                            var res = await machineService.SetMachineTag(machine.Name, targetTag.Name, recipeRes.Item2);
                                            if (!res.IsSuccess)
                                            {
                                                return res;
                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                        return new(4, $"Machine target tag not found in tag category");
                                    }
                                }
                            }
                            else
                            {
                                return new(4, $"Get recipe {recipe.RecipeItemName} value fail");
                            }
                        }
                        return new(1, "Deploy recipes success");
                    }
                    else
                    {
                        return new(1, "No recipes");
                    }
                }
                else
                {
                    return new(4, $"Machines in porcess {targetProcess.Name} not all available");
                }
            }
            else
            {
                return new(4, $"Process of workorder not found");
            }
        }

        #endregion

        #region workorder record
        public Task<List<WorkorderRecordConfig>> GetWorkorderRecordConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecordConfigs.ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public List<WorkorderRecordContent> GetWorkorderRecordContentsByConfigID(Guid? id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return dbContext.WorkorderRecordContents.Where(x=>x.ConfigId == id).ToList();//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public Task<List<WorkorderRecordConfig>> GetWorkorderRecordAndDetails()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecordConfigs.Include(x => x.WorkorderRecordContents).AsNoTracking().ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public Task<WorkorderRecordConfig>? GetWorkorderRecordDetailsByConfigID(Guid? id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecordConfigs.Include(x => x.WorkorderRecordContents).FirstOrDefault(x => x.Id == id));//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public Task<List<WorkorderRecordDetail>> GetWorkorderRecordDetailByWoAndConfig(Guid wo)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecordDetails.Include(x=>x.RecordContent).Where(x => x.WorkerderId == wo).ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public async Task<RequestResult> UpsertWorkorderRecordConfig(WorkorderRecordConfig workorderRecordConfig)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.WorkorderRecordConfigs.FirstOrDefault(x => x.Id == workorderRecordConfig.Id);
                    if (target != null)
                    {
                        target.WorkorderRecordCategory = workorderRecordConfig.WorkorderRecordCategory;
                    }
                    else
                    {
                        await dbContext.WorkorderRecordConfigs.AddAsync(workorderRecordConfig);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert workorder record config {workorderRecordConfig.WorkorderRecordCategory} success");
                }
                catch (Exception ex)
                {
                    return new(4, $"Upsert workorder record config {workorderRecordConfig.WorkorderRecordCategory} fail({ex.Message})");
                }

            }
        }

        public async Task<RequestResult> DeletetWorkorderRecordConfig(WorkorderRecordConfig workorderRecordConfig)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.WorkorderRecordConfigs.FirstOrDefault(x => x.Id == workorderRecordConfig.Id);
                    if (target != null)
                    {
                        dbContext.WorkorderRecordConfigs.Remove(target);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete workorder record config {target.WorkorderRecordCategory} success");
                    }
                    else
                    {
                        return new(4, $"Workorder record config {workorderRecordConfig.WorkorderRecordCategory} not exist");
                    }
                }
                catch (Exception ex)
                {
                    return new(4, $"Delete workorder record config {workorderRecordConfig.WorkorderRecordCategory} fail({ex.Message})");
                }

            }
        }

        public Task<WorkorderRecordContent>? GetWorkorderRecordContentByID(Guid? configId, Guid? id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecordContents.FirstOrDefault(x =>x.ConfigId == configId && x.Id == id));//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
            }
        }

        public async Task<RequestResult> UpsertWorkorderRecordContent(WorkorderRecordContent workorderRecordContent)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.WorkorderRecordContents.FirstOrDefault(x => x.Id == workorderRecordContent.Id);
                    if (target != null)
                    {
                        target.RecordName = workorderRecordContent.RecordName;
                        target.DataType = workorderRecordContent.DataType;
                    }
                    else
                    {
                        await dbContext.WorkorderRecordContents.AddAsync(workorderRecordContent);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert workorder record content {workorderRecordContent.RecordName} success");
                }
                catch (Exception ex)
                {
                    return new(4, $"Upsert workorder record config {workorderRecordContent.RecordName} fail({ex.Message})");
                }

            }
        }

        public async Task<RequestResult> DeletetWorkorderRecordContent(WorkorderRecordContent workorderRecordContent)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.WorkorderRecordContents.FirstOrDefault(x => x.Id == workorderRecordContent.Id);
                    if (target != null)
                    {
                        dbContext.WorkorderRecordContents.Remove(target);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete workorder record config {target.RecordName} success");
                    }
                    else
                    {
                        return new(4, $"Workorder record config {workorderRecordContent.RecordName} not exist");
                    }
                }
                catch (Exception ex)
                {
                    return new(4, $"Delete workorder record config {workorderRecordContent.RecordName} fail({ex.Message})");
                }

            }
        }

        public async Task<List<WorkorderRecordDetail>> RetriveOrGenerateWorkorderRecordContent(Workorder wo, Guid? itemRecordConfigId)
        {
            List<WorkorderRecordDetail> res = new();
            var content = await GetWorkorderRecordDetailsByConfigID(itemRecordConfigId);
            if (content != null)
            {
                //total content
                var woRecordConfig = await GetWorkorderRecordDetailsByConfigID(wo.WorkorderRecordCategoryId);
                var totalWorkorderRecordContent = woRecordConfig.WorkorderRecordContents;
                //current record
                List<WorkorderRecordDetail> currentWorkorderRecordDetail = await GetWorkorderRecordDetailByWoAndConfig(wo.Id);
                foreach (var recordContent in totalWorkorderRecordContent)
                {
                    var existDetails = currentWorkorderRecordDetail.FirstOrDefault(x => x.RecordContentId == recordContent.Id);
                    if (existDetails != null)
                    {
                        //existDetails.RecordContent = recordContent;
                        res.Add(existDetails);
                    }
                    else
                    {
                        res.Add(new WorkorderRecordDetail(wo.Id, recordContent));
                    }
                }
            }
            return res;
        }

        public async Task<RequestResult> UpdateWorkorderRecordDetails(Guid woId, IEnumerable<WorkorderRecordDetail> workorderRecordDetails)
        {
            var workorderRecordDetailsWithFilter = workorderRecordDetails.Where(x => !string.IsNullOrEmpty(x.Value));
            var woOnlyone = workorderRecordDetailsWithFilter.DistinctBy(x => x.WorkerderId);
            if (woOnlyone.Count() > 1)
            {
                return new RequestResult(4, $"Update multiple workorder record items at once is invalid");
            }
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var currentInDB = dbContext.WorkorderRecordDetails.Where(x => x.WorkerderId == woId);

                    dbContext.WorkorderRecordDetails.RemoveRange(currentInDB);


                    var detailWithoutNav = workorderRecordDetailsWithFilter.Select(
                        x => new WorkorderRecordDetail()
                        {
                            WorkerderId = x.WorkerderId,
                            RecordContentId = x.RecordContentId,
                            Value = x.Value,
                        });
                    await dbContext.WorkorderRecordDetails.AddRangeAsync(detailWithoutNav);

                    await dbContext.SaveChangesAsync();
                    return new(2, $"Update workorder record items success");
                }
                catch (Exception ex)
                {
                    return new(4, $"Update workorder record items fail({ex.Message})");
                }

            }
        }


        #endregion

        #region item

        public Action<Guid>? ItemDetailUpdateAct;

        private void ItemDetailUpdate(Guid id) => ItemDetailUpdateAct?.Invoke(id);

        public ItemDetail? GetIetmDetailByID(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return dbContext.ItemDetails.AsNoTracking().FirstOrDefault(x=>x.Id == id);
            }
        }

        private async Task<ItemDetail> GetOrGenerateItemDetailByWorkorderAndSerialNo(Workorder workorder, string serialNo)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var wo = dbContext.Workorders.Include(x => x.ItemDetails).FirstOrDefault(x => x.Id == workorder.Id);
                if (wo.ItemDetails.Any(x => x.SerialNo == serialNo))
                {
                    return wo.ItemDetails.FirstOrDefault(x => x.SerialNo == serialNo);
                }
                else
                {
                    ItemDetail newItemDetail = new ItemDetail(workorder, serialNo);
                    await dbContext.ItemDetails.AddAsync(newItemDetail);
                    await dbContext.SaveChangesAsync();
                    return newItemDetail;
                }
            }
        }
        private async Task<ItemDetail?> GetOrGenerateItemDetailByWorkorderWithoutSerialNo(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var wo = dbContext.Workorders.Include(x => x.ItemDetails).FirstOrDefault(x => x.Id == workorder.Id);
                if (wo is not null)
                {
                    var targetItem = wo.ItemDetails.FirstOrDefault(x => string.IsNullOrEmpty(x.SerialNo));
                    if (targetItem is not null)
                    {
                        var a = dbContext.Entry(targetItem);
                        return targetItem;
                    }
                    else
                    {
                        ItemDetail newItemDetail = new ItemDetail(workorder);
                        await dbContext.ItemDetails.AddAsync(newItemDetail);
                        await dbContext.SaveChangesAsync();
                        return newItemDetail;
                    }
                }
                return null;
            }
        }

        #endregion

        #region item record

        public Task<List<ItemRecordConfig>> GetItemRecordConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();

                return Task.FromResult(dbContext.ItemRecordConfigs.AsNoTracking().ToList());
            }
        }

        public Task<List<ItemRecordConfig>> GetItemRecordConfigsAndContent()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();

                return Task.FromResult(dbContext.ItemRecordConfigs.Include(x => x.ItemRecordContents).AsNoTracking().ToList());
            }
        }

        public async Task<RequestResult> UpsertItemRecordConfig(ItemRecordConfig itemRecordConfig)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ItemRecordConfigs.FirstOrDefault(x => x.Id == itemRecordConfig.Id);
                    if (target != null)
                    {
                        target.ItemRecordCategory = itemRecordConfig.ItemRecordCategory;
                    }
                    else
                    {
                        await dbContext.ItemRecordConfigs.AddAsync(itemRecordConfig);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert item record config {itemRecordConfig.ItemRecordCategory} success");
                }
                catch (Exception ex)
                {
                    return new(4, $"Upsert item record config {itemRecordConfig.ItemRecordCategory} fail({ex.Message})");
                }
            }
        }

        public async Task<RequestResult> DeletetItemRecordConfig(ItemRecordConfig itemRecordConfig)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ItemRecordConfigs.FirstOrDefault(x => x.Id == itemRecordConfig.Id);
                    if (target != null)
                    {
                        dbContext.ItemRecordConfigs.Remove(target);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete Item record config {target.ItemRecordCategory} success");
                    }
                    else
                    {
                        return new(4, $"Item record config {itemRecordConfig.ItemRecordCategory} not exist");
                    }
                }
                catch (Exception ex)
                {
                    return new(4, $"Delete item record config {itemRecordConfig.ItemRecordCategory} fail({ex.Message})");
                }

            }
        }


        public async Task<RequestResult> UpsertItemRecordContent(ItemRecordContent itemRecordContent)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ItemRecordContents.FirstOrDefault(x => x.Id == itemRecordContent.Id);
                    if (target != null)
                    {
                        target.RecordName = itemRecordContent.RecordName;
                        target.DataType = itemRecordContent.DataType;
                    }
                    else
                    {
                        await dbContext.ItemRecordContents.AddAsync(itemRecordContent);
                    }
                    await dbContext.SaveChangesAsync();
                    return new(2, $"Upsert item record content {itemRecordContent.RecordName} success");
                }
                catch (Exception ex)
                {
                    return new(4, $"Upsert item record config {itemRecordContent.RecordName} fail({ex.Message})");
                }
            }
        }

        public async Task<RequestResult> DeletetItemRecordContent(ItemRecordContent itemRecordContent)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var target = dbContext.ItemRecordContents.FirstOrDefault(x => x.Id == itemRecordContent.Id);
                    if (target != null)
                    {
                        dbContext.ItemRecordContents.Remove(target);
                        await dbContext.SaveChangesAsync();
                        return new(2, $"Delete Item record content {target.RecordName} success");
                    }
                    else
                    {
                        return new(4, $"Item record content {itemRecordContent.RecordName} not exist");
                    }
                }
                catch (Exception ex)
                {
                    return new(4, $"Delete item record content {itemRecordContent.RecordName} fail({ex.Message})");
                }

            }
        }


        public List<ItemRecordDetail> RetriveOrGenerateItemRecordDetail(Workorder wo, ItemDetail item)
        {
            List<ItemRecordDetail> res = new();
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();

                //get total item record
                Workorder? targetWo = dbContext.Workorders
                    .Include(x => x.ItemRecordsCategory).ThenInclude(x => x.ItemRecordContents)
                    .FirstOrDefault(x => x.Id == wo.Id);
                if (targetWo != null && targetWo.HasItemRecord && targetWo.ItemRecordsCategory?.ItemRecordContents.Count > 0)
                {
                    List<ItemRecordContent> totalRecord = targetWo.ItemRecordsCategory.ItemRecordContents.ToList();
                    //current record
                    ItemDetail? targetItem = dbContext.ItemDetails.Include(x => x.ItemRecordDetails)
                        .FirstOrDefault(x => x.Id == item.Id);
                    if (targetItem != null)
                    {
                        res = targetItem.ItemRecordDetails.ToList();
                        //fill
                        if (totalRecord.Count() > res.Count())
                        {
                            var emptyRecords = totalRecord.Where(x => !res.Exists(y => y.RecordContentId == x.Id));
                            foreach (ItemRecordContent emptyRecord in emptyRecords)
                            {
                                var tmp = new ItemRecordDetail(targetItem, emptyRecord);
                                tmp.RecordContent = emptyRecord;
                                res.Add(tmp);
                            }
                        }
                    }
                }
            }
            return res;
        }

        public async Task<RequestResult> WriteItemRecord(string serialNo, string recordName, string recordValue)
        {
            Workorder? wo = null;
            ItemDetail? itemDetail = null;
            bool woAndItemFound = false;
            try
            {
                foreach (Station station in stations)
                {
                    wo = null;
                    switch (station.StationType)
                    {
                        case 0:
                            StationSingleWorkorderSingleSerial? stationSingleWorkorderSingleSerial = station as StationSingleWorkorderSingleSerial;
                            if (stationSingleWorkorderSingleSerial is not null && stationSingleWorkorderSingleSerial.HasWorkorder)
                            {
                                wo = stationSingleWorkorderSingleSerial.Workerder;
                                itemDetail = stationSingleWorkorderSingleSerial.ItemDetail;
                            }
                            break;
                        case 1:
                            StationSingleWorkorderMutipleSerial? stationSingleWorkorderMutipleSerial = station as StationSingleWorkorderMutipleSerial;
                            if (stationSingleWorkorderMutipleSerial is not null && stationSingleWorkorderMutipleSerial.HasWorkorder)
                            {
                                wo = stationSingleWorkorderMutipleSerial.Workerder;
                                itemDetail = stationSingleWorkorderMutipleSerial.ItemDetails.FirstOrDefault(x => x.SerialNo == serialNo);
                            }
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                    if (wo != null && itemDetail != null)
                    {
                        woAndItemFound = true;
                        break;
                    }
                }

                if (woAndItemFound)
                {
                    var itemContents = wo?.ItemRecordsCategory?.ItemRecordContents;
                    if (itemContents.Any(x => x.RecordName == recordName))
                    {
                        var targetContent = itemContents.FirstOrDefault(x => x.RecordName == recordName);
                        ItemRecordDetail newItemRecord = new(itemDetail, targetContent);
                        newItemRecord.Value = recordValue;
                        await UpsertItemRecord(new List<ItemRecordDetail> { newItemRecord });
                        return new RequestResult(2, $"record {recordName} for {serialNo} with {recordValue} success");
                    }
                    else
                    {
                        return new RequestResult(4, $"record {recordName} not found in item record category");
                    }
                }
                else
                {
                    return new RequestResult(4, $"serial no {serialNo} not found in running workorder");
                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, $"{ex.Message}");
            }


        }



        public ItemRecordDetail? GetItemRecordDetail(Guid ItemID, Guid? recordID)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return dbContext.ItemRecordDetails.FirstOrDefault(x => x.ItemId == ItemID && x.RecordContentId == recordID);
            }
        }

        //public ItemRecordContent? GetItemDetailRecordContent(ItemRecordDetail itemRecordDetail)
        //{
        //    using (var scope = scopeFactory.CreateScope())
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
        //        ItemRecordDetail? target = dbContext.ItemRecordDetails
        //            .Include(x => x.RecordContent)
        //            .FirstOrDefault(x => x.ItemId == itemRecordDetail.ItemId && x.RecordContentId == itemRecordDetail.RecordContentId);
        //        return target?.RecordContent;
        //    }
        //}

        public async Task UpsertItemRecord(List<ItemRecordDetail> itemRecordDetails)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                foreach (ItemRecordDetail itemRecordDetail in itemRecordDetails)
                {
                    ItemRecordDetail? target = dbContext.ItemRecordDetails.FirstOrDefault(x => x.ItemId == itemRecordDetail.ItemId && x.RecordContentId == itemRecordDetail.RecordContentId);
                    if (target != null)
                    {
                        string? newVal = itemRecordDetail.Value?.Trim();
                        if (newVal != null && !string.IsNullOrEmpty(newVal))
                        {
                            if (target.Value.Trim() != itemRecordDetail.Value?.Trim())
                            {
                                target.Value = itemRecordDetail.Value?.Trim();
                            }

                        }
                    }
                    else
                    {
                        string? newVal = itemRecordDetail.Value?.Trim();
                        if (newVal != null && !string.IsNullOrEmpty(newVal))
                        {
                            await dbContext.ItemRecordDetails.AddAsync(new ItemRecordDetail(itemRecordDetail));
                        }
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region task
        private async Task<TaskDetail> GenerateTaskDetailByItem(Station station, ItemDetail itemDetail)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var newTaskdetail = new TaskDetail(station, itemDetail); ;

                await dbContext.TaskDetails.AddAsync(newTaskdetail);
                await dbContext.SaveChangesAsync();
                return newTaskdetail;
            }

        }
        private async Task UpdateTaskDetailWhenStationOut(TaskDetail taskDetail)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                TaskDetail? target = dbContext.TaskDetails.FirstOrDefault(x => x.Id == taskDetail.Id);
                if (target != null)
                {
                    target.FinishedTime = DateTime.Now;
                    target.Okamount = taskDetail.Okamount;
                    target.Ngamount = taskDetail.Ngamount;
                    await dbContext.SaveChangesAsync();
                }
            }

        }

        public int GetWorkorderOkInStation(Guid workorderID, Guid stationID)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.Workorders.Include(x => x.ItemDetails).ThenInclude(x => x.TaskDetails.Where(x => x.StationId == stationID))
                    .FirstOrDefault(x => x.Id == workorderID);
                if (target == null)
                {
                    return 0;
                }
                else
                {
                    return target.ItemDetails.Select(x => x.TaskDetails).SelectMany(x => x).Sum(x => x.Okamount);
                }
            }
        }

        public int GetWorkorderNgInStation(Guid workorderID, Guid stationID)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.Workorders.Include(x => x.ItemDetails).ThenInclude(x => x.TaskDetails.Where(x => x.StationId == stationID))
                    .FirstOrDefault(x => x.Id == workorderID);
                if (target == null)
                {
                    return 0;
                }
                else
                {
                    return target.ItemDetails.Select(x => x.TaskDetails).SelectMany(x => x).Sum(x => x.Ngamount);
                }
            }
        }
        #endregion

        #region task record

        public Task<List<TaskRecordConfig>> GetTaskRecordConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.TaskRecordConfigs.ToList());
            }
        }

        #endregion

        #region map
        public Task<List<MapImage>> GetAllMapImages()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.MapImages.AsNoTracking().ToList());
            }
        }
        public async Task UpsertImage(MapImage mapImage)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.MapImages.FirstOrDefault(x => x.Id == mapImage.Id);
                if (target != null)
                {
                    target.Name = mapImage.Name;
                    target.DataByte = mapImage.DataByte;
                }
                else
                {
                    await dbContext.AddAsync(mapImage);
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public Task<List<MapConfig>> GetAllMapConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.MapConfigs.ToList());
            }
        }

        public Task<MapConfig?> GetMapConfigAndComponentById(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.MapConfigs
                    .Include(x => x.Image)
                    .Include(x => x.MapComponents)
                    .FirstOrDefault(x => x.Id == id));
            }
        }

        public async Task UpsertMapConfig(MapConfig mapConfig)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.MapConfigs.FirstOrDefault(x => x.Id == mapConfig.Id);
                if (target != null)
                {
                    target.Name = mapConfig.Name;
                    target.ImageId = mapConfig.ImageId;
                }
                else
                {
                    await dbContext.AddAsync(mapConfig);
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<RequestResult> UpsertMapComponents(MapComponent mapComponent)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();

                    var target = dbContext.MapComponents.FirstOrDefault(x => x.Id == mapComponent.Id);
                    //update
                    if (target != null)
                    {
                        target.Type = mapComponent.Type;
                        target.TargetId = mapComponent.TargetId;
                        target.PositionX = mapComponent.PositionX;
                        target.PositionY = mapComponent.PositionY;
                        target.Height = mapComponent.Height;
                        target.Width = mapComponent.Width;
                    }
                    //insert
                    else
                    {
                        await dbContext.MapComponents.AddAsync(mapComponent);
                    }
                    await dbContext.SaveChangesAsync();
                }
                return new(2, "save map success");
            }
            catch (Exception ex)
            {
                return new(4, $"save map fail({ex.Message})");
            }
        }

        public async Task<RequestResult> UpsertMapComponentsAttribute(Guid mapID, IEnumerable<MapComponent> mapComponents)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var deSet = dbContext.MapComponents;
                    var mapComponentInDBs = deSet.Where(x => x.MapId == mapID);

                    foreach (var e in mapComponentInDBs)
                    {
                        if(!mapComponents.Any(x=>x.Id == e.Id))
                        {
                            deSet.Remove(e);
                        }
                    }
                    

                    foreach (var mapComponent in mapComponents)
                    {
                        var target = deSet.FirstOrDefault(x => x.Id == mapComponent.Id);
                        if (target != null)
                        {
                            target.Type = mapComponent.Type;
                            target.TargetId = mapComponent.TargetId;
                            target.PositionX = mapComponent.PositionX;
                            target.PositionY = mapComponent.PositionY;
                            target.Height = mapComponent.Height;
                            target.Width = mapComponent.Width;
                        }
                        else
                        {
                            deSet.AddAsync(mapComponent);
                        }

                    }
                    await dbContext.SaveChangesAsync();
                }
                return new(2, "save map success");
            }
            catch (Exception ex)
            {
                return new(4, $"save map fail({ex.Message})");
            }
        }

        public async Task<RequestResult> DeleteMapComponents(MapComponent mapComponent)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();

                    var target = dbContext.MapComponents.FirstOrDefault(x => x.Id == mapComponent.Id);
                    if (target != null)
                    {
                        dbContext.Remove(target);
                    }
                    else
                    {
                        return new(4, "component not found");
                    }
                    await dbContext.SaveChangesAsync();
                }
                return new(2, "save map success");
            }
            catch (Exception ex)
            {
                return new(4, $"save map fail({ex.Message})");
            }
        }

        #endregion

        #region log
        private async Task WriteEventLog(RequestResult requestResult)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var eventLogService = scope.ServiceProvider.GetRequiredService<EventLogService>();
                await eventLogService.AddEventLog(requestResult);
            }
        }
        #endregion

        #region developer

        public async Task<RequestResult> ResetWorkorderById(Guid id)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    var itemDetails = dbContext.ItemDetails.Include(x => x.ItemRecordDetails)
                        .Include(x => x.TaskDetails).ThenInclude(x => x.TaskRecordDetails)
                        .Where(x => x.WorkordersId == id).ToList();
                    if (itemDetails != null)
                    {
                        dbContext.ItemDetails.RemoveRange(itemDetails);
                    }
                    var workorder = dbContext.Workorders.FirstOrDefault(x => x.Id == id);
                    if (workorder != null)
                    {
                        workorder.Status = (int)WorkorderStatus.Init;
                        workorder.Okamount = 0;
                        workorder.Ngamount = 0;
                        workorder.StartTime = null;
                        workorder.FinishedTime = null;
                    }
                    await dbContext.SaveChangesAsync();
                    return new RequestResult(2, $"reset ({id}) {workorder.WorkorderNo}/{workorder.Lot} success");
                }
            }
            catch (Exception e)
            {
                return new RequestResult(4, $"reset fail ({e.Message})");

            }

        }

        




        #endregion
    }
}
