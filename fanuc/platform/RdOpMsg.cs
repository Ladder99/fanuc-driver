namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdOpMsg1_15_15i_Async()
    {
        return await Task.FromResult(RdOpMsg(0, 6 + 130));
    }

    public async Task<dynamic> RdOpMsg2_15_15i_Async()
    {
        return await Task.FromResult(RdOpMsg(1, 6 + 130));
    }

    public async Task<dynamic> RdOpMsg3_15_15i_Async()
    {
        return await Task.FromResult(RdOpMsg(2, 6 + 130));
    }

    public async Task<dynamic> RdOpMsg4_15_15i_Async()
    {
        return await Task.FromResult(RdOpMsg(3, 6 + 130));
    }

    public async Task<dynamic> RdOpMsgMacro_15_15i_Async()
    {
        return await Task.FromResult(RdOpMsg(4, 6 + 28));
    }

    public async Task<dynamic> RdOpMsgAll_15_15i_Async()
    {
        return await Task.FromResult(RdOpMsg(-1, 578));
    }

    public async Task<dynamic> RdOpMsg1_16i_18iW_Async()
    {
        return await Task.FromResult(RdOpMsg());
    }

    public async Task<dynamic> RdOpMsg2_16i_18iW_Async()
    {
        return await Task.FromResult(RdOpMsg(1));
    }

    public async Task<dynamic> RdOpMsg3_16i_18iW_Async()
    {
        return await Task.FromResult(RdOpMsg(2));
    }

    public async Task<dynamic> RdOpMsg4_16i_18iW_Async()
    {
        return await Task.FromResult(RdOpMsg(3));
    }

    public async Task<dynamic> RdOpMsgAll_16i_18iW_Async()
    {
        return await Task.FromResult(RdOpMsg(-1, 1048));
    }

    public async Task<dynamic> RdOpMsg1_16_18_21_16i_18i_21i_0i_30i_PowerMatei_PMiA_Async()
    {
        return await Task.FromResult(RdOpMsg());
    }

    public async Task<dynamic> RdOpMsgAsync(short type = 0, short length = 262)
    {
        return await Task.FromResult(RdOpMsg(type, length));
    }

    public dynamic RdOpMsg(short type = 0, short length = 262)
    {
        var opmsg = new Focas.OPMSG();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdopmsg(_handle, type, length, opmsg); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdopmsg",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/misc/cnc_rdopmsg",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdopmsg = new {type, length}},
            response = new {cnc_rdopmsg = new {opmsg}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}