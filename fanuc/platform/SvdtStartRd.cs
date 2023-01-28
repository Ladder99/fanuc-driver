namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> SvdtStartRdAsync(short axis)
    {
        return await Task.FromResult(SvdtStartRd(axis));
    }

    public dynamic SvdtStartRd(short axis)
    {
        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_svdtstartrd(_handle, axis); });

        var nr = new
        {
            @null = false,
            method = "cnc_svdtstartrd",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/servo/cnc_svdtstartrd",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_svdtstartrd = new {axis}},
            response = new {cnc_svdtstartrd = new { }}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}