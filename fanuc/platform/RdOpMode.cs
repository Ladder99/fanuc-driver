using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdOpModeAsync()
        {
            return await Task.FromResult(RdOpMode());
        }
        
        public dynamic RdOpMode()
        {
            short mode = 0; // array?

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdopmode(_handle, out mode);
            });

            var nr = new
            {
                method = "cnc_rdopmode",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/motor/cnc_rdopmode",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdopmode = new { }},
                response = new {cnc_rdopmode = new {mode}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}