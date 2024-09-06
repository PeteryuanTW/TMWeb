using TMWeb.EFModels;

namespace TMWeb.Data
{
	public class StationSingleWorkorderSingleSerial : StationSingleWorkorder
	{
		private ItemDetail? itemDetail;
		public bool HasItem => itemDetail != null;
		public ItemDetail? ItemDetail => itemDetail;

		private TaskDetail? taskDetail;
		public bool HasTask => taskDetail != null;
		public TaskDetail? TaskDetail => taskDetail;


		public StationSingleWorkorderSingleSerial(Station station):base(station)
		{
			
		}

		public override void AddItemDetail(ItemDetail itemDetail)
		{
			if (!HasItem)
			{
				this.itemDetail = itemDetail;
                UIUpdate();
            }
		}
		public override ItemDetail? RemoveItemDetail()
		{
			if (HasItem)
			{
                ItemDetail tmp = itemDetail;
                itemDetail = null;
                UIUpdate();
                return tmp;
			}
			return null;
		}
		public override void AddTaskDetail(TaskDetail taskDetail)
		{
			if (!HasTask)
			{
				this.taskDetail = taskDetail;
                UIUpdate();
            }
		}

		public override TaskDetail? RemoveTaskDetail()
		{
			if (HasTask)
			{
                TaskDetail tmp = taskDetail;
                taskDetail = null;
                UIUpdate();
                return tmp;
			}
			return null;
		}
	}
}
