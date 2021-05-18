using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdOpMsgAsync(short type = 0, short length = 262)
        {
            return Task.FromResult(RdOpMsg(type, length));
        }
        
        public dynamic RdOpMsg(short type = 0, short length = 262)
        {
            Focas1.OPMSG opmsg = new Focas1.OPMSG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdopmsg(_handle, type, length, opmsg);
            });

            return new
            {
                method = "cnc_rdopmsg",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_rdopmsg",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdopmsg = new {type, length}},
                response = new {cnc_rdopmsg = new {opmsg}}
            };
        }
    }
}