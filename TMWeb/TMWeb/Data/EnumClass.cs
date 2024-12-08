using DevExpress.Blazor;
using System.Reflection;
using System.Drawing;
using TMWeb.Data.CustomAttribute;
using TMWeb.EFModels;
using CommonLibrary.Data;
using CommonLibrary.MachinePKG;

namespace TMWeb.Data
{
    public static class TypeEnumHelper
    {
        //public static IEnumerable<ConnectionTypeWrapperClass> GetConnectTypesWrapperClass()
        //{
        //    return Enum.GetValues(typeof(ConnectType)).OfType<ConnectType>()
        //        .Select(x => new ConnectionTypeWrapperClass(x));
        //}

        public static IEnumerable<StationTypeWrapperClass> GetStationTypesWrapperClass()
        {
            return Enum.GetValues(typeof(StationType)).OfType<StationType>()
                .Select(x => new StationTypeWrapperClass(x));
        }

        //public static Dictionary<DataType, Type> TypeDict = new()
        //{
        //    { DataType.Bool, typeof(bool) },
        //    { DataType.Ushort, typeof(ushort) },
        //    { DataType.Float, typeof(float) },
        //    { DataType.String, typeof(string) },
        //    //{ DataType.ArrayOfBool, typeof(bool[]) },
        //    //{ DataType.ArrayOfUshort, typeof(ushort[]) },
        //    //{ DataType.ArrayOfFloat, typeof(float[]) },
        //    //{ DataType.ArrayOfString, typeof(string[]) },

        //};

        //public static bool TypeMatch(int? code, Type? type)
        //{
        //    if (code is null || type is null)
        //    {
        //        return false;
        //    }
        //    DataType dt = (DataType)code;
        //    if (TypeDict.ContainsKey(dt))
        //    {
        //        if (TypeDict[dt] == type)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public static bool TypeMatch(DataType dt, Type type)
        //{
        //    if (TypeDict.ContainsKey(dt))
        //    {
        //        if (TypeDict[dt] == type)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public static IEnumerable<DataTypeWrapperClass> GetDataTypesWrapperClass()
        //{
        //    return Enum.GetValues(typeof(DataType)).OfType<DataType>()
        //        .Select(x => new DataTypeWrapperClass(x));
        //}

        //public static IEnumerable<SpecialTagTypeWrapperClass> GetSpecialTagTypesWrapperClass()
        //{
        //    return Enum.GetValues(typeof(SpecialTagType)).OfType<SpecialTagType>()
        //        .Select(x => new SpecialTagTypeWrapperClass(x));
        //}

        public static IEnumerable<BuildInRecipeWrapperClass> GetBuildInRecipeWrapperClass()
        {
            IEnumerable<PropertyInfo> buildinProperties = typeof(Workorder).GetProperties().Where(x => x.IsDefined(typeof(PublicPropertyAttribute), false));
            foreach (var p in buildinProperties)
            {
                var targetInDict = MachineTypeEnumHelper.TypeDict.FirstOrDefault(x => x.Value == p.PropertyType);
                if (!default(KeyValuePair<DataType, Type>).Equals(targetInDict))
                {
                    yield return new BuildInRecipeWrapperClass(targetInDict.Key, p.Name);
                }
            }
        }

        public static IEnumerable<RecipeTriggerTimingWrapperClass> GetRecipeTriggerTimingWrapperClass()
        {
            return Enum.GetValues(typeof(RecipeTriggerTiming)).OfType<RecipeTriggerTiming>()
                .Select(x => new RecipeTriggerTimingWrapperClass(x));
        }

        //public static IEnumerable<StatusStyle> StatusStyles =>
        //    new List<StatusStyle>
        //    {
        //        new (Status.Init, ButtonRenderStyle.Secondary, Color.FromArgb(143, 143, 143)),
        //        new (Status.TryConnecting, ButtonRenderStyle.Info, Color.FromArgb(91, 91, 174)),
        //        new (Status.Disconnect, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),
        //        new (Status.Idle, ButtonRenderStyle.Info, Color.FromArgb(130, 192, 192)),
        //        new (Status.Running, ButtonRenderStyle.Success, Color.FromArgb(1, 178, 104)),
        //        new (Status.Pause, ButtonRenderStyle.Warning, Color.FromArgb(235, 192, 0)),
        //        new (Status.Stop, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),
        //        new (Status.Error, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),
        //    };

        //public static IEnumerable<StatusWrapperClass> GetStatusWrapperClass()
        //{
        //    return Enum.GetValues(typeof(Status)).OfType<Status>()
        //        .Select(x => new StatusWrapperClass(x));
        //}

        public static IEnumerable<MapComponentTargetTypeWrapperClass> GetMapComponentTargetTypeWrapperClass()
        {
            return Enum.GetValues(typeof(MapComponentTargetType)).OfType<MapComponentTargetType>()
                .Select(x => new MapComponentTargetTypeWrapperClass(x));
        }

        //public static StatusStyle? GetStatusStyle(int statusCode)
        //{
        //    var target = StatusStyles.FirstOrDefault(x => (int)x.status == statusCode);
        //    return target;
        //}

