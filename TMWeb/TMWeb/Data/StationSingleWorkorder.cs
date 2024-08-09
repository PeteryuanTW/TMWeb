using TMWeb.EFModels;

namespace TMWeb.Data
{
	public abstract class StationSingleWorkorder : Station
	{
		private Workorder? workorder;
		public bool HasWorkorder => workorder != null;
		public Workorder? Workerder => workorder;

		public StationSingleWorkorder(Station station)
		{
			Id = station.Id;
			ProcessId = station.ProcessId;
			Name = station.Name;
			ProcessIndex = station.ProcessIndex;
			StationType = station.StationType;
			Process = station.Process;

        }

		public override bool SetWorkorder(Workorder wo)
		{
			if (!HasWorkorder && Status== StationState.Uninit)
			{
                workorder = wo;
                //UIUpdate();
                return true;
            }
			else
			{
				return false;
            }

        }
		public override bool ClearWorkorder()
		{
			if (!HasWorkorder)
			{
				return false;
			}
			else
			{
				workorder = null;
				//UIUpdate();
                return true;
			}
		}

		public override void Run()
		{
			if (HasWorkorder)
			{
				base.Run();
			}

		}

		public abstract void AddItemDetail(ItemDetail itemDetail);
		public abstract ItemDetail? RemoveItemDetail();
		public abstract void AddTaskDetail(TaskDetail taskDetail);
		public abstract TaskDetail? RemoveTaskDetail();
	}
}
