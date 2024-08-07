using TMWeb.EFModels;

namespace TMWeb.Data
{
	public class ModbusTCPMachine : Machine
	{
		public ModbusTCPMachine(Machine machine)//: base(machine)
		{
			Id = machine.Id;
			Name = machine.Name;
			Ip = machine.Ip;
			Port = machine.Port;
			ConnectionType = machine.ConnectionType;
			Enabled = machine.Enabled;
			TagCategoryId = machine.TagCategoryId;

			TagCategory = new TagCategory
			{
				Id = machine.TagCategory.Id,
				Name = machine.TagCategory.Name,
				ConnectionType = machine.ConnectionType,
				
				Tags = machine.TagCategory.Tags,
			};
		}
	}
}
