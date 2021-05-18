using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdExecPtAsync()
        {
            return Task.FromResult(RdExecPt());
        }
        
        public dynamic RdExecPt()
        {
            Focas1.PRGPNT pact = new Focas1.PRGPNT();
            Focas1.PRGPNT pnext = new Focas1.PRGPNT();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdexecpt(_handle, pact, pnext);
            });

            return new
            {
                method = "cnc_rdexecpt",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdexecpt",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdexecpt = new { }},
                response = new {cnc_rdexecpt = new {pact, pnext}}
            };
        }
    }
}