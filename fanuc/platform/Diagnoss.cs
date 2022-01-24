using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        /* no axis returns EW_ATTRIB
        public async Task<dynamic> DiagnossByteNoAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 0, 4+1*1, 1));
        }
        
        public async Task<dynamic> DiagnossWordNoAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 0, 4+2*1, 1));
        }
        
        public async Task<dynamic> DiagnossDoubleWordNoAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 0, 4+4*1, 1));
        }
        
        public async Task<dynamic> DiagnossRealNoAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 0, 4+8*1, 1));
        }
        */
        
        public async Task<dynamic> DiagnossByteFirstAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 1, 4+1*1, 1));
        }
        
        public async Task<dynamic> DiagnossWordFirstAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 1, 4+2*1, 1));
        }
        
        public async Task<dynamic> DiagnossDoubleWordFirstAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 1, 4+4*1, 1));
        }
        
        public async Task<dynamic> DiagnossRealFirstAxisAsync(short number = 0)
        {
            return await Task.FromResult(Diagnoss(number, 1, 4+8*1, 1));
        }
        
        public async Task<dynamic> DiagnossAsync(short number = 0, short axis = 0, short length = 5, int ODBDGN_type = 1)
        {
            return await Task.FromResult(Diagnoss(number, axis, length, ODBDGN_type));
        }
        
        public dynamic Diagnoss(short number = 0, short axis = 0, short length = 5, int ODBDGN_type = 1)
        {
            dynamic diag = new object();

            switch (ODBDGN_type)
            {
                case 1:
                    diag = new Focas.ODBDGN_1();
                    break;
                case 2:
                    diag = new Focas.ODBDGN_2();
                    break;
                case 3:
                    diag = new Focas.ODBDGN_3();
                    break;
                case 4:
                    diag = new Focas.ODBDGN_4();
                    break;
            }

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_diagnoss(_handle, number, axis, length, diag);
            });

            var nr = new
            {
                method = "cnc_diagnoss",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/misc/cnc_diagnoss",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_diagnoss = new {number, axis, length, ODBDGN_type}},
                response = new {cnc_diagnoss = new {diag}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}