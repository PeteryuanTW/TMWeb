using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace TMWeb.Extension
{
    public static class SerilogExtension
    {
        public static void LogWithSeverity(LogEventLevel level, string msg)
        {
            StackTrace stackTrace = new StackTrace(true);
            StackFrame frame = stackTrace.GetFrame(1);

            string callerName = Path.GetFileName(frame.GetFileName());
            string methodName = frame.GetMethod().Name;
            int rowNo = frame.GetFileLineNumber();
            int columnNo = frame.GetFileColumnNumber();

            var contextSetting = Log.ForContext("Caller", callerName)
                .ForContext("Method", methodName)
                .ForContext("Row", rowNo)
                .ForContext("Col", columnNo);
            contextSetting.Write(level, msg);


        }
    }
}
