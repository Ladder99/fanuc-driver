namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> UpEnd3Async()
    {
        return await Task.FromResult(UpEndt3());
    }

    public dynamic UpEndt3()
    {
        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_upend3(_handle); });

        var nr = new
        {
            @null = false,
            method = "cnc_upend3",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/program/cnc_upend3",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_upend3 = new {}},
            response = new {cnc_upend3 = new { }}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}