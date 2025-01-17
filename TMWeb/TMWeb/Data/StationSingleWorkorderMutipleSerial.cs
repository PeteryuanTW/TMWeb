﻿using CommonLibrary.API.Message;
using TMWeb.EFModels;

namespace TMWeb.Data
{
	public class StationSingleWorkorderMutipleSerial : StationSingleWorkorder
	{
		private List<ItemDetail> itemDetails;
		private bool HasItem => itemDetails != null && itemDetails.Count() > 0;
		public List<ItemDetail> ItemDetails => itemDetails;

		private List<TaskDetail> taskDetailsInStation;
		public bool HasTask => taskDetailsInStation != null && taskDetailsInStation.Count()>0;
		public List<TaskDetail> TaskDetailsInStation => taskDetailsInStation;

		public StationSingleWorkorderMutipleSerial(Station station) : base(station)
		{
			itemDetails = new();
            taskDetailsInStation = new();
		}

        public override bool CheckCanReset()
        {
            return !HasItem && !HasTask;
        }

        public override bool CheckHasItem()
        {
            return HasItem;
        }

        public override Task<RequestResult> ResetStation()
        {
            if (CheckCanReset())
            {
                try
                {
                    ClearWorkorder();
                    InitStation();
                    return Task.FromResult(new RequestResult(2, $"Reset station {Name} success"));
                }
                catch (Exception e)
                {
                    return Task.FromResult(new RequestResult(4, $"Reset station {Name} fail({e.Message})"));

                }

            }
            else
            {
                return Task.FromResult(new RequestResult(4, $"Clear data in station before reset"));

            }
        }

        public override void AddItemDetail(ItemDetail itemDetail)
		{
			if (!itemDetails.Exists(x => x.Id == itemDetail.Id))
			{
				itemDetails.Add(itemDetail);
				UIUpdate();
			}
		}

		public override ItemDetail? RemoveItemDetail()
		{
			if (HasItem)
			{
				ItemDetail? target = itemDetails.OrderBy(x => x.StartTime).FirstOrDefault();
				if (target != null)
				{
					ItemDetail tmp = target;
                    itemDetails.Remove(target);
                    UIUpdate();
                    return tmp;
				}
			}
			return null;
		}
        public ItemDetail? RemoveItemDetail(ItemDetail itemDetail)
        {
            if (HasItem)
            {
                ItemDetail? target = itemDetails.FirstOrDefault(x=>x.Id == itemDetail.Id);
                if (target != null)
                {
                    ItemDetail tmp = target;
                    itemDetails.Remove(target);
                    UIUpdate();
                    return tmp;
                }
            }
            return null;
        }



        public override void AddTaskDetail(TaskDetail taskDetail)
		{
			if (itemDetails.Exists(x => x.Id == taskDetail.ItemId))
			{
                taskDetailsInStation.Add(taskDetail);
                UIUpdate();
            }
		}

		public override TaskDetail? RemoveTaskDetail()
		{
			if (HasTask)
			{
				TaskDetail? target = taskDetailsInStation.OrderBy(x => x.StartTime).FirstOrDefault();
				if (target != null)
				{
                    taskDetailsInStation.Remove(target);
					if (!taskDetailsInStation.Exists(x => x.ItemId == target.ItemId))
					{
						TaskDetail tmp = target;
                        UIUpdate();
                        return tmp;
					}
				}
			}
			return null;
		}
        public TaskDetail? RemoveTaskDetail(string serialNo)
        {
            if (HasTask)
            {
                TaskDetail? target = taskDetailsInStation.FirstOrDefault(x=>x.SerialNo == serialNo);
                if (target != null)
                {
                    taskDetailsInStation.Remove(target);
                    if (!taskDetailsInStation.Exists(x => x.ItemId == target.ItemId))
                    {
                        TaskDetail tmp = target;
                        UIUpdate();
                        return tmp;
                    }
                }
            }
            return null;
        }
    }
}
