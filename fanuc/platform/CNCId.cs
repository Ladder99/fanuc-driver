using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> CNCIdAsync()
        {
            return await Task.FromResult(CNCId());
        }
        
        public dynamic CNCId()
        {
            uint[] cncid = new uint[4];

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdcncid(_handle, cncid);
            });

            var nr = new
            {
                method = "cnc_rdcncid",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdcncid",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdcncid = new { }},
                response = new {cnc_rdcncid = new {cncid}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}