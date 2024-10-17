using System.ComponentModel.DataAnnotations;
using DT = TMWeb.Data.DataType;

namespace TMWeb.EFModels
{
    public partial class StaticRecipe : IValidatableObject
    {
        public StaticRecipe()
        {

        }

        public StaticRecipe(Guid configId)
        {
            ConfigId = configId;
            Id = Guid.NewGuid();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataType == 1)
            {
                bool res = bool.TryParse(ValueString, out var b);
                if (!res)
                {
                    yield return new ValidationResult($"Type {((DataType)DataType)} and {ValueString} not match", new List<string> { "DataType", "ValueString" });
                }
            }
            else if (DataType == 2)
            {
                bool res = ushort.TryParse(ValueString, out var u);
                if (!res)
                {
                    yield return new ValidationResult($"Type code {((DataType)DataType)} and {ValueString} not match", new List<string> { "DataType", "ValueString" });
                }
            }
            else if (DataType == 3)
            {
                bool res = float.TryParse(ValueString, out var f);
                if (!res)
                {
                    yield return new ValidationResult($"Type code {((DataType)DataType)} and {ValueString} not match", new List<string> { "DataType", "ValueString" });
                }
            }
            else if (DataType == 4)
            {
            }
            else
            {
                yield return new ValidationResult($"Type code {DataType} not support yet", new List<string> { "DataType", "ValueString" });
            }
        }

        public override Tuple<bool, Object?> GetValue(Workorder wo)
        {
            switch ((DT)this.DataType)
            {
                case DT.Bool:
                    bool b_res;
                    bool canParseBool = bool.TryParse(ValueString, out b_res);
                    return canParseBool?new(true, b_res) : new(false, null);
                case DT.Ushort:
                    ushort u_res;
                    bool canParseUshort = ushort.TryParse(ValueString, out u_res);
                    return canParseUshort ? new(true, u_res) : new(false, null);
                case DT.Float:
                    float f_res;
                    bool canParseFloat = float.TryParse(ValueString, out f_res);
                    return canParseFloat ? new(true, f_res) : new(false, null);
                case DT.String:
                    return !string.IsNullOrEmpty(ValueString) ? new(true, ValueString) : new(false, null);
                default:
                    return new(false, null);
            }
        }

        public override Tuple<bool, string> GetValueString(Workorder wo)
        {
            return !string.IsNullOrEmpty(ValueString) ? new(true, ValueString) : new(false, string.Empty);
        }
    }
}
