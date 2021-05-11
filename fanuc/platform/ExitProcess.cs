namespace fanuc
{
    public partial class Platform
    {
        public dynamic ExitProcess()
        {
#if ARMV7 || LINUX64 || LINUX32
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret)Focas1.cnc_exitprocess();
            });

            return new
            {
                method = "cnc_exitprocess",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new { cnc_exitprocess = new {  } },
                response = new { cnc_exitprocess = new { } }
            };
#else
            return new
            {
                method = "cnc_exitprocess",
                invocationMs = -1,
                doc = "",
                success = true,
                Focas1.EW_OK,
                request = new {cnc_exitprocess = new { }},
                response = new {cnc_exitprocess = new { }}
            };
#endif
        }
    }
}