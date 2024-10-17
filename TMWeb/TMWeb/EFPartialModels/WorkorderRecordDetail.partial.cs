using System.Collections;
using System.Text.RegularExpressions;
using TMWeb.Data;
using TMWeb.Data.TagHelper;

namespace TMWeb.EFModels
{
    public partial class WorkorderRecordDetail
    {
        public WorkorderRecordDetail() { }

        public WorkorderRecordDetail(Guid workorderID, WorkorderRecordContent workorderRecordContent)
        {
            WorkerderId = workorderID;
            RecordContentId = workorderRecordContent.Id;
            RecordContent = workorderRecordContent;
        }

        private bool includeContent => RecordContent != null;
        public void SetValueByString(string s)
        {
            if (includeContent)
            {
                try
                {
                    switch ((DataType)RecordContent.DataType)
                    {
                        case DataType.Bool:
                            bool b;
                            if (Boolean.TryParse(s, out b))
                            {
                                Value = s;
                            }
                            break;
                        case DataType.Ushort:
                            ushort u;
                            if (UInt16.TryParse(s, out u))
                            {
                                Value = s;
                            }
                            break;
                        case DataType.Float:
                            float f;
                            if (float.TryParse(s, out f))
                            {
                                Value = s;
                            }
                            break;
                        case DataType.String:
                            Value = s;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {

                }
            }
        }
    }
}
