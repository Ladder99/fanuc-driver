using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace fanuc.collectors
{
    public class Basic03 : Collector
    {
        public Basic03(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = _machine.Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    dynamic paths = _machine.Platform.GetPath();

                    IEnumerable<int> path_slices = Enumerable
                        .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                    _machine.SliceVeneer(path_slices.ToArray());

                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                    _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                    
                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = _machine.Platform.SetPath(current_path);
                        dynamic axes = _machine.Platform.RdAxisName();
                        dynamic axis_slices = new List<dynamic> { };

                        var fields = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                        for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                        {
                            var axis = fields[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                            axis_slices.Add(((char) axis.name).ToString().Trim('\0') +
                                            ((char) axis.suff).ToString().Trim('\0'));
                        }

                        _machine.SliceVeneer(current_path, axis_slices.ToArray());

                        _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                    }
                    
                    dynamic disconnect = _machine.Platform.Disconnect();
                    
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
            dynamic connect = _machine.Platform.Connect();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic cncid = _machine.Platform.CNCId();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = _machine.Platform.RdTimer(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                dynamic poweron_6750 = _machine.Platform.RdParam(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                dynamic info = _machine.Platform.SysInfo();
                _machine.PeelVeneer("sys_info", info);
                
                dynamic paths = _machine.Platform.GetPath();
                _machine.PeelVeneer("get_path", paths);

                for (short current_path = paths.response.cnc_getpath.path_no;
                    current_path <= paths.response.cnc_getpath.maxpath_no;
                    current_path++)
                {
                    dynamic path = _machine.Platform.SetPath(current_path);
                    dynamic path_marker = new {path.request.cnc_setpath.path_no};
                    _machine.MarkVeneer(current_path, path_marker);

                    dynamic axes = _machine.Platform.RdAxisName();
                    _machine.PeelAcrossVeneer(current_path, "axis_name", axes);

                    dynamic spindles = _machine.Platform.RdSpdlName();
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
                            suff =  ((char)axis.suff).ToString().Trim('\0')
                        };
                        
                        _machine.MarkVeneer(new[] { current_path, axis_name }, new[] { path_marker, axis_marker });
                        
                        dynamic axis_data = _machine.Platform.RdDynamic2(current_axis, 44, 2);
                        _machine.PeelAcrossVeneer(new[] { current_path, axis_name }, "axis_data", axis_data);
                    }
                }

                dynamic disconnect = _machine.Platform.Disconnect();

                LastSuccess = connect.success;
            }
        }
    }
}