using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdPrgNumAsync()
        {
            return await Task.FromResult(RdPrgNum());
        }
        
        public dynamic RdPrgNum()
        {
            Focas.ODBPRO prgnum = new Focas.ODBPRO();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdprgnum(_handle, prgnum);
            });

            var nr = new
            {
                method = "cnc_rdprgnum",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/Program/cnc_rdprgnum",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdprgnum = new { }},
                response = new {cnc_rdprgnum = new {prgnum}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}