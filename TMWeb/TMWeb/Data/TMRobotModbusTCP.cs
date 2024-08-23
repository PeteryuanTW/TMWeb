using TMWeb.EFModels;

namespace TMWeb.Data
{
    public class TMRobotModbusTCP : ModbusTCPMachine
    {
        public TMRobotModbusTCP(Machine machine) : base(machine)
        {
        }


        public override async Task UpdateTag(Tag tag)
        {
            if (status != Status.Error || status != Status.Stop)
            {
                try
                {
                    //MachineStatus newStatus = MachineStatus.Init;
                    //bool isError = (await master?.ReadInputsAsync(1, 7201, 1)).FirstOrDefault();
                    //bool isRunProject = (await master?.ReadInputsAsync(1, 7202, 1)).FirstOrDefault();

                    //newStatus = GetRobotState(isError, isRunProject);
                    //Console.WriteLine(newStatus.ToString());
                    //if (newStatus != Status)
                    //{
                    //    switch (newStatus)
                    //    {
                    //        case MachineStatus.Init:
                    //            break;
                    //        case MachineStatus.Disconnect:
                    //            Disconnect(string.Empty);
                    //            break;
                    //        case MachineStatus.Idel:
                    //            Idel();
                    //            break;
                    //        case MachineStatus.Running:
                    //            Running();
                    //            break;
                    //        case MachineStatus.Error:
                    //            var errorCode = await master.ReadInputRegistersAsync(1, 7320, 2);
                    //            Error("Error");
                    //            break;
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }
                
            }
            
        }

        private Status GetRobotState(bool isError, bool isRunProject)
        {
            if (isError)
            {
                return Status.Error;
            }
            else
            {
                if (isRunProject)
                {
                    return Status.Running;
                }
                else
                {
                    return Status.Idel; ;
                }
            }
        }
    }
}
