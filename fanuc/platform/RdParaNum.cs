namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdParaNumAsync()
    {
        return await Task.FromResult(RdParaNum());
    }

    public dynamic RdParaNum()
    {
        var paranum = new Focas.ODBPARANUM();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdparanum(_handle, paranum); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdparanum",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/ncdata/cnc_rdparanum",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdparanum = new { }},
            response = new {cnc_rdparanum = new {paranum}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}