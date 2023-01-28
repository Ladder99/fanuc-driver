namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdExecPtAsync()
    {
        return await Task.FromResult(RdExecPt());
    }

    public dynamic RdExecPt()
    {
        var pact = new Focas.PRGPNT();
        var pnext = new Focas.PRGPNT();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdexecpt(_handle, pact, pnext); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdexecpt",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/program/cnc_rdexecpt",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdexecpt = new { }},
            response = new {cnc_rdexecpt = new {pact, pnext}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}