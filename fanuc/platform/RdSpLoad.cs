using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSpLoadAsync(short sp_no = 0)
        {
            return await Task.FromResult(RdSpLoad(sp_no));
        }
        
        public dynamic RdSpLoad(short sp_no = 0)
        {
            Focas1.ODBSPN serial_spindle = new Focas1.ODBSPN();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdspload(_handle, sp_no, serial_spindle);
            });

            var nr = new
            {
                method = "cnc_rdspload",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdspload",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdspload = new {sp_no}},
                response = new {cnc_rdspload = new {serial_spindle}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}