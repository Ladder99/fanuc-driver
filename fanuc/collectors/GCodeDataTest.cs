using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class GCodeDataTest : FanucMultiStrategyCollector
{
    private int _currentBc;
    private readonly Dictionary<int, string> _program = new();

    public GCodeDataTest(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
    }

    public override async Task InitPathsAsync()
    {
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        var blkcount = await Strategy.Platform.RdBlkCountAsync();
        var exeprg = await Strategy.Platform.RdExecProgAsync(1024);

        if (blkcount.success && exeprg.success)
        {
            var bc = blkcount.response.cnc_rdblkcount.prog_bc;
            var bn = exeprg.response.cnc_rdexecprog.blknum;
            var prg = MoreEnumerable.SkipLast(new string(exeprg.response.cnc_rdexecprog.data).Split('\n'), 1);

            if (_currentBc != bc) Console.WriteLine($">>>{bc}<<<");

            _currentBc = bc;

            var offset = 0;
            foreach (var line in prg)
            {
                int bcOfLine = bc + offset;
                if (!_program.ContainsKey(bcOfLine))
                {
                    _program.Add(bcOfLine, line);
                    Console.WriteLine($"bc:{bcOfLine} | {line}");
                }

                offset += 1;
            }
        }
    }
}