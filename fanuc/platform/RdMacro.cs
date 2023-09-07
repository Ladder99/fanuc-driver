namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdMacroAsync(short number = 1, short length = 10)
    {
        return await Task.FromResult(RdMacro(number, length));
    }

    public dynamic RdMacro(short number = 1, short length = 10)
    {
        var macro = new Focas.ODBM();

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_rdmacro(_handle, number, length, macro);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_rdmacro",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/ncdata/cnc_rdmacro",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new { cnc_rdmacro = new { number, length } },
            response = new { cnc_rdmacro = new { macro } },
            bag = new Dictionary<dynamic, dynamic>()
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}