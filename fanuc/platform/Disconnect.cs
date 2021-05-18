namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic Disconnect()
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_freelibhndl(_handle);
            });

            return new
            {
                method = "cnc_freelibhndl",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/handle/cnc_freelibhndl",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_freelibhndl = new { }},
                response = new {cnc_freelibhndl = new { }}
            };
        }
    }
}