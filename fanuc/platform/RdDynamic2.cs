using System.Threading.Tasks;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdDynamic2Async(short axis = 1, short length = 44, int ODBDY2_type = 2)
        {
            return Task.FromResult(RdDynamic2(axis, length, ODBDY2_type));
        }
        
        public dynamic RdDynamic2(short axis = 1, short length = 44, int ODBDY2_type = 2)
        {
            dynamic rddynamic = new object();

            switch (ODBDY2_type)
            {
                case 1:
                    rddynamic = new Focas1.ODBDY2_1();
                    break;
                case 2:
                    rddynamic = new Focas1.ODBDY2_2();
                    break;
            }

            //length = (short) Marshal.SizeOf(rddynamic);

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rddynamic2(_handle, axis, length, rddynamic);
            });

            return new
            {
                method = "cnc_rddynamic2",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_rddynamic2",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rddynamic2 = new {axis, length}},
                response = new {cnc_rddynamic2 = new {rddynamic}}
            };
        }
    }
}