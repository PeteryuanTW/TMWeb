using DevExpress.Pdf;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TMWeb.Components.Pages.Setting;
using CommonLibrary.API.Message;
using TMWeb.Data;
using TMWeb.EFModels;
using static System.Collections.Specialized.BitVector32;

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
            _ = InitAll();
        }

        public async Task InitAll()
        {
            InitAllStations();
            await InitAllMachinesFromDB();
        }


        #region process
        public Task<List<Process>> GetAllProcess()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.ToList());
            }
        }

        public Task<List<string>> GetAllProcessName()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.Select(x => x.Name).ToList());
            }
        }

        public async Task<RequestResult> UpsertProcessAndStations(Process process)
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
                        foreach (var station in process.Stations)
                        {
                            await UpdateStationConfig(station);
                        }
                        InitAllStations();
                    }
                    else
                    {
                        var a = await dbContext.Processes.AddAsync(process);
                    }
                    await dbContext.SaveChangesAsync();
                    return new RequestResult(2, "Upsert success");
                }
            }
            catch (Exception ex)
            {
                return new RequestResult(4, ex.Message);
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
        public Task<List<Station>> GetAllStationsConfig()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Stations.AsNoTracking().ToList());
            }
        }

        public async Task UpdateStationConfig(Station station)
        {
            using (var scope = scopeFactory.CreateScope())
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
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private List<Station> stations = new List<Station>();
        public List<Station> Stations => stations;

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
                if (targetStation.Status == Status.Running)
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
                if (targetStation.Status == Status.Running)
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
                                taskDetail.Okamount += ok;
                                taskDetail.Ngamount += ng;
                                taskDetail.FinishedTime = DateTime.Now;
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
                    targetItem.Ngamount = targetItem.TaskDetails.Max(x => x.Ngamount);
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

        public Task<List<Machine>> GetAllMachinesConfig()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Machines.AsNoTracking().ToList());
            }
        }

        private List<Machine> machines = new();
        public List<Machine> Machines => machines;
        private async Task InitAllMachinesFromDB()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var tmp = dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags).AsNoTracking().ToList();
                machines = tmp.Select(x => InitMachineToDerivesClass(x)).ToList();
                foreach (Machine machine in machines)
                {
                    machine.InitMachine();
                    if (machine.Enabled)
                    {
                        await machine.ConnectAsync();
                        //machine.Running();
                    }
                }
            }
        }
        private Machine? InitMachineFromDBById(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var tmp = dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags).AsNoTracking().FirstOrDefault(x => x.Id == id);
                tmp = InitMachineToDerivesClass(tmp);
                tmp.InitMachine();
                if (tmp.Enabled)
                {
                    tmp.ConnectAsync();
                    //tmp.Running();
                }
                return tmp;
            }
        }
        private Machine InitMachineToDerivesClass(Machine machine)
        {
            switch (machine.ConnectionType)
            {
                case 0:
                    return new ModbusTCPMachine(machine);
                case 1:
                    return new TMRobotModbusTCP(machine);
                case 2:
                case 10:
                default:
                    return machine;
            }
        }
        public async Task UpsertMachineConfig(Machine machine)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.Machines.FirstOrDefault(x => x.Id == machine.Id);
                if (target != null)
                {
                    target.Name = machine.Name;
                    target.ProcessId = machine.ProcessId;
                    target.Ip = machine.Ip;
                    target.Port = machine.Port;
                    target.ConnectionType = machine.ConnectionType;
                    target.TagCategoryId = machine.TagCategoryId;
                    target.Enabled = machine.Enabled;
                }
                else
                {

                }
                await dbContext.SaveChangesAsync();
                await RefreshMachine(target);
            }
        }
        private async Task RefreshMachine(Machine machine)
        {
            Machine target = await GetMachineByID(machine.Id);
            if (target != null)
            {
                machines.Remove(target);
            }
            machines.Add(InitMachineFromDBById(machine.Id));
        }

        public async Task<List<Machine>> GetMachineByProcessName(string processName)
        {
            Process? target = await GetProcessByName(processName);
            return machines.Where(x => x.ProcessId == target.Id).ToList();
        }
        public Task<List<Machine>> GetMachineByProcessID(Guid id)
        {
            return Task.FromResult(machines.Where(x => x.ProcessId == id).ToList());
        }

        public async Task<RecipeCheckTable> GetRecipeCheckTableInProcess(string processName, WorkorderRecipeContent workorderRecipeContent)
        {
            RecipeCheckTable res = new(workorderRecipeContent);
            var machineInProcess = await GetMachineByProcessName(processName);
            var targetMachine = machineInProcess.Where(x => x.TagCategoryId == workorderRecipeContent.TagCategoryId).ToList();
            foreach (var machine in targetMachine)
            {
                var targetTag = machine.TagCategory.Tags.FirstOrDefault(x => x.Id == workorderRecipeContent.TagId);
                if (targetTag != null)
                {
                    await machine.UpdateTag(targetTag);
                    res.AddCheckItem(new(machine, targetTag));
                }
            }
            return res;
        }

        public Task<Machine?> GetMachineByID(Guid? id)
        {
            return Task.FromResult(machines.FirstOrDefault(x => x.Id == id));
        }
        public Task<Machine?> GetMachineByName(string name)
        {
            return Task.FromResult(machines.FirstOrDefault(x => x.Name == name));
        }
        #endregion

        #region tag

        public Task<List<TagCategory>> GetCategoryByConnectionType(int connectionType)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.TagCategories.Where(x => x.ConnectionType == connectionType).ToList());
            }
        }

        public async Task<Tag?> GetMachineTag(string machineName, string tagName)
        {
            Machine? targetMachine = await GetMachineByName(machineName);
            if (targetMachine != null)
            {
                if (targetMachine.hasCategory)
                {
                    Tag? targetTag = targetMachine.TagCategory.Tags.FirstOrDefault(x => x.Name == tagName);
                    if (targetTag!=null)
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

        public async Task SetMachineTag(string machineName, string tagName, Object val)
        {
            Machine? targetMachine = await GetMachineByName(machineName);
            if (targetMachine != null)
            {
                if (targetMachine.hasCategory)
                {
                    Tag? targetTag = targetMachine.TagCategory.Tags.FirstOrDefault(x => x.Name == tagName);
                    if (targetTag != null)
                    {
                        await targetMachine.SetTag(targetTag.Name, val);
                    }
                }
            }
        }

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
        public Task<Workorder?> GetWorkordersDetails(Guid id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.Include(x => x.Process)
                    .Include(x => x.RecipeCategory).ThenInclude(x => x.WorkorderRecipeContents)
                    .Include(x => x.WorkorderRecordCategory).ThenInclude(x => x.WorkorderRecordContents).ThenInclude(x => x.WorkorderRecordDetails)
                    .Include(x => x.ItemRecordsCategory).ThenInclude(x => x.ItemRecordContents).ThenInclude(x => x.ItemRecordDetails)
                    .Include(x => x.TaskRecordCategory).ThenInclude(x => x.TaskRecordContents).ThenInclude(x => x.TaskRecordDetails)
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

        public async Task UpsertWorkorderConfig(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var target = dbContext.Workorders.FirstOrDefault(x => x.Id == workorder.Id);
                if (target != null)
                {
                    target.ProcessId = workorder.ProcessId;
                    target.WorkorderNo = workorder.WorkorderNo;
                    target.Lot = workorder.Lot;
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
            }
        }
        public Task<Workorder?> GetWorkorderAndRecipeByIdString(string id)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.FirstOrDefault(x => x.Id == new Guid(id)));
            }
        }
        #endregion

        #region recipe
        public Task<WorkorderRecipeConfig?> GetRecipeConfigsByName(string RecipeName)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x => x.WorkorderRecipeContents).FirstOrDefault(x => x.RecipeCategory == RecipeName));
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
                var a = dbContext.WorkorderRecipeConfigs.Include(x => x.WorkorderRecipeContents).ToList();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x => x.Workorders).ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
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
        #endregion

        #region item

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
        private async Task<ItemDetail> GetOrGenerateItemDetailByWorkorderWithoutSerialNo(Workorder workorder)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var wo = dbContext.Workorders.Include(x => x.ItemDetails).FirstOrDefault(x => x.Id == workorder.Id);
                if (wo.ItemDetails.Any(x => string.IsNullOrEmpty(x.SerialNo)))
                {
                    return wo.ItemDetails.FirstOrDefault(x => string.IsNullOrEmpty(x.SerialNo));
                }
                else
                {
                    ItemDetail newItemDetail = new ItemDetail(workorder);
                    await dbContext.ItemDetails.AddAsync(newItemDetail);
                    await dbContext.SaveChangesAsync();
                    return newItemDetail;
                }
            }
        }

        #endregion

        #region item record

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
                if (targetWo != null && targetWo.ItemRecordsCategory.ItemRecordContents.Count > 0)
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
                            if (stationSingleWorkorderSingleSerial != null)
                            {
                                wo = stationSingleWorkorderSingleSerial.Workerder;
                                itemDetail = stationSingleWorkorderSingleSerial.ItemDetail;
                            }
                            break;
                        case 1:
                            StationSingleWorkorderMutipleSerial? stationSingleWorkorderMutipleSerial = station as StationSingleWorkorderMutipleSerial;
                            if (stationSingleWorkorderMutipleSerial != null)
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

        public Task<List<ItemRecordConfig>> GetItemRecordConfigs()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();

                return Task.FromResult(dbContext.ItemRecordConfigs.ToList());
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
                        target.MachineId = mapComponent.MachineId;
                        target.StationId = mapComponent.StationId;
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

        public async Task<RequestResult> UpsertMapComponentsAttribute(IEnumerable<MapComponent> mapComponents)
        {
            try
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                    foreach (var mapComponent in mapComponents)
                    {
                        var target = dbContext.MapComponents.FirstOrDefault(x => x.Id == mapComponent.Id);
                        if (target != null)
                        {
                            target.Type = mapComponent.Type;
                            target.MachineId = mapComponent.MachineId;
                            target.StationId = mapComponent.StationId;
                            target.PositionX = mapComponent.PositionX;
                            target.PositionY = mapComponent.PositionY;
                            target.Height = mapComponent.Height;
                            target.Width = mapComponent.Width;
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
                        workorder.Status = 0;
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
