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
        protected override async Task UpdateTag(Tag tag)
        {
            int station = Int32.Parse(tag.Param1);
            int startIndex = Int32.Parse(tag.Param3);
            int offset = Int32.Parse(tag.Param4);
            switch (tag.DataType)
            {
                //bool
                case 1:
                    bool res_bool = false;
                    if (tag.Param2 == "in")
                    {
                        res_bool = (await master.ReadInputsAsync((byte)station, (byte)startIndex, (byte)offset)).FirstOrDefault();

                    }
                    else if (tag.Param2 == "out")
                    {
                        res_bool = (await master.ReadCoilsAsync((byte)station, (byte)startIndex, (byte)offset)).FirstOrDefault();
                    }
                    tag.SetValue(res_bool);
                    break;
                //ushort
                case 2:
                    ushort res_ushort = 0;
                    if (tag.Param2 == "in")
                    {
                        res_ushort = (await master.ReadInputRegistersAsync((byte)station, (byte)startIndex, (byte)offset)).FirstOrDefault();

                    }
                    else if (tag.Param2 == "out")
                    {
                        res_ushort = (await master.ReadHoldingRegistersAsync((byte)station, (byte)startIndex, (byte)offset)).FirstOrDefault();
                    }
                    tag.SetValue(res_ushort);
                    break;
                case 3:
                    break;
                case 11:
                    List<bool> res_boolList = Enumerable.Repeat(false, offset).ToList();
                    if (tag.Param2 == "in")
                    {
                        res_boolList = (await master.ReadInputsAsync((byte)station, (byte)startIndex, (byte)offset)).ToList();

                    }
                    else if (tag.Param2 == "out")
                    {
                        res_boolList = (await master.ReadCoilsAsync((byte)station, (byte)startIndex, (byte)offset)).ToList();
                    }
                    tag.SetValue(res_boolList);
                    break;
                case 22:
                    List<ushort> res_ushortList = Enumerable.Repeat((ushort)0, offset).ToList();
                    if (tag.Param2 == "in")
                    {
                        res_ushortList = (await master.ReadInputRegistersAsync((byte)station, (byte)startIndex, (byte)offset)).ToList();

                    }
                    else if (tag.Param2 == "out")
                    {
                        res_ushortList = (await master.ReadHoldingRegistersAsync((byte)station, (byte)startIndex, (byte)offset)).ToList();
                    }
                    tag.SetValue(res_ushortList);
                    break;
                case 33:
                    break;
                default:
                    break;
            }
        }


    }
}
