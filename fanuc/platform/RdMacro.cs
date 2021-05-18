namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public dynamic RdMacro(short number = 1, short length = 10)
        {
            Focas1.ODBM macro = new Focas1.ODBM();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_rdmacro(_handle, number, length, macro);
            });

            return new
            {
                method = "cnc_rdmacro",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/ncdata/cnc_rdmacro",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnd_rdmacro = new {number, length}},
                response = new {cnd_rdmacro = new {macro}}
            };
        }
    }
}