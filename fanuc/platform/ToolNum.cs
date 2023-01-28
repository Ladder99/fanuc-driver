namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> ToolNumAsync(short grp_num = 0, short tuse_num = 0)
    {
        return await Task.FromResult(ToolNum(grp_num, tuse_num));
    }

    public dynamic ToolNum(short grp_num = 0, short tuse_num = 0)
    {
        var toolnum = new Focas.ODBTLIFE4();

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_toolnum(_handle, grp_num, tuse_num, toolnum);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_toolnum",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/toollife/cnc_toolnum",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_toolnum = new {grp_num, tuse_num}},
            response = new {cnc_toolnum = new {toolnum}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}