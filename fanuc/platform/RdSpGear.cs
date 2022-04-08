
namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdSpGearAsync(short sp_no = 1)
        {
            return await Task.FromResult(RdSpGear(sp_no));
        }
        
        public dynamic RdSpGear(short sp_no = 1)
        {
            Focas.ODBSPN serialspindle = new Focas.ODBSPN();

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdspgear(_handle, sp_no, serialspindle);
            });

            var nr = new
            {
                method = "cnc_rdspgear",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/position/cnc_rdspgear",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdspgear = new {sp_no}},
                response = new {cnc_rdspgear = new {serialspindle}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}