        public static IEnumerable<MapEditCommandWrapperClass> GetMapEditCommandWrapperClass()
        {
            return Enum.GetValues(typeof(MapEditCommand)).OfType<MapEditCommand>()
                .Select(x => new MapEditCommandWrapperClass(x));
        }
    }

    

    //public enum ModbusTCPAction
    //{
    //    ReadCoils = 1,
    //    ReadDiscreteInputs = 2,
    //    ReadHoldingRegisters = 3,
    //    ReadInputRegisters = 4,
    //    WriteSingleCoil = 5,
    //    WriteSingleRegister = 6,
    //    WriteMultipleCoils = 15,
    //    WriteMultipleRegisters = 16,
    //}

    public enum EventLogLevel
    {
        info = 1,
        success = 2,
        warning = 3,
        danger = 4,
    }

    //public class ConnectionTypeWrapperClass : WrapperClass
    //{
    //    public ConnectionTypeWrapperClass(ConnectType type)
    //    {
    //        Type = type;
    //        index = (int)Type;
    //        displayName = Type.ToString();
    //    }
    //    public ConnectType Type { get; init; }
    //}
    //public enum ConnectType
    //{
    //    ModbusTCP = 0,
    //    TMRobot = 1,
    //    ModbusTCPother = 2,
    //    WebAPI = 10,
    //}

    //public class DataTypeWrapperClass : WrapperClass
    //{
    //    public DataTypeWrapperClass(DataType dataType)
    //    {
    //        Type = dataType;
    //        index = (int)Type;
    //        displayName = Type.ToString();
    //    }

    //    public DataType Type { get; init; }
    //    public Type csType => TypeEnumHelper.TypeDict[Type];
    //}

    public class BuildInRecipeWrapperClass : WrapperClass
    {
        public BuildInRecipeWrapperClass(DataType dataType, string propertyName)
        {
            index = (int)dataType;
            displayName = propertyName;
        }
    }
    //public enum DataType
    //{
    //    Bool = 1,
    //    Ushort = 2,
    //    Float = 3,
    //    String = 4,
    //    //ArrayOfBool = 11,
    //    //ArrayOfUshort = 22,
    //    //ArrayOfFloat = 33,
    //    //ArrayOfString = 44,
    //}


    //public class SpecialTagTypeWrapperClass : WrapperClass
    //{
    //    public SpecialTagTypeWrapperClass(SpecialTagType specialTagType)
    //    {
    //        index = (int)specialTagType;
    //        displayName = specialTagType.ToString();
    //    }
    //}
    //public enum SpecialTagType
    //{
    //    General,
    //    CustomStatus,
    //    DetailCode,
    //}

    public class StationTypeWrapperClass : WrapperClass
    {
        public StationTypeWrapperClass(StationType stationType)
        {
            Type = stationType;
            index = (int)Type;
            displayName = Type.ToString();

        }
        public StationType Type { get; init; }

    }
    public enum StationType
    {
        SingleWorkorderSingleSerial = 0,
        SingleWorkorderMultipleSerials = 1,
        SingleWorkorderNoSerial = 2,

        MultipleWorkorderSingleSerial = 3,
        MultipleWorkorderMutipleSerials = 4,
        MultipleWorkorderNoSerial = 5,
    }

    public class RecipeTriggerTimingWrapperClass : WrapperClass
    {
        public RecipeTriggerTimingWrapperClass(RecipeTriggerTiming recipeTriggerTiming)
        {
            RecipeTriggerTiming = recipeTriggerTiming;
            index = (int)RecipeTriggerTiming;
            displayName = RecipeTriggerTiming.ToString();
        }

        public RecipeTriggerTiming RecipeTriggerTiming { get; init; }
    }

    public enum RecipeTriggerTiming
    {
        WorkorderStart = 0,
        WorkorderFinish = 1,
    }

    //machine and station status
    //public class StatusWrapperClass : WrapperClass
    //{
    //    public StatusWrapperClass(Status status)
    //    {
    //        Status = status;
    //        index = (int)status;
    //        displayName = status.ToString();
    //    }

    //    public Status Status { get; init; }
    //}

    
    //public enum Status
    //{
    //    Init,
    //    TryConnecting,
    //    Disconnect,
    //    Idle,
    //    Running,
    //    Pause,
    //    Stop,
    //    Error,
    //}

    public class MapComponentTargetTypeWrapperClass : WrapperClass
    {
        public MapComponentTargetTypeWrapperClass(MapComponentTargetType mapComponentTargetType)
        {
            MapComponentTargetType = mapComponentTargetType;
            index = (int)MapComponentTargetType;
            displayName = MapComponentTargetType.ToString();
        }

        public MapComponentTargetType MapComponentTargetType { get; init; }
    }

    public enum MapComponentTargetType
    {
        Station = 0,
        Machine = 1,
    }

    public class MapEditCommandWrapperClass : WrapperClass
    {
        public MapEditCommandWrapperClass(MapEditCommand mapEditCommand)
        {
            MapEditCommand = mapEditCommand;
            index = (int)MapEditCommand;
            displayName = MapEditCommand.ToString();
        }

        public MapEditCommand MapEditCommand { get; init; }
    }

    public enum MapEditCommand
    {
        Move,
        Resize,
    }

    //public enum DataEditMode
    //{
    //    Insert,
    //    Update,
    //    Delete,
    //}

}
