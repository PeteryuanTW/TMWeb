using System.Reflection;
using TMWeb.Data.CustomAttribute;

namespace TMWeb.EFModels
{
    public enum WorkorderStatus
    {
        Init = 0,
        Running = 1,
        Stop = 2,
    }
    public partial class Workorder
    {
        public Workorder() { }

        public Workorder(Guid id)
        {
            this.Id = id;
            Status = (int)WorkorderStatus.Init;
            CreateTime = DateTime.Now;
        }

        public bool HasProcess => Process != null;
        public bool HasRecipe => RecipeCategoryId != null;
        public bool RecipeIncluded => RecipeCategory != null;
        public bool HasWorkorderRecord => WorkorderRecordCategoryId != null;
        public bool WorkorderRecordIncluded => WorkorderRecordCategory != null;
        public bool HasItemRecord => ItemRecordsCategoryId != null;
        public bool ItemRecordIncluded => ItemRecordsCategory != null;
        public bool HasTaskRecord => TaskRecordCategoryId != null;
        public bool TaskRecordIncluded => TaskRecordCategory != null;

        public Dictionary<string, Object> GetVariableList()
        {
            var res = new Dictionary<string, Object>();
            if (HasProcess)
            {
                res.Add("Process", Process.Name);
            }

            //porp in wo
            var props = GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetCustomAttributes(typeof(PublicPropertyAttribute), false).Length > 0)
                {
                    if (prop.GetValue(this) is not null)
                    {
                        res.Add(prop.Name, prop.GetValue(this));
                    }
                }
            }
            foreach (var customProp in WorkorderRecordDetails)
            {
                res.Add(customProp.RecordContent.RecordName, customProp.Value);
            }


            return res;
        }

    }
}
