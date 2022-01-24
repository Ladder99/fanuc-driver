using System;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ActsAsync()
        {
            return await Task.FromResult(Acts());
        }
        
        public dynamic Acts()
        {
            Focas.ODBACT actualfeed = new Focas.ODBACT();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_acts(_handle, actualfeed);
            });

            var nr = new
            {
                method = "cnc_acts",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/position/cnc_acts",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_acts = new { }},
                response = new {cnc_acts = new {actualfeed}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}