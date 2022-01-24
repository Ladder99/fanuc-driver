using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        public async Task<dynamic> RdParamByteNoAxisAsync(short number)
        {
            return await Task.FromResult(RdParam(number, 0, 4+1+1, 1));
        }
        
        public async Task<dynamic> RdParamWordNoAxisAsync(short number)
        {
            return await Task.FromResult(RdParam(number, 0, 4+2*1, 1));
        }
        
        public async Task<dynamic> RdParamDoubleWordNoAxisAsync(short number)
        {
            return await Task.FromResult(RdParam(number, 0, 6+2*1, 1));
        }
        
        public async Task<dynamic> RdParamRealNoAxisAsync(short number)
        {
            return await Task.FromResult(RdParam(number, 0, 4+8*1, 1));
        }
    
        public async Task<dynamic> RdParamAsync(short number, short axis, short length, int IODBPSD_type)
        {
            return await Task.FromResult(RdParam(number, axis, length, IODBPSD_type));
        }
        
        public dynamic RdParam(short number, short axis, short length, int IODBPSD_type)
        {
            dynamic param = new object();

            switch (IODBPSD_type)
            {
                case 1:
                    param = new Focas.IODBPSD_1();
                    break;
                case 2:
                    param = new Focas.IODBPSD_2();
                    break;
                case 3:
                    param = new Focas.IODBPSD_3();
                    break;
                case 4:
                    param = new Focas.IODBPSD_4();
                    break;
            }

            //Console.WriteLine(param.GetType().FullName);
            
            NativeDispatchReturn ndr = nativeDispatch(() =>
            {
                return (Focas.focas_ret) Focas.cnc_rdparam(_handle, number, axis, length, param);
            });

            //Console.WriteLine(JObject.FromObject(param).ToString());
            
            var nr = new
            {
                method = "cnc_rdparam",
                invocationMs = ndr.ElapsedMilliseconds,
                doc = $"{this._docBasePath}/ncdata/cnc_rdparam",
                success = ndr.RC == Focas.EW_OK,
                rc = ndr.RC,
                request = new {cnc_rdparam = new {number, axis, length, IODBPSD_type}},
                response = new {cnc_rdparam = new {param, IODBPSD_type = param.GetType().Name}}
            };
            
            _logger.Trace($"[{_machine.Id}] Platform invocation result:\n{JObject.FromObject(nr).ToString()}");

            return nr;
        }
    }
}