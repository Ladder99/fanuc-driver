namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> Upload3Async(int length = 256)
    {
        return await Task.FromResult(Upload3(length));
    }

    public dynamic Upload3(int length)
    {
        var data = new Focas.ODBUP3();
        
        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_upload3(_handle, ref length, data); });

        var nr = new
        {
            @null = false,
            method = "cnc_upload3",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/program/cnc_upload3",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_upload3 = new {length}},
            response = new {cnc_upload3 = new {data}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}