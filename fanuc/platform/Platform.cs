using System;
using System.Diagnostics;
using NLog;

namespace l99.driver.fanuc
{
    public partial class Platform
    {
        private ILogger _logger;
        
        private FanucMachine _machine;

        private ushort _handle;

        private struct NativeDispatchReturn
        {
            public Focas1.focas_ret RC;
            public long ElapsedMilliseconds;
        }
        
        private Func<Func<Focas1.focas_ret>, NativeDispatchReturn> nativeDispatch = (nativeCallWrapper) =>
        {
            Focas1.focas_ret rc = Focas1.focas_ret.EW_OK;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            rc = nativeCallWrapper();
            sw.Stop();
            return new NativeDispatchReturn
            {
                RC = rc,
                ElapsedMilliseconds = sw.ElapsedMilliseconds
            };
        };
        
        public Platform(FanucMachine machine)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _machine = machine;
        }
    }
}