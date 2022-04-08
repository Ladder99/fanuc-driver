
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ExePrgNameAsync()
        {
            return await Task.FromResult(ExePrgName());
        }
        
        public dynamic ExePrgName()
        {
            Focas.ODBEXEPRG exeprg = new Focas.ODBEXEPRG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_exeprgname(_handle, exeprg);
            });

            var nr= new
            {
                method = "cnc_exeprgname",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/program/cnc_exeprgname",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_exeprgname = new { }},
                response = new {cnc_exeprgname = new {exeprg}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}