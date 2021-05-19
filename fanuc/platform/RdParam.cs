using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdParamAsync(short number, short axis, short length, int IODBPSD_type)
        {
            return await Task.FromResult(RdParam(number, axis, length, IODBPSD_type));
        }
        
        public dynamic RdParam(short number, short axis, short length, int IODBPSD_type)
        {
            dynamic param = new object();

            switch (IODBPSD_type)
            {
                case 1:
                    param = new Focas1.IODBPSD_1();
                    break;
                case 2:
                    param = new Focas1.IODBPSD_2();
                    break;
                case 3:
                    param = new Focas1.IODBPSD_3();
                    break;
                case 4:
                    param = new Focas1.IODBPSD_4();
                    break;
            }

            Focas1.ODBALMMSG almmsg = new Focas1.ODBALMMSG();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdparam(_handle, number, axis, length, param);
            });

            return new
            {
                method = "cnc_rdparam",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/ncdata/cnc_rdparam",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdparam = new {number, axis, length, IODBPSD_type}},
                response = new {cnc_rdparam = new {param, IODBPSD_type = param.GetType().Name}}
            };
        }
    }
}