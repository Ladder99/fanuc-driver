using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic04 : FanucCollector
    {
        public Basic04(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = ((FanucMachine)_machine).Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    dynamic paths = ((FanucMachine)_machine).Platform.GetPath();

                    IEnumerable<int> path_slices = Enumerable
                        .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                    _machine.SliceVeneer(path_slices.ToArray());

                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.StatInfo), "stat_info");
                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                    
                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = ((FanucMachine)_machine).Platform.SetPath(current_path);
                        
                        dynamic axes = ((FanucMachine)_machine).Platform.RdAxisName();
                        dynamic spindles = ((FanucMachine)_machine).Platform.RdSpdlName();
                        dynamic axis_spindle_slices = new List<dynamic> { };

                        var fields_axes = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                        for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                        {
                            var axis = fields_axes[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                            axis_spindle_slices.Add(((char) axis.name).ToString().Trim('\0') +
                                                    ((char) axis.suff).ToString().Trim('\0'));
                        }
                        
                        // similar to Basic03 example, we use the axis logic to configure spindle specific data
                        //  axis specific data is at the same level as spindle specific data... children of the path
                        var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                        for (int x = 0; x <= spindles.response.cnc_rdspdlname.data_num - 1; x++)
                        {
                            var spindle = fields_spindles[x].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                            axis_spindle_slices.Add(((char) spindle.name).ToString().Trim('\0') +
                                                    ((char) spindle.suff1).ToString().Trim('\0').Trim() +
                                                    ((char) spindle.suff2).ToString().Trim('\0').Trim() +
                                                    ((char) spindle.suff3).ToString().Trim('\0').Trim('\u0003').Trim());
                        };

                        _machine.SliceVeneer(current_path, axis_spindle_slices.ToArray());

                        _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                        _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdActs2), "spindle_data");
                    }
                    
                    dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();
                    
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    System.Threading.Thread.Sleep(_sweepMs);
                }
            }
        }

        public override void Collect()
        {
            dynamic connect = ((FanucMachine)_machine).Platform.Connect();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic cncid = ((FanucMachine)_machine).Platform.CNCId();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = ((FanucMachine)_machine).Platform.RdTimer(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                dynamic poweron_6750 = ((FanucMachine)_machine).Platform.RdParam(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                dynamic paths = ((FanucMachine)_machine).Platform.GetPath();
                _machine.PeelVeneer("get_path", paths);

                for (short current_path = paths.response.cnc_getpath.path_no;
                    current_path <= paths.response.cnc_getpath.maxpath_no;
                    current_path++)
                {
                    dynamic path = ((FanucMachine)_machine).Platform.SetPath(current_path);
                    dynamic path_marker = new {path.request.cnc_setpath.path_no};
                    _machine.MarkVeneer(current_path, path_marker);

                    dynamic info = ((FanucMachine)_machine).Platform.SysInfo();
                    _machine.PeelAcrossVeneer(current_path,"sys_info", info);
                    
                    dynamic stat = ((FanucMachine)_machine).Platform.StatInfo();
                    _machine.PeelAcrossVeneer(current_path, "stat_info", stat);
                    
                    dynamic axes = ((FanucMachine)_machine).Platform.RdAxisName();
                    _machine.PeelAcrossVeneer(current_path, "axis_name", axes);

                    dynamic spindles = ((FanucMachine)_machine).Platform.RdSpdlName();
                    _machine.PeelAcrossVeneer(current_path, "spindle_name", spindles);
                    
                    var fields_axes = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();

                    for (short current_axis = 1;
                        current_axis <= axes.response.cnc_rdaxisname.data_num;
                        current_axis++)
                    {
                        dynamic axis = fields_axes[current_axis-1].GetValue(axes.response.cnc_rdaxisname.axisname);
                        dynamic axis_name = ((char) axis.name).ToString().Trim('\0') + ((char) axis.suff).ToString().Trim('\0');
                        dynamic axis_marker = new
                        {
                            name = ((char)axis.name).ToString().Trim('\0'), 
                            suff =  ((char)axis.suff).ToString().Trim('\0')
                        };
                        
                        _machine.MarkVeneer(new[] { current_path, axis_name }, new[] { path_marker, axis_marker });
                        
                        dynamic axis_data = ((FanucMachine)_machine).Platform.RdDynamic2(current_axis, 44, 2);
                        _machine.PeelAcrossVeneer(new[] { current_path, axis_name }, "axis_data", axis_data);
                    }
                    
                    var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                    
                    // again, similar concept as the axes
                    //  walk each spindle,
                    //      get its name (e.g. S1,S2,...),
                    //      create a descriptive marker,
                    //      mark the veneer,
                    //      retrieve spindle specific data,
                    //      reveal spindle specific observation
                    for (short current_spindle = 1;
                        current_spindle <= spindles.response.cnc_rdspdlname.data_num;
                        current_spindle++)
                    {
                        var spindle = fields_spindles[current_spindle - 1].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                        dynamic spindle_name = ((char) spindle.name).ToString().Trim('\0') +
                                                ((char) spindle.suff1).ToString().Trim('\0').Trim() +
                                                ((char) spindle.suff2).ToString().Trim('\0').Trim() +
                                                ((char) spindle.suff3).ToString().Trim('\0').Trim('\u0003').Trim();
                        dynamic spindle_marker = new
                        {
                            name = ((char)spindle.name).ToString().Trim('\0'), 
                            suff1 =  ((char)spindle.suff1).ToString().Trim('\0').Trim(),
                            suff2 =  ((char)spindle.suff2).ToString().Trim('\0').Trim(),
                            suff3 =  ((char)spindle.suff3).ToString().Trim('\0').Trim('\u0003').Trim()
                        };
                        
                        _machine.MarkVeneer(new[] { current_path, spindle_name }, new[] { path_marker, spindle_marker });
                        
                        dynamic spindle_data = ((FanucMachine)_machine).Platform.Acts2(current_spindle);
                        _machine.PeelAcrossVeneer(new[] { current_path, spindle_name }, "spindle_data", spindle_data);
                    };
                }

                dynamic disconnect = ((FanucMachine)_machine).Platform.Disconnect();
                
                LastSuccess = connect.success;
            }
        }
    }
}