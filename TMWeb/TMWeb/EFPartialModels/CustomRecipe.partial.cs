using DevExpress.Utils.Design;
using DT = TMWeb.Data.DataType;

namespace TMWeb.EFModels
{
    public partial class CustomRecipe
    {
        public CustomRecipe() { }
        public CustomRecipe(Guid configId)
        {
            ConfigId = configId;
            Id = Guid.NewGuid();
        }

        public bool SourceCatSelected => TargetRecordCatID != null;



        public override Tuple<bool, Object?> GetValue(Workorder wo)
        {
            var resStr = GetValueString(wo);
            if (!resStr.Item1)
            {
                return new(false, null);
            }
            switch ((DT)this.DataType)
            {
                case DT.Bool:
                    bool b_res;
                    bool canParseBool = bool.TryParse(resStr.Item2, out b_res);
                    return canParseBool ? new(true, b_res) : new(false, null);
                case DT.Ushort:
                    ushort u_res;
                    bool canParseUshort = ushort.TryParse(resStr.Item2, out u_res);
                    return canParseUshort ? new(true, u_res) : new(false, null);
                case DT.Float:
                    float f_res;
                    bool canParseFloat = float.TryParse(resStr.Item2, out f_res);
                    return canParseFloat ? new(true, f_res) : new(false, null);
                case DT.String:
                    return !string.IsNullOrEmpty(resStr.Item2) ? new(true, resStr.Item2) : new(false, null);
                default:
                    return new(false, null);
            }
        }

        public override Tuple<bool, string> GetValueString(Workorder wo)
        {
            if (wo.WorkorderRecordIncluded && SourceCatSelected)
            {
                bool catCorrect = wo.WorkorderRecordCategory.Id == TargetRecordCatID;
                if (catCorrect)
                {
                    var customRecord = wo.WorkorderRecordDetails.FirstOrDefault(x => x.RecordContentId == TargetRecordID);
                    if (customRecord is not null)
                    {
                        return new(true, customRecord.Value);
                    }
                }
            }
            return new(false, string.Empty);
        }
    }
}
