namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdSpMeterAsync(short type = 0, short data_num = 4)
    {
        return await Task.FromResult(RdSpMeter(type, data_num));
    }

    public dynamic RdSpMeter(short type = 0, short data_num = 4)
    {
        var data_num_out = data_num;
        var loadmeter = new Focas.ODBSPLOAD();

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_rdspmeter(_handle, type, ref data_num_out, loadmeter);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_rdspmeter",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/position/cnc_rdspmeter",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdspmeter = new {type, data_num}},
            response = new {cnc_rdspmeter = new {data_num = data_num_out, loadmeter}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}