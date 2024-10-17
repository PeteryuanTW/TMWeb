using CommonLibrary.API.Message;
using System.Collections;
using TMWeb.Data;
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

        public Tag(Guid CategoryID)
        {
            Id = Guid.NewGuid();
            DataType = 1;
            CategoryId = CategoryID;
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
                    SetValue(0.0f);
                    break;
                case 4:
                    SetValue(string.Empty);
                    break;
                case 11:
                    SetValue(new List<bool> { });
                    break;
                case 22:
                    SetValue(new List<ushort> { });
                    break;
                case 33:
                    SetValue(new List<float> { });
                    break;
                case 44:
                    SetValue(new List<string> { });
                    break;
                default:
                    break;
            }
        }

        public RequestResult SetValue(Object obj)
        {
            lastUpdateTime = DateTime.Now;
            if (TypeEnumHelper.TypeMatch((int)DataType, obj.GetType()))
            {
                if (obj.GetType().IsArray)
                {
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(value, obj))
                    {
                        value = obj;
                        lastChangedTime = DateTime.Now;
                        return new(1, $"Update tag {Name} success");
                    }
                    else
                    {
                        return new(1, $"Tag {Name} not changed");
                    }
                }
                else
                {
                    if (!value.Equals(obj))
                    {
                        value = obj;
                        lastChangedTime = DateTime.Now;
                        return new(1, $"Update tag {Name} success");
                    }
                    else
                    {
                        return new(1, $"Tag {Name} not changed");
                    }
                }
            }
            else
            {
                return new(4, $"Update tag {Name} fail data type not match");
            }
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
