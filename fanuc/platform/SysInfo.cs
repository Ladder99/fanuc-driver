
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> SysInfoAsync()
        {
            return await Task.FromResult(SysInfo());
        }
        
        public dynamic SysInfo()
        {
            Focas.ODBSYS sysinfo = new Focas.ODBSYS();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_sysinfo(_handle, sysinfo);
            });

            var nr = new
            {
                method = "cnc_sysinfo",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/misc/cnc_sysinfo",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_sysinfo = new { }},
                response = new {cnc_sysinfo = new {sysinfo}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}