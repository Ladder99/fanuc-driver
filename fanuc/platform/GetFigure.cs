namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic GetFigure(short data_type = 0, short axis = 8)
        {
            short valid_fig = 0; short[] dec_fig_in = new short[axis]; short[] dec_fig_out = new short[axis];

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_getfigure(_handle, data_type, out valid_fig, dec_fig_in, dec_fig_out);
            });

            return new
            {
                method = "cnc_getfigure",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_getfigure",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_getfigure = new {data_type, axis}},
                response = new {cnc_getfigure = new {valid_fig, dec_fig_in, dec_fig_out}}
            };
        }
    }
}