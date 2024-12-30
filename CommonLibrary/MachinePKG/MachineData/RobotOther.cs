using CommonLibrary.MachinePKG.EFModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.MachineData
{
    public class RobotOther : ModbusTCPMachine
    {
        public RobotOther(Machine machine) : base(machine)
        {
            //base(machine);
        }
        protected override async Task UpdateStatus()
        {
            if (hasCategory)
            {
                var statusTag = TagCategory.Tags.FirstOrDefault(x => x.Name == "Status");
                if (statusTag is not null)
                {
                    if (statusTag.DataType != (int)DataType.Ushort)
                    {
                        Error("status tag datatype error");
                    }
                    else
                    {
                        switch ((ushort)statusTag.Value)
                        {
                            case 1:
                                Running();
                                break;
                            case 2:
                                Pause();
                                break;
                            case 3:
                                Stop();
                                break;
                            case 7:
                                Offline();
                                break;
                            case 9:
                                PDown();
                                break;
                            default:
                                Error($"status tag value error{(statusTag.ValueString)}");
                                break;
                        }
                    }

                }
                else
                {
                    Error("no tag name status");
                }
            }
            else
            {
                Error("no category setting");
            }
        }

        private void PDown()
        {
            if (status != Status.PDown)
            {
                status = Status.PDown;
                errorMsg = "plan down";
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }

        private void Offline()
        {
            if (status != Status.Offline)
            {
                status = Status.Offline;
                errorMsg = "Offline";
                lastStatusChangedTime = DateTime.Now;
                MachineStatechanged();
            }
        }
    }
}
