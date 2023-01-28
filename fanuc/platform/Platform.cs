using System.Diagnostics;

namespace l99.driver.fanuc;

public partial class Platform
{
    private readonly string _docBasePath = "https://docs.ladder99.com/drivers/fanuc-driver/focas-api";

    private readonly Func<Func<Focas.focas_ret>, NativeDispatchReturn> _nativeDispatch = nativeCallWrapper =>
    {
        var rc = Focas.focas_ret.EW_OK;
        var sw = new Stopwatch();
        sw.Start();
        rc = nativeCallWrapper();
        sw.Stop();
        return new NativeDispatchReturn
        {
            RC = rc,
            ElapsedMilliseconds = sw.ElapsedMilliseconds
        };
    };

    private ushort _handle;
    private readonly ILogger _logger;

    private readonly FanucMachine _machine;

    public Platform(FanucMachine machine)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _machine = machine;
    }

    public ushort Handle => _handle;

    private struct NativeDispatchReturn
    {
        public Focas.focas_ret RC;
        public long ElapsedMilliseconds;
    }
}