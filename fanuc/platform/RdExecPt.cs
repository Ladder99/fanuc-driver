using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdExecPtAsync()
        {
            return await Task.FromResult(RdExecPt());
        }
        
        public dynamic RdExecPt()
        {
            Focas.PRGPNT pact = new Focas.PRGPNT();
            Focas.PRGPNT pnext = new Focas.PRGPNT();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdexecpt(_handle, pact, pnext);
            });

            var nr = new
            {
                method = "cnc_rdexecpt",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/Program/cnc_rdexecpt",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdexecpt = new { }},
                response = new {cnc_rdexecpt = new {pact, pnext}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}