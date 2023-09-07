namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdPmcRngGByteAsync(ushort number)
    {
        return await Task.FromResult(RdPmcRng(0, 0, number, number, 8 + 1, 0));
    }

    public async Task<dynamic> RdPmcRngYByteAsync(ushort number)
    {
        return await Task.FromResult(RdPmcRng(2, 0, number, number, 8 + 1, 0));
    }

    public async Task<dynamic> RdPmcRngAsync(short adr_type, short data_type, ushort s_number, ushort e_number,
        ushort length, int IODBPMC_type)
    {
        return await Task.FromResult(RdPmcRng(adr_type, data_type, s_number, e_number, length, IODBPMC_type));
    }

    public dynamic RdPmcRng(short adr_type, short data_type, ushort s_number, ushort e_number, ushort length,
        int IODBPMC_type)
    {
        dynamic buf = new object();

        switch (IODBPMC_type)
        {
            case 0:
                buf = new Focas.IODBPMC0();
                break;
            case 1:
                buf = new Focas.IODBPMC1();
                break;
            case 2:
                buf = new Focas.IODBPMC2();
                break;
        }

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.pmc_rdpmcrng(_handle, adr_type, data_type, s_number, e_number, length,
                buf);
        });

        var nr = new
        {
            @null = false,
            method = "pmc_rdpmcrng",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/pmc/pmc_rdpmcrng",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {pmc_rdpmcrng = new {adr_type, data_type, s_number, e_number, length, IODBPMC_type}},
            response = new {pmc_rdpmcrng = new {buf, IODBPMC_type = buf.GetType().Name}},
            bag = new Dictionary<dynamic, dynamic>()
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}