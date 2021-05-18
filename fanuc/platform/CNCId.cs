namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic CNCId()
        {
            uint[] cncid = new uint[4];

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdcncid(_handle, cncid);
            });

            return new
            {
                method = "cnc_rdcncid",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdcncid",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdcncid = new { }},
                response = new {cnc_rdcncid = new {cncid}}
            };
        }
    }
}