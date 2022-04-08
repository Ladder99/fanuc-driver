
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> SetPathAsync(short path_no)
        {
            return await Task.FromResult(SetPath(path_no));
        }
        
        public dynamic SetPath(short path_no)
        {
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_setpath(_handle, path_no);
            });

            var nr = new
            {
                method = "cnc_setpath",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/misc/cnc_setpath",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_setpath = new {path_no}},
                response = new {cnc_setpath = new { }}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}