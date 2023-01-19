
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdBlkCountAsync()
        {
            return await Task.FromResult(RdBlkCount());
        }
        
        public dynamic RdBlkCount()
        {
            int prog_bc = 0;

            NativeDispatchReturn ndr = _nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdblkcount(_handle, out prog_bc);
            });

            var nr = new
            {
                @null = false,
                method = "cnc_rdblkcount",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{_docBasePath}/program/cnc_rdblkcount",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdblkcount = new { }},
                response = new {cnc_rdblkcount = new {prog_bc}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}