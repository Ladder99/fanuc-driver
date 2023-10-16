using l99.driver.fanuc.strategies;
using System.Dynamic;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;


public class Pmc : FanucMultiStrategyCollector
{
    public Pmc(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
        if (!Configuration.ContainsKey("map")) 
            Configuration.Add("map", new Dictionary<object, object>());
    }

    public override async Task InitRootAsync()
    {
        await Strategy.Apply(typeof(veneers.Pmc), "pmc");
    }

    public override async Task CollectRootAsync()
    {
        foreach (var pmcEntry in Configuration["map"])
        {
            if (!new[] { "id", "address", "type" }.All(key => pmcEntry.ContainsKey(key))) continue;
            
            short adr_type = f_adr_type(pmcEntry["address"][0]);
            short data_type = 0;
            ushort s_number = 0;
            ushort e_number = s_number;
            ushort length = 9;
            int IODBPMC_type = 0;
            int bit = -1;

            switch (pmcEntry["type"])
            {
                case "bit":
                    s_number = ushort.Parse(pmcEntry["address"].Substring(1, pmcEntry["address"].Length - 3));
                    e_number = s_number;
                    bit = pmcEntry["address"][pmcEntry["address"].Length - 1] - '0';
                    break;
                case "byte":
                    break;
                case "word":
                    data_type = 1;
                    length = 10;
                    s_number = ushort.Parse(pmcEntry["address"].Substring(1));
                    e_number = (ushort)(s_number + 1);
                    IODBPMC_type = 1;
                    break;
                case "long": // is long 4 or 8 bytes?
                    data_type = 2;
                    length = 12;
                    s_number = ushort.Parse(pmcEntry["address"].Substring(1));
                    e_number = (ushort)(s_number + 3);
                    IODBPMC_type = 2;
                    break;
                case "float32":
                    data_type = 4;
                    length = 12;
                    s_number = ushort.Parse(pmcEntry["address"].Substring(1));
                    e_number = (ushort)(s_number + 3);
                    IODBPMC_type = 2;
                    break;
                case "float64":
                    data_type = 5;
                    length = 16;
                    s_number = ushort.Parse(pmcEntry["address"].Substring(1));
                    e_number = (ushort)(s_number + 7);
                    IODBPMC_type = 2;
                    break;
                default:
                    // unsupported
                    continue;
            }
            
            var nr = await Strategy.Platform.RdPmcRngAsync(adr_type, data_type, s_number, e_number, length, IODBPMC_type);
            dynamic value;

            switch (pmcEntry["type"])
            {
                case "bit":
                    value = (nr.response.pmc_rdpmcrng.buf.cdata[0] >> bit) &1;
                    break;
                case "byte":
                    value = nr.response.pmc_rdpmcrng.buf.cdata[0];
                    break;
                case "word":
                    value = nr.response.pmc_rdpmcrng.buf.idata[0];
                    break;
                case "long": 
                    value = nr.response.pmc_rdpmcrng.buf.ldata[0];
                    break;
                case "float32":
                    value = BitConverter.Int32BitsToSingle(nr.response.pmc_rdpmcrng.buf.ldata[0]);
                    break;
                case "float64":
                    value = BitConverter.Int64BitsToDouble(nr.response.pmc_rdpmcrng.buf.ldata[0]);
                    break;
                default:
                    // unsupported
                    continue;
            }

            nr.bag["id"] = pmcEntry["id"];
            nr.bag["address"] = pmcEntry["address"];
            nr.bag["type"] = pmcEntry["type"];
            nr.bag["value"] = value;
            
            await Strategy.SetNativeKeyed($"pmc_{pmcEntry["id"]}", nr);
        }

        await Strategy.Peel("pmc",
            Strategy.GetKeyedStartsWith("pmc_").ToArray(),
            new dynamic[]
            {
            });
    }

    private short f_adr_type(char adr_letter)
    {
        switch(adr_letter)
        {
            case 'G':
                return 0;
            case 'F':
                return 1;
            case 'Y':
                return 2;
            case 'X':
                return 3;
            case 'A':
                return 4;
            case 'R':
                return 5;
            case 'T':
                return 6;
            case 'K':
                return 7;
            case 'C':
                return 8;
            case 'D':
                return 9;
            case 'M':
                return 10;
            case 'N':
                return 11;
            case 'E':
                return 11;
            case 'Z':
                return 12;
            default:
                return 0;
        }
    }
}
