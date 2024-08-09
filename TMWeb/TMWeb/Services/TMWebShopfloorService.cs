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
        public Task<List<Process>> GetAllProcess()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                return Task.FromResult(dbContext.Processes.Include(x => x.Stations.OrderBy(x => x.ProcessIndex)).ToList());
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
                return Stations.Where(x => x.ProcessId == targetProcess.Id).ToList();
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

        public async Task StationInByName(string stationName, string serialNo)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationType <= 2)//single workorder
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                            StationSingleWorkorder? stationSingleWorkorder = targetStation as StationSingleWorkorder;
                            var itemDetail = await GetOrGenerateItemDetailByWorkorder(stationSingleWorkorder.Workerder, serialNo);
                            stationSingleWorkorder?.AddItemDetail(itemDetail);
                            var taskDetail = await GenerateTaskDetailByItem(targetStation, itemDetail);
                            stationSingleWorkorder?.AddTaskDetail(taskDetail);
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        public async Task StationOutBySerialNo(string stationName, string serialNo)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationType <= 2)//single workorder
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                            StationSingleWorkorder? stationSingleWorkorder = targetStation as StationSingleWorkorder;
                            var itemDetail = await GetOrGenerateItemDetailByWorkorder(stationSingleWorkorder.Workerder, serialNo);
                            stationSingleWorkorder?.AddItemDetail(itemDetail);
                            var taskDetail = await GenerateTaskDetailByItem(targetStation, itemDetail);
                            stationSingleWorkorder?.AddTaskDetail(taskDetail);
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
                }

            }
        }
        public async Task StationOutByFIFO(string stationName)
        {
            Station? targetStation = await GetStationsByName(stationName);
            if (targetStation != null)
            {
                if (targetStation.StationType <= 2)//single workorder
                {
                    switch (targetStation.StationType)
                    {
                        case 0:
                            StationSingleWorkorder? stationSingleWorkorder = targetStation as StationSingleWorkorder;
                            var itemDetail = await GetOrGenerateItemDetailByWorkorder(stationSingleWorkorder.Workerder, serialNo);
                            stationSingleWorkorder?.AddItemDetail(itemDetail);
                            var taskDetail = await GenerateTaskDetailByItem(targetStation, itemDetail);
                            stationSingleWorkorder?.AddTaskDetail(taskDetail);
                            break;
                        case 1:
                            break;
                        case 2:
                            break;
                        default:
                            break;
                    }
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

        private async Task<ItemDetail> GetOrGenerateItemDetailByWorkorder(Workorder workorder, string serialNo)
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
        #endregion
    }
}
