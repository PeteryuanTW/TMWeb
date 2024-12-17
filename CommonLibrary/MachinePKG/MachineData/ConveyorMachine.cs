using CommonLibrary.MachinePKG.EFModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.MachineData
{
    public class ConveyorMachine : ModbusTCPMachine
    {
        public ConveyorMachine(Machine machine) : base(machine)
        {
            //base(machine);
        }

        protected override async Task UpdateStatus()
        {
            var statusList = await master.ReadHoldingRegistersAsync((byte)1, (ushort)38, (ushort)1);
            int statusCode = statusList[0];
            switch (statusCode)
            {
                case 0:
                    Idle();
                    break;
                case 4:
                    Running();
                    break;
                case 7:
                    Error(string.Empty);
                    break;

                default:
                    Error(string.Empty);
                    break;
            }
        }
    }
}
