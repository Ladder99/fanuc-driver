namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic RdAxisName(short data_num = 8)
        {
            short data_num_out = data_num;
            Focas1.ODBAXISNAME axisname = new Focas1.ODBAXISNAME();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdaxisname(_handle, ref data_num_out, axisname);
            });

            return new
            {
                method = "cnc_rdaxisname",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdaxisname",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdaxisname = new {data_num}},
                response = new {cnc_rdaxisname = new {data_num = data_num_out, axisname}}
            };
        }
    }
}