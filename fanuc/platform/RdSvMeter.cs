using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSvMeterAsync(short data_num = 8)
        {
            return await Task.FromResult(RdSvMeter());
        }
        
        public dynamic RdSvMeter(short data_num = 8)
        {
            short data_num_out = data_num;
            Focas.ODBSVLOAD loadmeter = new Focas.ODBSVLOAD();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdsvmeter(_handle, ref data_num_out, loadmeter);
            });

            // each path
            // loadmeter.svloadX.data / Math.Pow(10, loadmeter.svloadX.dec)

            var nr = new
            {
                method = "cnc_rdsvmeter",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdsvmeter",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdsvmeter = new {data_num}},
                response = new {cnc_rdsvmeter = new {data_num = data_num_out, loadmeter}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}