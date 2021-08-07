using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSpLoadAsync(short sp_no = 1)
        {
            return await Task.FromResult(RdSpLoad(sp_no));
        }
        
        public dynamic RdSpLoad(short sp_no = 1)
        {
            Focas.ODBSPN serial_spindle = new Focas.ODBSPN();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdspload(_handle, sp_no, serial_spindle);
            });

            var nr = new
            {
                method = "cnc_rdspload",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/Position/cnc_rdspload",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdspload = new {sp_no}},
                response = new {cnc_rdspload = new {serial_spindle}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}