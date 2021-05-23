using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> Async(short type = 0, short block = 0, int ODBMDL_type = 1)
        {
            return await Task.FromResult(Modal(type, block, ODBMDL_type));
        }
        
        public dynamic Modal(short type = 0, short block = 0, int ODBMDL_type = 1)
        {
            dynamic modal = new object();

            switch (ODBMDL_type)
            {
                case 1:
                    modal = new Focas1.ODBMDL_1();
                    break;
                case 2:
                    modal = new Focas1.ODBMDL_2();
                    break;
                case 3:
                    modal = new Focas1.ODBMDL_3();
                    break;
                case 4:
                    modal = new Focas1.ODBMDL_4();
                    break;
                case 5:
                    modal = new Focas1.ODBMDL_5();
                    break;
            }

            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas1.focas_ret) Focas1.cnc_modal(_handle, type, block, modal);
            });

            var nr = new
            {
                method = "cnc_modal",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = "https://www.inventcom.net/fanuc-focas-library/misc/cnc_modal",
                success = ndr.RC == Focas1.EW_OK,
                rc = ndr.RC,
                request = new {cnc_modal = new {type, block, ODBMDL_type}},
                response = new {cnc_modal = new {modal, modal_type = modal.GetType().Name}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}