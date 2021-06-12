using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdBlkCountAsync()
        {
            return await Task.FromResult(RdBlkCount());
        }
        
        public dynamic RdBlkCount()
        {
            int prog_bc = 0;

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdblkcount(_handle, out prog_bc);
            });

            var nr = new
            {
                method = "cnc_rdblkcount",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdblkcount",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdblkcount = new { }},
                response = new {cnc_rdblkcount = new {prog_bc}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}