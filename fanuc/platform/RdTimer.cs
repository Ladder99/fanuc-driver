namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdTimerAsync(short type = 0)
    {
        return await Task.FromResult(RdTimer(type));
    }

    public dynamic RdTimer(short type = 0)
    {
        var time = new Focas.IODBTIME();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdtimer(_handle, type, time); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdtimer",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/misc/cnc_rdtimer",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdtimer = new {type}},
            response = new {cnc_rdtimer = new {time}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}