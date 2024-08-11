using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using TMWeb.Components.Pages.Setting;
using TMWeb.Data;
using TMWeb.EFModels;

namespace TMWeb.Services
{
    public class TMWebShopfloorService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public TMWebShopfloorService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;

            InitAllStations();
        }
        #region process

        public async Task UpsertProcessAndStations(Process process)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                Process? targetProcess = dbContext.Processes.Include(x=>x.Stations).FirstOrDefault(x => x.Id == process.Id);
                if (targetProcess != null)
                {
                    targetProcess.Name = process.Name;
                    targetProcess.Stations = process.Stations;
                    //targetProcess = process;
                }
                else
                {
                    var a = await dbContext.Processes.AddAsync(process);
                }
                await dbContext.SaveChangesAsync();
            }
        }

        public Task<List<Process>> GetAllProcessAndStations()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.Include(x => x.Stations.OrderBy(x => x.ProcessIndex).ThenBy(x=>x.Name)).ToList());
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
        public Task<Process?> GetProcessByID(Guid processID)
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
            return stations.Max(x => x.ProcessIndex) == station.ProcessIndex;
        }

        #endregion

        #region station
        private List<Station> stations = new List<Station>();
        public List<Station> Stations => stations;
        public async Task<List<Station>> GetStationsByProcessName(string processName)
        {
            Process? targetProcess = await GetProcessByName(processName);
            if (targetProcess != null)
            {
                return Stations.Where(x => x.ProcessId == targetProcess.Id).OrderBy(x=>x.ProcessIndex).ThenBy(x=>x.Name).ToList();
            }
            return new();
        }
        public async Task<List<Station>> GetStationsByProcessID(Guid processID)
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
        public void InitAllStations()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var stationbases = dbContext.Stations.Include(x => x.Process).ToList();
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

        public async Task StationInByNameAndSerialNo(string stationName, string serialNo)
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
                            StationSingleWorkorder? stationSingleWorkorder = targetStation as StationSingleWorkorder;
                            var itemDetail = await GetOrGenerateItemDetailByWorkorderAndSerialNo(stationSingleWorkorder.Workerder, serialNo);
                            stationSingleWorkorder?.AddItemDetail(itemDetail);
                            var taskDetail = await GenerateTaskDetailByItem(targetStation, itemDetail);
                            stationSingleWorkorder?.AddTaskDetail(taskDetail);
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                }

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
                                bool isLast = await CheckStationIsLastInProcess(targetStation);
                                if (isLast)
                                {
                                    await SummaryItemFromTaskWithSerialNo(targetItemDetail);
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
                                bool isLast = await CheckStationIsLastInProcess(targetStation);
                                if (isLast)
                                {
                                    await SummaryItemFromTaskWithSerialNo(targetItemDetail);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        public async Task StationOutByFIFO(string stationName, bool pass)
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

                                
                                bool isLast = await CheckStationIsLastInProcess(targetStation);
                                if (isLast)
                                {
                                    ItemDetail? targetItemDetail = StationSingleWorkorder?.RemoveItemDetail();
                                    await SummaryItemFromTaskWithSerialNo(targetItemDetail);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

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

        public async Task SummaryItemFromTaskWithSerialNo(ItemDetail itemDetail)
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
                        targetItem.Okamount = 0;
                    }
                    else
                    {
                        targetItem.Ngamount = 0;
                        targetItem.Okamount = targetItem.TaskDetails.Min(x => x.Okamount);
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
                return Task.FromResult(dbContext.Workorders.ToList());
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
                return Task.FromResult(dbContext.Workorders.Where(x => x.ProcessId == processID && targetStatus.Contains(x.Status)).ToList());
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
                    .Include(x => x.ItemDetails).ThenInclude(x => x.TaskDetails).ThenInclude(x => x.Station)
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
        public Task<Workorder?> GetWorkorderAndRecipeByNoAndLot(string wo, string lot)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Workorders.Include(x => x.WorkorderRecordCategoryId).FirstOrDefault(x => x.WorkorderNo == wo && x.Lot == lot));
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
                var a = dbContext.WorkorderRecipeConfigs.Include(x => x.WorkorderRecipeContents).ToList();
                return Task.FromResult(dbContext.WorkorderRecipeConfigs.Include(x => x.Workorders).ToList());//.Include(x=>x.WorkorderRecipeContents).ThenInclude(x =>x.WorkorderRecipeDetails)
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
        #endregion
    }
}
