using TMWeb.EFModels;
using CommonLibrary.MachinePKG.EFModel;
namespace TMWeb.Data
{
    public class RecipeCheckItem
    {
        public string MachineName { get; set; }

        public Tag Tag { get; set; }

        public string Val => Tag.ValueString;

        public RecipeCheckItem(Machine machine, Tag tag)
        {
            MachineName = machine.Name;
            Tag = tag;
        }
    }
}
