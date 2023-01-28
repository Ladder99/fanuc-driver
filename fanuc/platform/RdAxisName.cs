namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdAxisNameAsync(short data_num = 8)
    {
        return await Task.FromResult(RdAxisName(data_num));
    }

    public dynamic RdAxisName(short data_num = 8)
    {
        var data_num_out = data_num;
        var axisname = new Focas.ODBAXISNAME();

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_rdaxisname(_handle, ref data_num_out, axisname);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_rdaxisname",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/position/cnc_rdaxisname",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdaxisname = new {data_num}},
            response = new {cnc_rdaxisname = new {data_num = data_num_out, axisname}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}