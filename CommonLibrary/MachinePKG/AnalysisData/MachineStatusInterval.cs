using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.AnalysisData
{
    public  class MachineStatusInterval
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public Status Status { get; set; }

        public TimeSpan Interval => End - Start;

        public DateTime MidTime => Start + Interval;

        public MachineStatusInterval(DateTime start, DateTime end, Status status)
        {
            Start = start;
            End = end;
            Status = status;
        }
    }
}
