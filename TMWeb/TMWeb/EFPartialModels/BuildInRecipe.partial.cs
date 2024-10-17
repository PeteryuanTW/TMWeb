using System.ComponentModel.DataAnnotations;
using TMWeb.Data;
using DT = TMWeb.Data.DataType;

namespace TMWeb.EFModels
{
    public partial class BuildInRecipe: IValidatableObject
    {
        public BuildInRecipe() { }

        public BuildInRecipe(Guid configId)
        {
            ConfigId = configId;
            Id = Guid.NewGuid();
        }

        public override Tuple<bool, Object?> GetValue(Workorder wo)
        {
            var prop = wo.GetType().GetProperty(TargetProp);
            if (prop is null)
            {
                return new(false, null);
            }
            var val = prop.GetValue(wo);
            return val is not null ? new(true, val) : new(false, null);
        }

        public override Tuple<bool, string> GetValueString(Workorder wo)
        {
            var prop = wo.GetType().GetProperty(TargetProp);
            if (prop is null)
            {
                return new(false, string.Empty);
            }
            var val =prop.GetValue(wo);
            return val is not null ? new(true, val.ToString()): new(false, string.Empty);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var propertyType = typeof(Workorder).GetProperty(TargetProp).PropertyType;
            if (propertyType is null)
            {
                yield return new ValidationResult($"Property {TargetProp} not found", new List<string> { "TargetProp" });
            }
            else
            {
                if (!TypeEnumHelper.TypeMatch(DataType, propertyType))
                {
                    yield return new ValidationResult($"Property {TargetProp} type {propertyType.Name} and tag type {(DT)DataType} not match", new List<string> { "TargetProp", "DataType" });
                }
            }
        }
    }
}
