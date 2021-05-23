using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> DisconnectAsync()
        {
            return await Task.FromResult(Disconnect());
        }
        
        public dynamic Disconnect()
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_freelibhndl(_handle);
            });

            var nr = new
            {
                method = "cnc_freelibhndl",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/handle/cnc_freelibhndl",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_freelibhndl = new { }},
                response = new {cnc_freelibhndl = new { }}
            };

            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");
            
            return nr;
        }
    }
}