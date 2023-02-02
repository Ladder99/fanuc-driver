using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc;

public partial class Platform
{
    /*
    Once the library handle number is acquired, it must be held until the application program terminates, 
    because it is necessary to pass the number as an argument to each CNC/PMC Data window library function.

    The library handle is owned by the thread that got it. Even if the thread-A which has already got a library 
    handle shows the library handle to another thread-B, the thread-B cannot use that library handle.

    It is possible that one thread gets multiple library handles by calling the function for getting the library 
    handle repeatedly. For example, on the 2 path control system, it is possible that the thread on an application 
    gets two library handles for the same CNC (in other words, the same Node or the same IP address) and allocates 
    the individual path to each handle.

    If you create some handles for the same IP address, the library creates only one TCP/IP connection per process. 
    This is for saving the resource of the Ethernet Board on the CNC side. So when the connection of one library 
    handle is destroyed by the communication error and TCP/IP connection closes, the connection in other handle 
    is destroyed, too.
    */
    
    /*
    private readonly Func<Func<Focas.focas_ret>, NativeDispatchReturn> _nativeDispatch = nativeCallWrapper =>
    {
        var rc = Focas.focas_ret.EW_OK;
        var sw = new Stopwatch();
        sw.Start();
        rc = nativeCallWrapper();
        sw.Stop();
        return new NativeDispatchReturn
        {
            RC = rc,
            ElapsedMilliseconds = sw.ElapsedMilliseconds
        };
    };
    */
    
    private struct NativeDispatchReturn
    {
        public Focas.focas_ret RC;
        public long ElapsedMilliseconds;
        public long WaitingMilliseconds;
    }
    
    public Platform(FanucMachine machine)
    {
        _logger = LogManager.GetCurrentClassLogger();
        _machine = machine;
        _singleThreadRunner = new SingleThreadBlockingRunner();
    }

    private readonly ILogger _logger;
    private readonly FanucMachine _machine;
    
    private readonly SingleThreadBlockingRunner _singleThreadRunner;
    private ushort _handle;
    public ushort Handle => _handle;
    
    private readonly string _docBasePath = "https://docs.ladder99.com/drivers/fanuc-driver/focas-api";

    private NativeDispatchReturn _nativeDispatch(Func<Focas.focas_ret> func)
    {
        Focas.focas_ret innerRc = Focas.focas_ret.EW_OK;
        long innerElapsed = 0;
        Stopwatch outerSw = new();
        
        outerSw.Start();
        (innerElapsed, innerRc) = _singleThreadRunner.Run(() =>
        {
            var sw = new Stopwatch();
            sw.Start();
            var rc = func();
            sw.Stop();
            return (sw.ElapsedMilliseconds, rc);
        }).Result;
        outerSw.Stop();
        
        //Console.WriteLine($"elapsed={innerElapsed}, waiting={outerSw.ElapsedMilliseconds - innerElapsed}");
        
        return new NativeDispatchReturn
        {
            RC = innerRc,
            ElapsedMilliseconds = innerElapsed,
            WaitingMilliseconds = outerSw.ElapsedMilliseconds - innerElapsed
        };
    }
}