using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdParaNumAsync()
        {
            return await Task.FromResult(RdParaNum());
        }
        
        public dynamic RdParaNum()
        {
            Focas.ODBPARANUM paranum = new Focas.ODBPARANUM();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdparanum(_handle, paranum);
            });

            var nr = new
            {
                method = "cnc_rdparanum",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/Ncdata/cnc_rdparanum",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdparanum = new { }},
                response = new {cnc_rdparanum = new {paranum}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}