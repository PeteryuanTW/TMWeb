using DevExpress.Pdf.ContentGeneration.Interop;
using TMWeb.EFModels;

namespace TMWeb.Data
{
    public class TMRobotModbusTCP : ModbusTCPMachine
    {
        public TMRobotModbusTCP(Machine machine) : base(machine)
        {
            
        }

        protected override async Task UpdateStatus()
        {
            bool[] statusList = await master.ReadInputsAsync((byte)1, (ushort)7200, (ushort)14);
            if (statusList[8])
            {
                Stop();
            }
            else
            {
                if (statusList[1])
                {
                    Error("Error from robot");
                }
                else
                {
                    if (statusList[4])
                    {
                        Pause();
                    }
                    else
                    {
                        if (statusList[2])
                        {
                            Running();
                        }
                        else
                        {
                            Idel();
                        }
                    }
                }
            }
            //await base.UpdateStatus();
        }
    }
}
