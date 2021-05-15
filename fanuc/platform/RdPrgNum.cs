namespace fanuc
{
    public partial class Platform
    {
        public dynamic RdPrgNum()
        {
            Focas1.ODBPRO prgnum = new Focas1.ODBPRO();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdprgnum(_handle, prgnum);
            });

            return new
            {
                method = "cnc_rdprgnum",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdprgnum",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdprgnum = new { }},
                response = new {cnc_rdprgnum = new {prgnum}}
            };
        }
    }
}