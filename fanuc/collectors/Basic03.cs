using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic03 : FanucCollector
    {
        public Basic03(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override async Task InitializeAsync()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = await _machine["platform"].ConnectAsync();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    dynamic paths = await _machine["platform"].GetPathAsync();

                    IEnumerable<int> path_slices = Enumerable
                        .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                    _machine.SliceVeneer(path_slices.ToArray());

                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                    
                    // below we will be adding axis specific observations
                    // axes are children of a path
                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        // set the current path
                        dynamic path = await _machine["platform"].SetPathAsync(current_path);
                        // so that we can retrieve the axis names on that path
                        dynamic axes = await _machine["platform"].RdAxisNameAsync();
                        // Focas API returns a struct of axis names (e.g. data1,data2,data3,...)
                        dynamic axis_slices = new List<dynamic> { };
                        // use reflection to walk all available axes
                        var fields = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                        for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                        {
                            var axis = fields[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                            // capture each axis name so we can use it to slice each path
                            //  e.g. X1,Y1,Z1,A1
                            axis_slices.Add(((char) axis.name).ToString().Trim('\0') +
                                            ((char) axis.suff).ToString().Trim('\0'));
                        }
                        
                        // now slice the current path's veneers using the axis names
                        _machine.SliceVeneer(current_path, axis_slices.ToArray());
                        // apply the 'axis_data' observation to the current path and its axes
                        _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                    }
                    
                    dynamic disconnect = await _machine["platform"].DisconnectAsync();
                    
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    await Task.Delay(_sweepMs);
                }
            }
        }

        public override async Task CollectAsync()
        {
            dynamic connect = await _machine["platform"].ConnectAsync();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic cncid = await _machine["platform"].CNCIdAsync();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = await _machine["platform"].RdTimerAsync(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                dynamic poweron_6750 = await _machine["platform"].RdParamAsync(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                dynamic paths = await _machine["platform"].GetPathAsync();
                _machine.PeelVeneer("get_path", paths);

                for (short current_path = paths.response.cnc_getpath.path_no;
                    current_path <= paths.response.cnc_getpath.maxpath_no;
                    current_path++)
                {
                    dynamic path = await _machine["platform"].SetPathAsync(current_path);
                    dynamic path_marker = new {path.request.cnc_setpath.path_no};
                    _machine.MarkVeneer(current_path, path_marker);

                    dynamic info = await _machine["platform"].SysInfoAsync();
                    _machine.PeelAcrossVeneer(current_path,"sys_info", info);

                    dynamic axes = await _machine["platform"].RdAxisNameAsync();
                    _machine.PeelAcrossVeneer(current_path, "axis_name", axes);

                    dynamic spindles = await _machine["platform"].RdSpdlNameAsync();
                    _machine.PeelAcrossVeneer(current_path, "spindle_name", spindles);
                    
                    var fields = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                    
                    for (short current_axis = 1;
                        current_axis <= axes.response.cnc_rdaxisname.data_num;
                        current_axis++)
                    {
                        dynamic axis = fields[current_axis-1].GetValue(axes.response.cnc_rdaxisname.axisname);
                        dynamic axis_name = ((char) axis.name).ToString().Trim('\0') + ((char) axis.suff).ToString().Trim('\0');
                        dynamic axis_marker = new
                        {
                            name = ((char)axis.name).ToString().Trim('\0'), 
                            suff = ((char)axis.suff).ToString().Trim('\0')
                        };
                        
                        // mark the current path and axis
                        //  path 1      1, X1;  1, Y1;  1, Z1;  1, A1;
                        //  path 2      2, B1;
                        // with a descriptive marker
                        //  { path_no = 1}, { name = X, suff = 1 }
                        //  ...
                        //  { path_no = 2}, { name = B, suff = 1 }
                        _machine.MarkVeneer(new[] { current_path, axis_name }, new[] { path_marker, axis_marker });
                        // retrieve axis position data for the current path and current axis
                        dynamic axis_data = await _machine["platform"].RdDynamic2Async(current_axis, 44, 2);
                        // reveal the 'axis_data' observation for the current path and current axis
                        _machine.PeelAcrossVeneer(new[] { current_path, axis_name }, "axis_data", axis_data);
                    }
                }

                dynamic disconnect = await _machine["platform"].DisconnectAsync();

                LastSuccess = connect.success;
            }
        }
    }
}