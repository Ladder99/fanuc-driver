namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> CNCIdAsync()
    {
        return await Task.FromResult(CNCId());
    }

    public dynamic CNCId()
    {
        var cncid = new uint[4];

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdcncid(_handle, cncid); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdcncid",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/misc/cnc_rdcncid",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdcncid = new { }},
            response = new {cnc_rdcncid = new {cncid}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}