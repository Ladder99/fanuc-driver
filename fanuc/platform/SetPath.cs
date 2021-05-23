using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> SetPathAsync(short path_no)
        {
            return await Task.FromResult(SetPath(path_no));
        }
        
        public dynamic SetPath(short path_no)
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_setpath(_handle, path_no);
            });

            var nr = new
            {
                method = "cnc_setpath",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_setpath",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_setpath = new {path_no}},
                response = new {cnc_setpath = new { }}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}