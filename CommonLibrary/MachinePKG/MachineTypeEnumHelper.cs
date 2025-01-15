using CommonLibrary.Data;
using DevExpress.Blazor;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG
{
    public static class MachineTypeEnumHelper
    {
        #region connection type
        public static IEnumerable<ConnectionTypeWrapperClass> GetConnectTypesWrapperClass()
        {
            return Enum.GetValues(typeof(ConnectType)).OfType<ConnectType>()
                .Select(x => new ConnectionTypeWrapperClass(x));
        }
        #endregion
        #region status
        public static IEnumerable<StatusWrapperClass> GetStatusWrapperClass()
        {
            return Enum.GetValues(typeof(Status)).OfType<Status>()
                .Select(x => new StatusWrapperClass(x));
        }

        public static IEnumerable<StatusStyle> StatusStyles =>
            new List<StatusStyle>
            {
                new (Status.Init, ButtonRenderStyle.Secondary, Color.FromArgb(143, 143, 143)),
                new (Status.TryConnecting, ButtonRenderStyle.Info, Color.FromArgb(91, 91, 174)),
                new (Status.Disconnect, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),
                new (Status.Idle, ButtonRenderStyle.Info, Color.FromArgb(130, 192, 192)),
                new (Status.Running, ButtonRenderStyle.Success, Color.FromArgb(1, 178, 104)),
                new (Status.Pause, ButtonRenderStyle.Warning, Color.FromArgb(235, 192, 0)),
                new (Status.Stop, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),
                new (Status.Error, ButtonRenderStyle.Danger, Color.FromArgb(204, 0, 0)),

                //new (Status.PDown, ButtonRenderStyle.Info, Color.FromArgb(130, 192, 192)),
                //new (Status.Offline, ButtonRenderStyle.Secondary, Color.FromArgb(143, 143, 143)),
            };

        public static StatusStyle? GetStatusStyle(int statusCode)
        {
            var target = StatusStyles.FirstOrDefault(x => (int)x.status == statusCode);
            return target;
        }

        public static void SetPointColor(ChartSeriesPointCustomizationSettings pointSettings)
        {
            int statusCode = (int)pointSettings.Point.Argument;
            var targetStyle = MachineTypeEnumHelper.GetStatusStyle(statusCode);
            if (targetStyle is not null)
            {
                pointSettings.PointAppearance.Color = targetStyle.StyleColor;
            }
            else
            {
                pointSettings.PointAppearance.Color = Color.Black;
            }
        }

        #endregion

        #region data type
        public static IEnumerable<DataTypeWrapperClass> GetDataTypesWrapperClass()
        {
            return Enum.GetValues(typeof(DataType)).OfType<DataType>()
                .Select(x => new DataTypeWrapperClass(x));
        }
        public static Type GetTypeByCode(int code)
        {
            var target = GetDataTypesWrapperClass().FirstOrDefault(x => x.Index == code);
            return target is null ? typeof(Object) : target.csType;
        }

        public static Dictionary<DataType, Type> TypeDict = new()
        {
            { DataType.Bool, typeof(bool) },
            { DataType.Ushort, typeof(ushort) },
            { DataType.Float, typeof(float) },
            { DataType.String, typeof(string) },
            //{ DataType.ArrayOfBool, typeof(bool[]) },
            //{ DataType.ArrayOfUshort, typeof(ushort[]) },
            //{ DataType.ArrayOfFloat, typeof(float[]) },
            //{ DataType.ArrayOfString, typeof(string[]) },

        };
        public static bool TypeMatch(int? code, Type? type)
        {
            if (code is null || type is null)
            {
                return false;
            }
            DataType dt = (DataType)code;
            if (TypeDict.ContainsKey(dt))
            {
                if (TypeDict[dt] == type)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool TypeMatch(DataType dt, Type type)
        {
            if (TypeDict.ContainsKey(dt))
            {
                if (TypeDict[dt] == type)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
        #region special tag type
        public static IEnumerable<SpecialTagTypeWrapperClass> GetSpecialTagTypesWrapperClass()
        {
            return Enum.GetValues(typeof(SpecialTagType)).OfType<SpecialTagType>()
                .Select(x => new SpecialTagTypeWrapperClass(x));
        }
        #endregion
        #region tag parameter
        private static List<TagParameter> TagParameterDict = new()
        {
            //modbus tcp
            new TagParameter( ConnectType.ModbusTCP, "Bool1", "Input/Output" ),
            new TagParameter( ConnectType.ModbusTCP, "Bool2", "String Reverse" ),
            
            new TagParameter( ConnectType.ModbusTCP, "Int1", "Station No" ),
            new TagParameter( ConnectType.ModbusTCP, "Int2", "Start Index" ),
            new TagParameter( ConnectType.ModbusTCP, "Int3", "Offset" ),

            //tm robot
            new TagParameter( ConnectType.TMRobot, "Bool1", "Input/Output" ),
            new TagParameter( ConnectType.TMRobot, "Bool2", "String Reverse" ),

            new TagParameter( ConnectType.TMRobot, "Int1", "Station No" ),
            new TagParameter( ConnectType.TMRobot, "Int2", "Start Index" ),
            new TagParameter( ConnectType.TMRobot, "Int3", "Offset"),

            //conveyor
            new TagParameter( ConnectType.ConveyorMachine, "Bool1", "Input/Output" ),
            new TagParameter( ConnectType.ConveyorMachine, "Bool2", "String Reverse" ),

            new TagParameter( ConnectType.ConveyorMachine, "Int1", "Station No" ),
            new TagParameter( ConnectType.ConveyorMachine, "Int2", "Start Index" ),
            new TagParameter( ConnectType.ConveyorMachine, "Int3", "Offset" ),

            //Wrapping
            new TagParameter( ConnectType.WrappingMachine, "Bool1", "Input/Output" ),
            new TagParameter( ConnectType.WrappingMachine, "Bool2", "String Reverse" ),

            new TagParameter( ConnectType.WrappingMachine, "Int1", "Station No" ),
            new TagParameter( ConnectType.WrappingMachine, "Int2", "Start Index" ),
            new TagParameter( ConnectType.WrappingMachine, "Int3", "Offset" ),


            //Web api
            new TagParameter( ConnectType.WebAPI, "String1", "Get Controller" ),
            new TagParameter( ConnectType.WebAPI, "String2", "Post COntroller" ),

            //rfid
            new TagParameter( ConnectType.RegalscanRFID, "String1", "Get Controller" ),
            new TagParameter( ConnectType.RegalscanRFID, "String2", "Post COntroller" ),
        };

        public static string GetTagParameterMeaning(ConnectType connectType, string varName)
        {
            var target = TagParameterDict.FirstOrDefault(x => x.connectType == connectType && x.variableName == varName);
            return target is null ? "Not Defined" : target.parameterName;
        }
        #endregion
    }

    #region connection type
    public class ConnectionTypeWrapperClass : WrapperClass
    {
        public ConnectionTypeWrapperClass(ConnectType type)
        {
            Type = type;
            index = (int)Type;
            displayName = Type.ToString();
        }
        public ConnectType Type { get; init; }
    }
    public enum ConnectType
    {
        ModbusTCP = 0,
        TMRobot = 1,
        ModbusTCPother = 2,
        WebAPI = 10,
        ConveyorMachine = 20,
        WrappingMachine = 21,
        RobotOther = 22,
        RegalscanRFID = 78,
    }
    #endregion
    
    #region status
    public class StatusWrapperClass : WrapperClass
    {
        public StatusWrapperClass(Status status)
        {
            Status = status;
            index = (int)status;
            displayName = status.ToString();
        }

        public Status Status { get; init; }
    }


    public enum Status
    {
        Init,
        TryConnecting,
        Disconnect,
        
        Idle,
        Running,
        Pause,
        Stop,
        Error,

        //PDown,
        //Offline,
    }

    public class StatusStyle
    {
        public Status status { get; init; }
        public ButtonRenderStyle buttonRenderStyle { get; init; }
        public Color StyleColor { get; init; }
        public string ColorRGBString => $"RGB({StyleColor.R}, {StyleColor.G}, {StyleColor.B})";
        public string ColorHTMLString => $"#{StyleColor.R:X2}{StyleColor.G:X2}{StyleColor.B:X2}";
        public StatusStyle(Status status, ButtonRenderStyle buttonRenderStyle, Color color)
        {
            this.status = status;
            this.buttonRenderStyle = buttonRenderStyle;
            this.StyleColor = color;
        }
    }

    #endregion

    #region status type
    public enum MachineStatusRecordType
    {
        InputStatus = 0,
        CustomStatus = 1,
    }
    #endregion

    #region data type
    public class DataTypeWrapperClass : WrapperClass
    {
        public DataTypeWrapperClass(DataType dataType)
        {
            Type = dataType;
            index = (int)Type;
            displayName = Type.ToString();
        }

        public DataType Type { get; init; }
        public Type csType => MachineTypeEnumHelper.TypeDict[Type];
    }
    public enum DataType
    {
        Bool = 1,
        Ushort = 2,
        Float = 3,
        String = 4,
        //ArrayOfBool = 11,
        //ArrayOfUshort = 22,
        //ArrayOfFloat = 33,
        //ArrayOfString = 44,
    }
    #endregion

    #region editmode
    public enum DataEditMode
    {
        Insert,
        Update,
        Delete,
    }
    #endregion

    #region special tag type

    public class SpecialTagTypeWrapperClass : WrapperClass
    {
        public SpecialTagTypeWrapperClass(SpecialTagType specialTagType)
        {
            index = (int)specialTagType;
            displayName = specialTagType.ToString();
        }
    }
    public enum SpecialTagType
    {
        General,
        CustomStatus,
        DetailCode,
    }
    #endregion

    #region tag parameter

    public class TagParameter
    {
        public TagParameter(ConnectType connectType, string variableName, string parameterName)
        {
            this.connectType = connectType;
            this.variableName = variableName;
            this.parameterName = parameterName;
        }
        public ConnectType connectType {  get; init; }
        public string variableName { get; init; } = null!;
        public string parameterName { get; init; } = null!;
    }

    #endregion
}
