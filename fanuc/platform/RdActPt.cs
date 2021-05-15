namespace fanuc
{
    public partial class Platform
    {
        public dynamic RdActPt()
        {
            int prog_no = 0;
            int blk_no = 0;

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdactpt(_handle, out prog_no, out blk_no);
            });

            return new
            {
                method = "cnc_rdactpt",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/program/cnc_rdactpt",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdactpt = new { }},
                response = new {cnc_rdactpt = new {prog_no, blk_no}}
            };
        }
    }
}