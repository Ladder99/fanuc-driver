namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdAlmMsg2Async(short type = 0, short num = 10)
    {
        return await Task.FromResult(RdAlmMsg2(type, num));
    }

    public dynamic RdAlmMsg2(short type = 0, short num = 10)
    {
        var num_out = num;
        var almmsg = new Focas.ODBALMMSG2();

        var ndr = _nativeDispatch(() =>
        {
            return (Focas.focas_ret) Focas.cnc_rdalmmsg2(_handle, type, ref num_out, almmsg);
        });

        var nr = new
        {
            @null = false,
            method = "cnc_rdalmmsg2",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/misc/cnc_rdalmmsg2",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdalmmsg2 = new {type, num}},
            response = new {cnc_rdalmmsg2 = new {num = num_out, almmsg}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }

    public async Task<dynamic> RdAlmMsg2AllAsync(short count = 10, short maxType = 20)
    {
        var alms = new Dictionary<short, dynamic>();

        for (short type = 0; type <= maxType; type++)
        {
            short countRead = 10;
            alms.Add(type, await RdAlmMsg2Async(type, countRead));
        }

        var nr = new
        {
            method = "cnc_rdalmmsg2_ALL",
            invocationMs = (long) alms.Sum(x => (int) x.Value.invocationMs),
            doc = $"{_docBasePath}/misc/cnc_rdalmmsg2",
            success = true, // TODO: aggregate
            rc = Focas.EW_OK, // TODO: aggregate
            request = new {cnc_rdalmmsg2_ALL = new {minType = 0, maxType, count}},
            response = new {cnc_rdalmmsg2_ALL = alms}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }

    public dynamic RdAlmMsg2All(short count = 10, short maxType = 20)
    {
        var alms = new Dictionary<short, dynamic>();

        for (short type = 0; type <= maxType; type++)
        {
            short countRead = 10;
            alms.Add(type, RdAlmMsg2(type, countRead));
        }

        var nr = new
        {
            method = "cnc_rdalmmsg2_ALL",
            invocationMs = (long) alms.Sum(x => (int) x.Value.invocationMs),
            doc = $"{_docBasePath}/misc/cnc_rdalmmsg2",
            success = true, // TODO: aggregate
            rc = Focas.EW_OK, // TODO: aggregate
            request = new {cnc_rdalmmsg2_ALL = new {minType = 0, maxType, count}},
            response = new {cnc_rdalmmsg2_ALL = alms}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}