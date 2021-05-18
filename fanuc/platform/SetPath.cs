namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic SetPath(short path_no)
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_setpath(_handle, path_no);
            });

            return new
            {
                method = "cnc_setpath",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_setpath",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_setpath = new {path_no}},
                response = new {cnc_setpath = new { }}
            };
        }
    }
}