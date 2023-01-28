namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdOpModeAsync()
    {
        return await Task.FromResult(RdOpMode());
    }

    public dynamic RdOpMode()
    {
        short mode = 0; // array?

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdopmode(_handle, out mode); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdopmode",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/Motor/cnc_rdopmode",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdopmode = new { }},
            response = new {cnc_rdopmode = new {mode}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}