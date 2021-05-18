using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> ExePrgNameAsync()
        {
            return Task.FromResult(ExePrgName());
        }
        
        public dynamic ExePrgName()
        {
            Focas1.ODBEXEPRG exeprg = new Focas1.ODBEXEPRG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_exeprgname(_handle, exeprg);
            });

            return new
            {
                method = "cnc_exeprgname",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_exeprgname",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_exeprgname = new { }},
                response = new {cnc_exeprgname = new {exeprg}}
            };
        }
    }
}