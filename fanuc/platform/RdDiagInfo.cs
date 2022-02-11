using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdDiagInfoAsync(short s_number, ushort read_no = 1)
        {
            return await Task.FromResult(RdDiagInfo(s_number, read_no));
        }
        
        public dynamic RdDiagInfo(short s_number, ushort read_no = 1)
        {
            Focas.ODBDIAGIF diagif = new Focas.ODBDIAGIF();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rddiaginfo(_handle, s_number, read_no, diagif);
            });

            var nr = new
            {
                method = "cnc_rddiaginfo",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/misc/cnc_rddiaginfo",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rddiaginfo = new {s_number, read_no }},
                response = new {cnc_rddiaginfo = new {diagif }}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}