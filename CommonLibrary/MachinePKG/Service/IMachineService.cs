using CommonLibrary.API.Message;
using CommonLibrary.MachinePKG.AnalysisData;
using CommonLibrary.MachinePKG.EFModel;
using DevExpress.XtraPrinting.Shape.Native;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG
{
    public interface IMachineService
    {
        #region machine
        public List<Machine> Machines { get; }

        public Task<List<Machine>> GetAllMachinesConfig();

        public Task InitAllMachinesFromDB();

        public Machine? InitMachineFromDBById(Guid id);

        public Machine InitMachineToDerivesClass(Machine machine);

        public Task<RequestResult> UpsertMachineConfig(Machine machine);

        public Task<RequestResult> DeleteMachine(Machine machine);

        public Task RefreshMachine(Machine machine, DataEditMode dataEditMode);

        public Action<Guid, DataEditMode>? MachineConfigChangedAct { get; set; }

        public void MachineConfigChanged(Guid id, DataEditMode mode);

        public Task<Machine?> GetMachineByID(Guid? id);

        public Task<Machine?> GetMachineByName(string name);

        public void MachineStatusChangedRecord(Machine machine, MachineStatusRecordType machineStatusRecordType);

        #endregion

        #region utilization
        public Task<List<MachineStatusLog>> GetMachineStatusLogByID(MachineUtilizationDTO machineUtilizationDTO);

        public IAsyncEnumerable<MachineStatusInterval> CalculateMachineStatusIntervalByOrderedLog(List<MachineStatusLog> machineStatusLogs, ushort delayMilliSec = 0, IProgress<int>? progress = null);
        #endregion

        #region tag

        public Task<List<TagCategory>> GetAllTagCategories();

        public Task<List<TagCategory>> GetAllTagCategoriesWithTags();

        public List<Tag> GetTagsByCatId(Guid? catID);

        public int GetTagTypeCodeByIds(Guid? catID, Guid? tagID);

        public Task<List<TagCategory>> GetCategoryByConnectionType(int connectionType);

        public Task<RequestResult> UpsertTagCategory(TagCategory tagCategory);

        public Task<RequestResult> DeleteTagCategory(TagCategory tagCategory);

        public Task<RequestResult> UpsertTag(Tag tag);

        public Task<RequestResult> DeleteTag(Tag tag);

        public Task<Tag?> GetMachineTag(string machineName, string tagName);

        public Task<RequestResult> SetMachineTag(string machineName, string tagName, Object val);

        #endregion

        #region StatusCondition
        public Task<List<LogicStatusCategory>> GetCustomStatusAndConditions();

        public Task<RequestResult> UpsertCustomStatusCategory(LogicStatusCategory tagCategory);

        public Task<RequestResult> DeleteCustomStatusCategory(LogicStatusCategory tagCategory);

        public Task<RequestResult> UpsertCustomStatusCondition(LogicStatusCondition condition);

        public Task<RequestResult> DeleteCustomStatusCondition(LogicStatusCondition condition);

        #endregion

        #region Error code

        public Task<List<ErrorCodeCategory>> GetErrorCodeTables();

        public Task<RequestResult> UpsertErrorCodeCategory(ErrorCodeCategory errorCodeCategory);

        public Task<RequestResult> DeleteErrorCodeCategory(ErrorCodeCategory errorCodeCategory);

        public Task<RequestResult> UpsertErrorCodeMapping(ErrorCodeMapping errorCodeMapping);

        public Task<RequestResult> DeleteErrorCodeMapping(ErrorCodeMapping errorCodeMapping);

        #endregion
    }
}
