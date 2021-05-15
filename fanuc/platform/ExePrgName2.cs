namespace fanuc
{
    public partial class Platform
    {
        public dynamic ExePrgName2()
        {
            char[] path_name = new char[256];

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_exeprgname2(_handle, ref path_name);
            });

            return new
            {
                method = "cnc_exeprgname2",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_exeprgname2",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_exeprgname2 = new { }},
                response = new {cnc_exeprgname2 = new {path_name}}
            };
        }
    }
}