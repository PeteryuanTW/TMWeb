using Microsoft.EntityFrameworkCore;
using TMWeb.Data;
using TMWeb.EFModels;

namespace TMWeb.Services
{
    public class TMWebMachineService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private List<Machine> machines = new();
        public TMWebMachineService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            machines = InitAllMachinesFromDB();
        }

        private List<Machine> InitAllMachinesFromDB()
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TmwebContext>();
                var tmp =  dbContext.Machines.Include(x => x.TagCategory).ThenInclude(x => x.Tags).AsNoTracking().ToList();
                return tmp.Select(x => InitMachineToDerivesClass(x)).ToList();

			}
        }

        private Machine InitMachineToDerivesClass(Machine machine)
        {
            switch (machine.ConnectionType)
            {
                case 0:
                    return new ModbusTCPMachine(machine);
                default:
                    return machine;
			}
        }

		public Task<Machine?> GetMachineByID(Guid id)
		{
			return Task.FromResult(machines.FirstOrDefault(x => x.Id == id));
		}
		public Task<Machine?> GetMachineByName(string name)
        {
            return Task.FromResult(machines.FirstOrDefault(x => x.Name == name));
		}

	}
}
