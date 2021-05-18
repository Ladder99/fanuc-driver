using System;
using System.Collections.Generic;
using System.Linq;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic02 : FanucCollector
    {
        public Basic02(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            // repeat the initialization loop until we have success
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                // connect to the controller, we will interrogate its execution paths later
                dynamic connect = ((FanucMachine)_machine).Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    // below veneers will reveal observations relevant to the entire controller
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    // get a list of available execution paths from the controller
                    dynamic paths = ((FanucMachine)_machine).Platform.GetPath();

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
                    dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();
                    
                    // initialization successful
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // TODO: not in here
                    System.Threading.Thread.Sleep(_sweepMs);
                }
            }
        }

        public override void Collect()
        {
            // on each sweep of the collector...
            
            // connect to the machine
            dynamic connect = ((FanucMachine)_machine).Platform.Connect();
            // reveal the "connect" observation by peeling the veneer associated with it
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                // similarly, reveal below observations from the values returned by Focas API
                dynamic cncid = ((FanucMachine)_machine).Platform.CNCId();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = ((FanucMachine)_machine).Platform.RdTimer(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                // here is an example where the RdParamLData veneer is generic based on the input parameters
                //  and can be applied to multiple observations
                dynamic poweron_6750 = ((FanucMachine)_machine).Platform.RdParam(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                // retrieve the number of paths to walk each one
                dynamic paths = ((FanucMachine)_machine).Platform.GetPath();
                _machine.PeelVeneer("get_path", paths);

                // walk each path and retrieve values relevant to it
                for (short current_path = paths.response.cnc_getpath.path_no;
                    current_path <= paths.response.cnc_getpath.maxpath_no;
                    current_path++)
                {
                    // when the path is set using Focas API, consecutive calls will retrieve that path's data
                    dynamic path = ((FanucMachine)_machine).Platform.SetPath(current_path);
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
                    dynamic info = ((FanucMachine)_machine).Platform.SysInfo();
                    _machine.PeelAcrossVeneer(current_path, "sys_info", info);
                    
                    // 'axis_name' observation contains the axis names for this path
                    dynamic axes = ((FanucMachine)_machine).Platform.RdAxisName();
                    _machine.PeelAcrossVeneer(current_path, "axis_name", axes);

                    // 'spindle_name' observation contains the spindle names for this path
                    dynamic spindles = ((FanucMachine)_machine).Platform.RdSpdlName();
                    _machine.PeelAcrossVeneer(current_path, "spindle_name", spindles);
                }

                // finally, disconnect
                dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();

                // this sweep has been successful
                LastSuccess = connect.success;
            }
        }
    }
}