using CommonLibrary.MachinePKG.EFModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.MachineData
{
    public class WrappingMachine : ModbusTCPMachine
    {
        public WrappingMachine(Machine machine) : base(machine)
        {

        }

        protected override async Task UpdateStatus()
        {
            var statusList = await master.ReadHoldingRegistersAsync((byte)1, (ushort)6000, (ushort)1);
            int statusCode = statusList[0];
            switch (statusCode)
            {
                case 1:
                    Running();
                    break;
                case 2:
                    Error(string.Empty);
                    break;
                case 0:
                case 3:
                    Idle();
                    break;
                default:
                    Error(string.Empty);
                    break;
            }
        }
    }
}
