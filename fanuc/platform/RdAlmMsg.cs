using System.Collections.Generic;

namespace fanuc
{
    public partial class Platform
    {
        public dynamic RdAlmMsg(short type = 0, short num = 10)
        {
            short num_out = num;
            Focas1.ODBALMMSG almmsg = new Focas1.ODBALMMSG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdalmmsg(_handle, type, ref num_out, almmsg);
            });

            return new
            {
                method = "cnc_rdalmmsg",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdalmmsg",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdalmmsg = new {type, num}},
                response = new {cnc_rdalmmsg = new {num = num_out, almmsg}}
            };
        }
        
        public dynamic RdAlmMsgAll(short count = 10, short maxType = 20)
        {
            var alms = new Dictionary<short, dynamic>();

            for(short type = 0; type <= maxType; type++)
            {
                short countRead = 10;
                alms.Add(type, RdAlmMsg(type, countRead));
            }

            return new
            {
                method = "cnc_rdalmmsg_ALL",
                request = new { cnc_rdalmmsg_ALL = new { minType = 0, maxType, count } },
                response = new { cnc_rdalmmsg_ALL = alms }
            };
        }
    }
}