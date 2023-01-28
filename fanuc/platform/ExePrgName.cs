namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> ExePrgNameAsync()
    {
        return await Task.FromResult(ExePrgName());
    }

    public dynamic ExePrgName()
    {
        var exeprg = new Focas.ODBEXEPRG();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_exeprgname(_handle, exeprg); });

        var nr = new
        {
            @null = false,
            method = "cnc_exeprgname",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/program/cnc_exeprgname",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_exeprgname = new { }},
            response = new {cnc_exeprgname = new {exeprg}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}