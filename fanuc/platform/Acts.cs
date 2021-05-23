using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ActsAsync()
        {
            return await Task.FromResult(Acts());
        }
        
        public dynamic Acts()
        {
            Focas1.ODBACT actualfeed = new Focas1.ODBACT();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_acts(_handle, actualfeed);
            });

            var nr = new
            {
                method = "cnc_acts",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_acts",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_acts = new { }},
                response = new {cnc_acts = new {actualfeed}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}