namespace l99.driver.fanuc;

public partial class Platform
{
    public async Task<dynamic> StartupProcessAsync(short level = 0, string filename = "~/focas2.log")
    {
        return await Task.FromResult(StartupProcess(level, filename));
    }

    public dynamic StartupProcess(short level = 0, string filename = "~/focas2.log")
    {
#if ARMV7 || LINUX64 || LINUX32
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret)Focas.cnc_startupprocess(level, filename);
            });

            var nr = new
            {
                @null = false,
                method = "cnc_startupprocess",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new { cnc_startupprocess = new { level, filename } },
                response = new { cnc_startupprocess = new { } }
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
#else
        var nr = new
        {
            @null = false,
            method = "cnc_startupprocess",
            invocationMs = -1,
            doc = "",
            success = true,
            Focas.EW_OK,
            request = new {cnc_startupprocess = new {level, filename}},
            response = new {cnc_startupprocess = new { }}
        };

        _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr)}");

        return nr;
#endif
    }
}