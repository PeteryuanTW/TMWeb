using TMWeb.EFModels;

namespace TMWeb.Data
{
    public class StationSingleWorkorderNoSerial : StationSingleWorkorder
    {

        private ItemDetail? itemDetail;
        public bool HasItem => itemDetail != null;
        public ItemDetail? ItemDetail => itemDetail;

        private TaskDetail? taskDetail;
        public bool HasTask => taskDetail != null;
        public TaskDetail? TaskDetail => taskDetail;

        private int wip = 0;

        public int WIP => wip;

        public StationSingleWorkorderNoSerial(Station station) : base(station)
        {
            
        }

        public override void AddItemDetail(ItemDetail itemDetail)
        {
            if (!HasItem)
            {
                this.itemDetail = itemDetail;
            }
        }

        public override void AddTaskDetail(TaskDetail taskDetail)
        {
            if (!HasTask)
            {
                this.taskDetail = taskDetail;
            }
        }

        public override ItemDetail? RemoveItemDetail()
        {
            if (HasItem)
            {
                ItemDetail tmp = itemDetail;
                itemDetail = null;
                return tmp;
            }
            return null;
        }

        public override TaskDetail? RemoveTaskDetail()
        {
            if (HasTask)
            {
                TaskDetail tmp = taskDetail;
                taskDetail = null;
                return tmp;
            }
            return null;
        }

        public void StationInWithAmount(int amount)
        {
            if (HasItem && HasTask)
            {
                wip += amount;
                UIUpdate();
            }
        }
        public void StationOutWithAmount(int ok, int ng)
        {
            if (HasItem && HasTask)
            {
                taskDetail.Okamount += ok;
                taskDetail.Ngamount += ng;
                taskDetail.FinishedTime = DateTime.Now;
                wip -= (ok + ng);
                UIUpdate();
            }
        }
    }
}
