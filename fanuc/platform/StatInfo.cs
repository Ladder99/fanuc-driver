using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> StatInfoAsync()
        {
            return await Task.FromResult(StatInfo());
        }
        
        public dynamic StatInfo()
        {
            Focas.ODBST statinfo = new Focas.ODBST();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_statinfo(_handle, statinfo);
            });

            var nr = new
            {
                method = "cnc_statinfo",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_statinfo",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_statinfo = new { }},
                response = new {cnc_statinfo = new {statinfo}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}