namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> GetFigureAsync(short data_type = 0, short axis = 8)
    {
        return await Task.FromResult(GetFigure(data_type, axis));
    }

    public dynamic GetFigure(short data_type = 0, short axis = 8)
    {
        short valid_fig = 0;
        var dec_fig_in = new short[axis];
        var dec_fig_out = new short[axis];

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_getfigure(_handle, data_type, out valid_fig, dec_fig_in,
                dec_fig_out);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_getfigure",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/misc/cnc_getfigure",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_getfigure = new {data_type, axis}},
            response = new {cnc_getfigure = new {valid_fig, dec_fig_in, dec_fig_out}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}