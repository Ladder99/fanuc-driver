
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> SvdtRdDataAsync(int length)
        {
            return await Task.FromResult(SvdtRdData(length));
        }
        
        public dynamic SvdtRdData(int length)
        {
            short stat = 0;
            int length_out = length;
            object data = new object();
            
            NativeDispatchReturn ndr = _nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_svdtrddata(_handle, out stat, ref length_out, data);
            });

            var nr = new
            {
                @null = false,
                method = "cnc_svdtrddata",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{_docBasePath}/servo/cnc_svdtrddata",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_svdtrddata = new {length}},
                response = new {cnc_svdtrddata = new {stat, length_out, data}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}