
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSpdlNameAsync(short data_num = 4)
        {
            return await Task.FromResult(RdSpdlName(data_num));
        }
        
        public dynamic RdSpdlName(short data_num = 4)
        {
            short data_num_out = data_num;
            Focas.ODBSPDLNAME spdlname = new Focas.ODBSPDLNAME();

            NativeDispatchReturn ndr = _nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdspdlname(_handle, ref data_num_out, spdlname);
            });

            var nr = new
            {
                @null = false,
                method = "cnc_rdspdlname",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{_docBasePath}/position/cnc_rdspdlname",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdspdlname = new {data_num}},
                response = new {cnc_rdspdlname = new {data_num = data_num_out, spdlname}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}