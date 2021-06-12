using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ConnectAsync()
        {
            return await Task.FromResult(Connect());
        }
        
        public dynamic Connect()
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_allclibhndl3(
                    _machine.FocasEndpoint.IPAddress, 
                    _machine.FocasEndpoint.Port,
                    _machine.FocasEndpoint.ConnectionTimeout, 
                    out _handle);
            });

            var nr = new
            {
                method = "cnc_allclibhndl3",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/handle/cnc_allclibhndl3",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new
                {
                    cnc_allclibhndl3 = new
                        {
                            ipaddr = _machine.FocasEndpoint.IPAddress, 
                            port = _machine.FocasEndpoint.Port, 
                            timeout = _machine.FocasEndpoint.ConnectionTimeout
                        }
                },
                response = new {cnc_allclibhndl3 = new {FlibHndl = _handle}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}