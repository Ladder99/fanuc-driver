using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSeqNumAsync()
        {
            return await Task.FromResult(RdSeqNum());
        }
        
        public dynamic RdSeqNum()
        {
            Focas.ODBSEQ seqnum = new Focas.ODBSEQ();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdseqnum(_handle, seqnum);
            });

            var nr = new
            {
                method = "cnc_rdseqnum",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/program/cnc_rdseqnum",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdseqnum = new { }},
                response = new {cnc_rdseqnum = new {seqnum}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}