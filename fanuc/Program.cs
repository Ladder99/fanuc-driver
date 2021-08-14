using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            dynamic config = await Bootstrap.Start(args);
            Machines machines = await Machines.CreateMachines(config);
            await machines.RunAsync();
            await Bootstrap.Stop();
        }
    }
}      