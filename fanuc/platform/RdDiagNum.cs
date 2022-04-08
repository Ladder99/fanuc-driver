
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdDiagNumAsync()
        {
            return await Task.FromResult(RdDiagNum());
        }
        
        public dynamic RdDiagNum()
        {
            Focas.ODBDIAGNUM diagnum = new Focas.ODBDIAGNUM();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rddiagnum(_handle, diagnum);
            });

            var nr = new
            {
                method = "cnc_rddiagnum",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/misc/cnc_rddiagnum",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rddiagnum = new { }},
                response = new {cnc_rddiagnum = new {diagnum}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}