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
			}
		}
		public override void RemoveItemDetail()
		{
			if (HasItem)
			{
				itemDetail = null;
			}
		}
		public override void AddTaskDetail(TaskDetail taskDetail)
		{
			if (!HasTask)
			{
				this.taskDetail = taskDetail;
			}
		}

		public override void RemoveTaskDetail()
		{
			if (!HasTask)
			{
				taskDetail = null;
			}
		}
	}
}
