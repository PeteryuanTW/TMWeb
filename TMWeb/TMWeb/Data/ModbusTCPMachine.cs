using System.Net.Sockets;
using TMWeb.EFModels;
using NModbus;
using System;

namespace TMWeb.Data
{
    public class ModbusTCPMachine : Machine
    {
        private TcpClient tcpClient;
        private IModbusFactory modbusFactory;
        public IModbusMaster? master;
        public ModbusTCPMachine(Machine machine)//: base(machine)
        {
            Id = machine.Id;
            ProcessId = machine.ProcessId;
            Name = machine.Name;
            Ip = machine.Ip;
            Port = machine.Port;
            ConnectionType = machine.ConnectionType;
            Enabled = machine.Enabled;
            TagCategoryId = machine.TagCategoryId;

            if (machine.TagCategory != null)
            {
                TagCategory = new TagCategory
                {
                    Id = machine.TagCategory.Id,
                    Name = machine.TagCategory.Name,
                    ConnectionType = machine.ConnectionType,

                    Tags = machine.TagCategory.Tags,
                };
            }

            tcpClient = new TcpClient();
            modbusFactory = new ModbusFactory();
        }

        public override async Task ConnectAsync()
        {
            try
            {
                await tcpClient.ConnectAsync(Ip, Port);
                master = modbusFactory.CreateMaster(tcpClient);
            }
            catch (Exception e)
            {
                Disconnect(e.Message);
            }
        }
    }
}
