using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdProgDir3Async(int top_prog = 1)
        {
            return await Task.FromResult(RdProgDir3(top_prog: top_prog));
        }
        
        public dynamic RdProgDir3(short type = 2, int top_prog = 1, short num_prog = 1)
        {
            Focas.PRGDIR3 buf = new Focas.PRGDIR3();
            int top_prog_in = top_prog;
            short num_prog_in = num_prog;

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdprogdir3(_handle, type, ref top_prog, ref num_prog, buf);
            });

            var nr = new
            {
                method = "cnc_rdprogdir3",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/program/cnc_rdprogdir3",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdprogdir3 = new {type, top_prog_in, num_prog_in}},
                response = new {cnc_rdprogdir3 = new {top_prog, num_prog, buf}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}