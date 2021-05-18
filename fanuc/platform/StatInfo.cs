namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic StatInfo()
        {
            Focas1.ODBST statinfo = new Focas1.ODBST();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_statinfo(_handle, statinfo);
            });

            return new
            {
                method = "cnc_statinfo",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_statinfo",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_statinfo = new { }},
                response = new {cnc_statinfo = new {statinfo}}
            };
        }
    }
}