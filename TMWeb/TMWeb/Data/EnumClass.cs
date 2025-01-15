using DevExpress.Blazor;
using System.Reflection;
using System.Drawing;
using TMWeb.Data.CustomAttribute;
using TMWeb.EFModels;
using CommonLibrary.Data;
using CommonLibrary.MachinePKG;
using Serilog.Core;
using Serilog.Events;
using Microsoft.IdentityModel.Abstractions;

namespace TMWeb.Data
{
    public static class TypeEnumHelper
    {
        public static IEnumerable<EventLogLevelStyle> EventLogLevelStyle =>
            new List<EventLogLevelStyle>
            {
                new (EventLogLevel.info, ButtonRenderStyle.Info, Color.FromArgb(130, 192, 192)),
                new (EventLogLevel.success, ButtonRenderStyle.Success, Color.FromArgb(1, 178, 104)),
                new (EventLogLevel.warning, ButtonRenderStyle.Warning, Color.FromArgb(235, 192, 0)),
                new (EventLogLevel.danger, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),
            };

        public static EventLogLevelStyle? GetEventLogLevelStyle(int statusCode)
        {
            var target = EventLogLevelStyle.FirstOrDefault(x => (int)x.eventLogLevel == statusCode);
            return target;
        }

        public static IEnumerable<SerilogEventLogLevelStyle> SerilogEventLogLevelStyle =>
            new List<SerilogEventLogLevelStyle>
            {
                new (LogEventLevel.Verbose, Color.FromArgb(100, 0, 0, 0)),
                new (LogEventLevel.Debug, Color.FromArgb(100, 0, 0, 0)),
                new (LogEventLevel.Information, Color.FromArgb(80, 130, 192, 192)),
                new (LogEventLevel.Warning, Color.FromArgb(80, 235, 192, 0)),
                new (LogEventLevel.Error, Color.FromArgb(80, 204, 0, 0)),
                new (LogEventLevel.Fatal, Color.FromArgb(80, 204, 0, 0)),
            };

        public static SerilogEventLogLevelStyle? GetSerilogEventLogLevelStyle(int statusCode)
        {
            var target = SerilogEventLogLevelStyle.FirstOrDefault(x => (int)x.logEventLevel == statusCode);
            return target;
        }
        public static IEnumerable<StationTypeWrapperClass> GetStationTypesWrapperClass()
        {
            return Enum.GetValues(typeof(StationType)).OfType<StationType>()
                .Select(x => new StationTypeWrapperClass(x));
        }

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


        public static IEnumerable<MapComponentTargetTypeWrapperClass> GetMapComponentTargetTypeWrapperClass()
        {
            return Enum.GetValues(typeof(MapComponentTargetType)).OfType<MapComponentTargetType>()
                .Select(x => new MapComponentTargetTypeWrapperClass(x));
        }


        public static IEnumerable<MapEditCommandWrapperClass> GetMapEditCommandWrapperClass()
        {
            return Enum.GetValues(typeof(MapEditCommand)).OfType<MapEditCommand>()
                .Select(x => new MapEditCommandWrapperClass(x));
        }
    }



    public enum EventLogLevel
    {
        info = 1,
        success = 2,
        warning = 3,
        danger = 4,
    }



    public class EventLogLevelStyle
    {
        public EventLogLevel eventLogLevel { get; init; }
        public ButtonRenderStyle buttonRenderStyle { get; init; }
        public Color StyleColor { get; init; }
        public string ColorRGBString => $"RGB({StyleColor.R}, {StyleColor.G}, {StyleColor.B}, 0.3)";
        public string ColorHTMLString => $"#{StyleColor.R:X2}{StyleColor.G:X2}{StyleColor.B:X2}";
        public EventLogLevelStyle(EventLogLevel eventLogLevel, ButtonRenderStyle buttonRenderStyle, Color color)
        {
            this.eventLogLevel = eventLogLevel;
            this.buttonRenderStyle = buttonRenderStyle;
            this.StyleColor = color;
        }
    }

    public class SerilogEventLogLevelStyle
    {
        public LogEventLevel logEventLevel { get; init; }
        public Color StyleColor { get; init; }
        public string ColorRGBString => $"RGB({StyleColor.R}, {StyleColor.G}, {StyleColor.B}, 0.3)";
        public string ColorHTMLString => $"#{StyleColor.R:X2}{StyleColor.G:X2}{StyleColor.B:X2}";
        public SerilogEventLogLevelStyle(LogEventLevel logEventLevel, Color color)
        {
            this.logEventLevel = logEventLevel;
            this.StyleColor = color;
        }
    }

    public class LogLevelAsIntEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var logLevelAsInt = MapLogLevelToInt(logEvent.Level);
            logEvent.AddPropertyIfAbsent(new LogEventProperty("Severity", new ScalarValue(logLevelAsInt)));

        }
        int MapLogLevelToInt(LogEventLevel level) => level switch
        {
            LogEventLevel.Verbose => 0,
            LogEventLevel.Debug => 1,
            LogEventLevel.Information => 2,
            LogEventLevel.Warning => 3,
            LogEventLevel.Error => 4,
            LogEventLevel.Fatal => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }


    public class BuildInRecipeWrapperClass : WrapperClass
    {
        public BuildInRecipeWrapperClass(DataType dataType, string propertyName)
        {
            index = (int)dataType;
            displayName = propertyName;
        }
    }


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
