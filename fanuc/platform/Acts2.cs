namespace fanuc
{
    public partial class Platform
    {
        public dynamic Acts2(short sp_no = -1)
        {
            Focas1.ODBACT2 actualspindle = new Focas1.ODBACT2();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_acts2(_handle, sp_no, actualspindle);
            });

            return new
            {
                method = "cnc_acts2",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/position/cnc_acts2",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_acts2 = new {sp_no}},
                response = new {cnc_acts2 = new {actualspindle}}
            };
        }
    }
}