using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> Acts2Async(short sp_no = -1)
        {
            return await Task.FromResult(Acts2(sp_no));
        }
        
        public dynamic Acts2(short sp_no = -1)
        {
            Focas.ODBACT2 actualspindle = new Focas.ODBACT2();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_acts2(_handle, sp_no, actualspindle);
            });

            var nr = new
            {
                method = "cnc_acts2",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_acts2",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_acts2 = new {sp_no}},
                response = new {cnc_acts2 = new {actualspindle}}
            };
            
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}