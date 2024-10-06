namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> UpStart3Async(short type, int start_number, int end_number)
    {
        return await Task.FromResult(UpStart3(type, start_number, end_number));
    }

    public dynamic UpStart3(short type, int start_number, int end_number)
    {
        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_upstart3(_handle, type, start_number, end_number); });

        var nr = new
        {
            @null = false,
            method = "cnc_upstart3",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/program/cnc_upstart3",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_upstart3 = new {type, start_number, end_number}},
            response = new {cnc_upstart3 = new { }}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}