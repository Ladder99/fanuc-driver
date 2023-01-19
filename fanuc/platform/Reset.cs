
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ResetAsync()
        {
            return await Task.FromResult(Reset());
        }
        
        public dynamic Reset()
        {
            NativeDispatchReturn ndr = _nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_reset(_handle);
            });

            var nr = new
            {
                @null = false,
                method = "cnc_reset",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{_docBasePath}/misc/cnc_reset",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_reset = new { }},
                response = new {cnc_reset = new { }}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}