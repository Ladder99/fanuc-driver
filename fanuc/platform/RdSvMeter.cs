namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic RdSvMeter(short data_num = 8)
        {
            short data_num_out = data_num;
            Focas1.ODBSVLOAD loadmeter = new Focas1.ODBSVLOAD();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdsvmeter(_handle, ref data_num_out, loadmeter);
            });

            // each path
            // loadmeter.svloadX.data / Math.Pow(10, loadmeter.svloadX.dec)

            return new
            {
                method = "cnc_rdsvmeter",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdsvmeter",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdsvmeter = new {data_num}},
                response = new {cnc_rdsvmeter = new {data_num = data_num_out, loadmeter}}
            };
        }
    }
}