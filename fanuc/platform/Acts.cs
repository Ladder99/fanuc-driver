namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> ActsAsync()
    {
        return await Task.FromResult(Acts());
    }

    public dynamic Acts()
    {
        var actualfeed = new Focas.ODBACT();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_acts(_handle, actualfeed); });

        var nr = new
        {
            @null = false,
            method = "cnc_acts",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/position/cnc_acts",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_acts = new { }},
            response = new {cnc_acts = new {actualfeed}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}