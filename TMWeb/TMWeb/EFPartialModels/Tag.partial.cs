using System.Collections;
using TMWeb.Data.TagHelper;

namespace TMWeb.EFModels
{
    public partial class Tag
    {
        //param 1: station, param2: in/out put, param 3: start index param, param 4: offset
        public Tag()
        {
            //Init();
        }

        public void Init()
        {
            lastUpdateTime = DateTime.Now;
            lastChangedTime = DateTime.Now;
            InitVal();
        }

        private DateTime lastUpdateTime;
        public DateTime LastUpdateTime => lastUpdateTime;
        private DateTime lastChangedTime;
        public DateTime LastChangedTime => lastChangedTime;

        public Object Value => value;
        private Object value = new();
        public string ValueString => FormatingValueToString();

        private void InitVal()
        {
            switch (DataType)
            {
                case 1:
                    SetValue(false);
                    break;
                case 2:
                    SetValue((ushort)0);
                    break;
                case 3:
                    SetValue("");
                    break;
                case 11:
                    SetValue(new List<bool> { });
                    break;
                case 22:
                    SetValue(new List<ushort> { });
                    break;
                case 33:
                    SetValue(new List<string> { });
                    break;
                default:
                    break;
            }
        }

        public void SetValue(Object obj)
        {
            if (TagTypeHelper.TagTypeMatchCode(DataType, obj.GetType()))
            {
                if (obj.GetType().IsArray)
                {
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(value, obj))
                    {
                        value = obj;
                        lastChangedTime = DateTime.Now;
                    }
                    else
                    {

                    }
                }
                else
                {
                    if (!value.Equals(obj))
                    {
                        value = obj;
                        lastChangedTime = DateTime.Now;
                    }
                    else
                    {

                    }
                }
            }
            lastUpdateTime = DateTime.Now;
        }
        private string FormatingValueToString()
        {
            if (value == null)
                return string.Empty;
            if (value.GetType().IsArray)
            {
                return "[" + string.Join(",", value) + "]";
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
