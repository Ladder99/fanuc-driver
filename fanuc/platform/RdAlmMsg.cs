using System.Collections.Generic;
using System.Linq;
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
            Focas.ODBALMMSG almmsg = new Focas.ODBALMMSG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdalmmsg(_handle, type, ref num_out, almmsg);
            });

            var nr = new
            {
                method = "cnc_rdalmmsg",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/Misc/cnc_rdalmmsg",
                success = ndr.RC == Focas.EW_OK,
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
                invocationMs = (long) alms.Sum(x => (int)x.Value.invocationMs),
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/misc/cnc_rdalmmsg",
                success = true, // TODO: aggregate
                rc = Focas.EW_OK, // TODO: aggregate
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
                invocationMs = (long) alms.Sum(x => (int)x.Value.invocationMs),
                doc = "https://ladder99.github.io/fanuc-driver/focas/SpecE/misc/cnc_rdalmmsg",
                success = true, // TODO: aggregate
                rc = Focas.EW_OK, // TODO: aggregate
                request = new { cnc_rdalmmsg_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg_ALL = alms }
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}