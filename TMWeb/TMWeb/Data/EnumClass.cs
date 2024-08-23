namespace TMWeb.Data
{
    //public enum StationState
    //{
    //    Uninit,
    //    Running,
    //    Pause,
    //    Error,
    //    Stop,
    //}

    //public enum MachineStatus
    //{
    //    Init,
    //    Disconnect,
    //    Running,
    //    Idel,
    //    Error,
    //}

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
