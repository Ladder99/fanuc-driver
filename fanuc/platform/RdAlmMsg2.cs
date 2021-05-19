using System.Collections.Generic;
using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdAlmMsg2Async(short type = 0, short num = 10)
        {
            return await Task.FromResult(RdAlmMsg2(type, num));
        }
        
        public dynamic RdAlmMsg2(short type = 0, short num = 10)
        {
            short num_out = num;
            Focas1.ODBALMMSG2 almmsg = new Focas1.ODBALMMSG2();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdalmmsg2(_handle, type, ref num_out, almmsg);
            });

            return new
            {
                method = "cnc_rdalmmsg2",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdalmmsg2",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdalmmsg2 = new {type, num}},
                response = new {cnc_rdalmmsg2 = new {num = num_out, almmsg}}
            };
        }
        
        public async Task<dynamic> RdAlmMsg2AllAsync(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for (short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, await RdAlmMsg2Async(type, countRead));
            }

            return new
            {
                method = "cnc_rdalmmsg2_ALL",
                request = new { cnc_rdalmmsg2_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg2_ALL = alms }
            };
        }
        
        public dynamic RdAlmMsg2All(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for (short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, RdAlmMsg2(type, countRead));
            }

            return new
            {
                method = "cnc_rdalmmsg2_ALL",
                request = new { cnc_rdalmmsg2_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg2_ALL = alms }
            };
        }
    }
}