using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdParaInfoAsync(short s_number, ushort read_no = 1)
        {
            return await Task.FromResult(RdParaInfo(s_number, read_no));
        }
        
        public dynamic RdParaInfo(short s_number, ushort read_no = 1)
        {
            Focas.ODBPARAIF paraif = new Focas.ODBPARAIF();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdparainfo(_handle, s_number, read_no, paraif);
            });

            var nr = new
            {
                method = "cnc_rdparainfo",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/Ncdata/cnc_rdparainfo",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdparainfo = new {s_number, read_no }},
                response = new {cnc_rdparainfo = new {paraif }}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}