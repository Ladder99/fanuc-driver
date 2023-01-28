namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> RdSpeedAsync(short type = 0)
    {
        return await Task.FromResult(RdSpeed(type));
    }

    public dynamic RdSpeed(short type = 0)
    {
        var speed = new Focas.ODBSPEED();

        var ndr = _nativeDispatch(() => { return (Focas.focas_ret) Focas.cnc_rdspeed(_handle, type, speed); });

        var nr = new
        {
            @null = false,
            method = "cnc_rdspeed",
            invocationMs = ndr.ElapsedMilliseconds,
            doc = $"{_docBasePath}/position/cnc_rdspeed",
            success = ndr.RC == Focas.EW_OK,
            rc = ndr.RC,
            request = new {cnc_rdspeed = new {type}},
            response = new {cnc_rdspeed = new {speed}}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
    }
}