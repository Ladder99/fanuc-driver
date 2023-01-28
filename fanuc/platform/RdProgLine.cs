namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdProgLineAsync(int prog_no = -1, uint line_no = 0, uint line_len = 1,
        uint data_len = 128)
    {
        return await Task.FromResult(RdProgLine(prog_no, line_no, line_len, data_len));
    }

    public dynamic RdProgLine(int prog_no = -1, uint line_no = 0, uint line_len = 1, uint data_len = 128)
    {
        var prog_data = new char[data_len];
        var line_len_in = line_len;
        var data_len_in = data_len;

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_rdprogline(_handle, prog_no, line_no, prog_data, ref line_len,
                ref data_len);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_rdprogline",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/program/cnc_rdprogline",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdprogline = new {prog_no, line_no, line_len_in, data_len_in}},
            response = new {cnc_rdprogline = new {prog_data, line_len, data_len}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}