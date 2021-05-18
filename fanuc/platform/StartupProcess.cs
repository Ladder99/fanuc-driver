using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> StartupProcessAsync(short level = 0, string filename = "~/focas2.log")
        {
            return Task.FromResult(StartupProcess(level, filename));
        }
        
        public dynamic StartupProcess(short level = 0, string filename = "~/focas2.log")
        {
#if ARMV7 || LINUX64 || LINUX32
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret)Focas1.cnc_startupprocess(level, filename);
            });

            return new
            {
                method = "cnc_startupprocess",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new { cnc_startupprocess = new { level, filename } },
                response = new { cnc_startupprocess = new { } }
            };
#else
            return new
            {
                method = "cnc_startupprocess",
                invocationMs = -1,
                doc = "",
                success = true,
                Focas1.EW_OK,
                request = new {cnc_startupprocess = new {level, filename}},
                response = new {cnc_startupprocess = new { }}
            };
#endif
        }
    }
}