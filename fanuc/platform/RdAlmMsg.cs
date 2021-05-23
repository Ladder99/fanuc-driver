using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdAlmMsgAsync(short type = 0, short num = 10)
        {
            return await Task.FromResult(RdAlmMsg(type, num));
        }
        
        public dynamic RdAlmMsg(short type = 0, short num = 10)
        {
            short num_out = num;
            Focas1.ODBALMMSG almmsg = new Focas1.ODBALMMSG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdalmmsg(_handle, type, ref num_out, almmsg);
            });

            var nr = new
            {
                method = "cnc_rdalmmsg",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdalmmsg",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdalmmsg = new {type, num}},
                response = new {cnc_rdalmmsg = new {num = num_out, almmsg}}
            };

            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
        
        public async Task<dynamic> RdAlmMsgAllAsync(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for(short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, await RdAlmMsgAsync(type, countRead));
            }

            var nr = new
            {
                method = "cnc_rdalmmsg_ALL",
                request = new { cnc_rdalmmsg_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg_ALL = alms }
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
        
        public dynamic RdAlmMsgAll(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for(short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, RdAlmMsg(type, countRead));
            }

            var nr = new
            {
                method = "cnc_rdalmmsg_ALL",
                request = new { cnc_rdalmmsg_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg_ALL = alms }
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}