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
        public Basic02(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {

        }

        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                // repeat the initialization loop until we have success
                while (!machine.VeneersApplied)
                {
                    Console.WriteLine("fanuc - creating veneers");

                    // connect to the controller, we will interrogate its execution paths later
                    dynamic connect = await machine["platform"].ConnectAsync();
                    
                    if (connect.success)
                    {
                        // below veneers will reveal observations relevant to the entire controller
                        machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                        machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                        machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                        machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");

                        // get a list of available execution paths from the controller
                        dynamic paths = await machine["platform"].GetPathAsync();

                        // turn the available paths into an array, e.g. [1,2,3]
                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        // slice the veneer, making it addressable using individual path numbers
                        machine.SliceVeneer(path_slices.ToArray());

                        // below veneers will be laid down across each 'slice'
                        //  this means that 'sys_info', 'axis_name', and 'spindle_name' observations
                        //  will be available for each execution path
                        machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                        machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                        machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");

                        // disconnect from controller
                        dynamic disconnect = await machine["platform"].DisconnectAsync();

                        // initialization successful
                        machine.VeneersApplied = true;
                    }
                    else
                    {
                        await Task.Delay(sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                // on each sweep of the collector...

                // connect to the machine
                dynamic connect = await machine["platform"].ConnectAsync();
                // reveal the "connect" observation by peeling the veneer associated with it
                await machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    // similarly, reveal below observations from the values returned by Focas API
                    dynamic cncid = await machine["platform"].CNCIdAsync();
                    await machine.PeelVeneerAsync("cnc_id", cncid);

                    dynamic poweron = await machine["platform"].RdTimerAsync(0);
                    await machine.PeelVeneerAsync("power_on_time", poweron);

                    // here is an example where the RdParamLData veneer is generic based on the input parameters
                    //  and can be applied to multiple observations
                    dynamic poweron_6750 = machine["platform"].RdParamDoubleWordNoAxisAsync(6750);
                    await machine.PeelVeneerAsync("power_on_time_6750", poweron_6750);

                    // retrieve the number of paths to walk each one
                    dynamic paths = await machine["platform"].GetPathAsync();
                    await machine.PeelVeneerAsync("get_path", paths);

                    // walk each path and retrieve values relevant to it
                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        // when the path is set using Focas API, consecutive calls will retrieve that path's data
                        dynamic path = await machine["platform"].SetPathAsync(current_path);
                        // create a marker for the path, this marker will become part of the output
                        //  to help identify the exact source of the observation
                        dynamic path_marker = new {path.request.cnc_setpath.path_no};
                        // tag each veneer with the created marker
                        //  e.g.
                        //      path 1 will be marked with { path_no = 1 }
                        //      path 2 will be marked with { path_no = 2 } ...
                        machine.MarkVeneer(current_path, path_marker);

                        // reveal observations for the current path
                        //  next iteration of the loop will reveal observations for that path ...

                        // 'sys_info' observation contains the number of axes for this path
                        dynamic info = await machine["platform"].SysInfoAsync();
                        await machine.PeelAcrossVeneerAsync(current_path, "sys_info", info);

                        // 'axis_name' observation contains the axis names for this path
                        dynamic axes = await machine["platform"].RdAxisNameAsync();
                        await machine.PeelAcrossVeneerAsync(current_path, "axis_name", axes);

                        // 'spindle_name' observation contains the spindle names for this path
                        dynamic spindles = await machine["platform"].RdSpdlNameAsync();
                        await machine.PeelAcrossVeneerAsync(current_path, "spindle_name", spindles);
                    }

                    // finally, disconnect
                    dynamic disconnect = await machine["platform"].DisconnectAsync();

                    // this sweep has been successful
                    LastSuccess = connect.success;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}