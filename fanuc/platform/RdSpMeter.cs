namespace fanuc
{
    public partial class Platform
    {
        public dynamic RdSpMeter(short type = 0, short data_num = 4)
        {
            short data_num_out = data_num;
            Focas1.ODBSPLOAD loadmeter = new Focas1.ODBSPLOAD();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdspmeter(_handle, type, ref data_num_out, loadmeter);
            });

            return new
            {
                method = "cnc_rdspmeter",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdspmeter",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdspmeter = new {type, data_num}},
                response = new {cnc_rdspmeter = new {data_num = data_num_out, loadmeter}}
            };
        }
    }
}