using TMWeb.EFModels;

namespace TMWeb.Data
{
	public class StationSingleWorkorderMutipleSerial : StationSingleWorkorder
	{
		private List<ItemDetail> itemDetails;
		public bool HasItem => itemDetails != null && itemDetails.Count() > 0;
		public List<ItemDetail> ItemDetails => itemDetails;

		private List<TaskDetail> taskDetails;

		

		public bool HasTask => taskDetails != null;
		public List<TaskDetail> TaskDetails => taskDetails;

		public StationSingleWorkorderMutipleSerial(Station station) : base(station)
		{
			itemDetails = new();
			taskDetails = new();
		}

		public override void AddItemDetail(ItemDetail itemDetail)
		{
			if (!itemDetails.Exists(x => x.Id == itemDetail.Id))
			{
				itemDetails.Add(itemDetail);
			}
		}

		public override void RemoveItemDetail()
		{
			if (HasItem)
			{
				ItemDetail? target = itemDetails.OrderBy(x => x.StartTime).FirstOrDefault();
				if (target != null)
				{
					itemDetails.Remove(target);
				}
			}
		}

		private void RemoveItemDetailByTaskDetail(TaskDetail taskDetail)
		{
			if (HasItem)
			{
				ItemDetail? target = itemDetails.FirstOrDefault(x => x.Id == taskDetail.ItemId);
				if (target != null)
				{
					itemDetails.Remove(target);
				}
			}
		}

		public override void AddTaskDetail(TaskDetail taskDetail)
		{
			if (itemDetails.Exists(x => x.Id == taskDetail.ItemId))
			{
				TaskDetails.Add(taskDetail);
			}
		}

		public override void RemoveTaskDetail()
		{
			if (HasTask)
			{
				TaskDetail? target = taskDetails.OrderBy(x => x.StartTime).FirstOrDefault();
				if (target != null)
				{
					taskDetails.Remove(target);
					if (!taskDetails.Exists(x => x.ItemId == target.ItemId))
					{
						RemoveItemDetailByTaskDetail(target);
					}
				}
			}
		}
	}
}
