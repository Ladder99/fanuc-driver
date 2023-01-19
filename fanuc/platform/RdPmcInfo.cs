
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdPmcInfoAsync(short adr_type = -1)
        {
            return await Task.FromResult(RdPmcInfo(adr_type));
        }
        
        public dynamic RdPmcInfo(short adr_type)
        {
            Focas.ODBPMCINF pmcif = new Focas.ODBPMCINF();

            NativeDispatchReturn ndr = _nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.pmc_rdpmcinfo(_handle, adr_type, pmcif);
            });

            var nr = new
            {
                @null = false,
                method = "pmc_rdpmcinfo",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{_docBasePath}/pmc/pmc_rdpmcinfo",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {pmc_rdpmcinfo = new {adr_type }},
                response = new {pmc_rdpmcinfo = new {pmcif }}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}