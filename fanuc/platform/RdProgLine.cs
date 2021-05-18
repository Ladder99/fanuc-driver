namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic RdProgLine(int prog_no = -1, uint line_no = 0, uint line_len = 1, uint data_len = 128)
        {
            char[] prog_data = new char[data_len];
            uint line_len_in = line_len;
            uint data_len_in = data_len;

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdprogline(_handle, prog_no, line_no, prog_data, ref line_len, ref data_len);
            });

            return new
            {
                method = "cnc_rdprogline",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdprogline",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdprogline = new {prog_no, line_no, line_len_in, data_len_in}},
                response = new {cnc_rdprogline = new {prog_data, line_len, data_len}}
            };
        }
    }
}