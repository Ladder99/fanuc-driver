using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic02 : FanucCollector
    {
        public Basic02(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {

        }

        public override async Task InitializeAsync()
        {
            try
            {
                // repeat the initialization loop until we have success
                while (!_machine.VeneersApplied)
                {
                    Console.WriteLine("fanuc - creating veneers");

                    // connect to the controller, we will interrogate its execution paths later
                    dynamic connect = await _machine["platform"].ConnectAsync();
                    
                    if (connect.success)
                    {
                        // below veneers will reveal observations relevant to the entire controller
                        _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");

                        // get a list of available execution paths from the controller
                        dynamic paths = await _machine["platform"].GetPathAsync();

                        // turn the available paths into an array, e.g. [1,2,3]
                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        // slice the veneer, making it addressable using individual path numbers
                        _machine.SliceVeneer(path_slices.ToArray());

                        // below veneers will be laid down across each 'slice'
                        //  this means that 'sys_info', 'axis_name', and 'spindle_name' observations
                        //  will be available for each execution path
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");

                        // disconnect from controller
                        dynamic disconnect = await _machine["platform"].DisconnectAsync();

                        // initialization successful
                        _machine.VeneersApplied = true;

                        Console.WriteLine("fanuc - created veneers");
                    }
                    else
                    {
                        await Task.Delay(_sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector initialization failed.");
            }
        }

        public override async Task CollectAsync()
        {
            try
            {
                // on each sweep of the collector...

                // connect to the machine
                dynamic connect = await _machine["platform"].ConnectAsync();
                // reveal the "connect" observation by peeling the veneer associated with it
                await _machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    // similarly, reveal below observations from the values returned by Focas API
                    dynamic cncid = await _machine["platform"].CNCIdAsync();
                    await _machine.PeelVeneerAsync("cnc_id", cncid);

                    dynamic poweron = await _machine["platform"].RdTimerAsync(0);
                    await _machine.PeelVeneerAsync("power_on_time", poweron);

                    // here is an example where the RdParamLData veneer is generic based on the input parameters
                    //  and can be applied to multiple observations
                    dynamic poweron_6750 = await _machine["platform"].RdParamAsync(6750, 0, 8, 1);
                    await _machine.PeelVeneerAsync("power_on_time_6750", poweron_6750);

                    // retrieve the number of paths to walk each one
                    dynamic paths = await _machine["platform"].GetPathAsync();
                    await _machine.PeelVeneerAsync("get_path", paths);

                    // walk each path and retrieve values relevant to it
                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        // when the path is set using Focas API, consecutive calls will retrieve that path's data
                        dynamic path = await _machine["platform"].SetPathAsync(current_path);
                        // create a marker for the path, this marker will become part of the output
                        //  to help identify the exact source of the observation
                        dynamic path_marker = new {path.request.cnc_setpath.path_no};
                        // tag each veneer with the created marker
                        //  e.g.
                        //      path 1 will be marked with { path_no = 1 }
                        //      path 2 will be marked with { path_no = 2 } ...
                        _machine.MarkVeneer(current_path, path_marker);

                        // reveal observations for the current path
                        //  next iteration of the loop will reveal observations for that path ...

                        // 'sys_info' observation contains the number of axes for this path
                        dynamic info = await _machine["platform"].SysInfoAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "sys_info", info);

                        // 'axis_name' observation contains the axis names for this path
                        dynamic axes = await _machine["platform"].RdAxisNameAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "axis_name", axes);

                        // 'spindle_name' observation contains the spindle names for this path
                        dynamic spindles = await _machine["platform"].RdSpdlNameAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "spindle_name", spindles);
                    }

                    // finally, disconnect
                    dynamic disconnect = await _machine["platform"].DisconnectAsync();

                    // this sweep has been successful
                    LastSuccess = connect.success;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed.");
            }
        }
    }
}