
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ToolNumAsync()
        {
            return await Task.FromResult(ToolNum());
        }
        
        public dynamic ToolNum(short grp_num = 0, short tuse_num = 0)
        {
            Focas.ODBTLIFE4 toolnum = new Focas.ODBTLIFE4();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_toolnum(_handle, grp_num, tuse_num, toolnum);
            });

            var nr = new
            {
                method = "cnc_toolnum",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/toollife/cnc_toolnum",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_toolnum = new {grp_num, tuse_num}},
                response = new {cnc_toolnum = new {toolnum}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}