using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> GetPathAsync(short path_no = 0)
        {
            return await Task.FromResult(GetPath(path_no));
        }
        
        public dynamic GetPath(short path_no = 0)
        {
            short maxpath_no = 0, path_no_out = path_no;

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_getpath(_handle, out path_no_out, out maxpath_no);
            });

            return new
            {
                method = "cnc_getpath",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_getpath",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_getpath = new {path_no}},
                response = new {cnc_getpath = new {path_no = path_no_out, maxpath_no}}
            };
        }
    }
}