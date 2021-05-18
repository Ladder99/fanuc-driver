namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic RdOpMode()
        {
            short mode = 0; // array?

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdopmode(_handle, out mode);
            });

            return new
            {
                method = "cnc_rdopmode",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/motor/cnc_rdopmode",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdopmode = new { }},
                response = new {cnc_rdopmode = new {mode}}
            };
        }
    }
}