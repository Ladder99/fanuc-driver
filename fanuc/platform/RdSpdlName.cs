using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSpdlNameAsync(short data_num = 4)
        {
            return Task.FromResult(RdSpdlName(data_num));
        }
        
        public dynamic RdSpdlName(short data_num = 4)
        {
            short data_num_out = data_num;
            Focas1.ODBSPDLNAME spdlname = new Focas1.ODBSPDLNAME();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdspdlname(_handle, ref data_num_out, spdlname);
            });

            return new
            {
                method = "cnc_rdspdlname",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rdspdlname",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdspdlname = new {data_num}},
                response = new {cnc_rdspdlname = new {data_num = data_num_out, spdlname}}
            };
        }
    }
}