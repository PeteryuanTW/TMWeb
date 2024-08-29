namespace TMWeb.Data
{
    public enum ModbusTCPAction
    {
        ReadCoils = 1,
        ReadDiscreteInputs = 2,
        ReadHoldingRegisters = 3,
        ReadInputRegisters = 4,
        WriteSingleCoil = 5,
        WriteSingleRegister = 6,
        WriteMultipleCoils = 15,
        WriteMultipleRegisters = 16,
    }


    public enum Status
    {
        Uninit,
        Init,
        Idel,
        Running,
        Stop,
        Error,
    }


    public enum DataMode
	{
		View,
		Edit,
		Delete,
	}
}
