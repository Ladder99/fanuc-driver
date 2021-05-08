using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace fanuc.collectors
{
    public class Basic : Collector
    {
        public Basic(Machine machine) : base(machine)
        {
            #if ARMV7
            Console.WriteLine("ARMV7 - Focas1.cnc_startupprocess()");
            machine.Platform.StartupProcess();
            #endif
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersCreated)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = _machine.Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.AddVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.AddVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.AddVeneer(typeof(fanuc.veneers.GetPath), "get_path");

                    dynamic paths = _machine.Platform.GetPath();

                    IEnumerable<int> path_slices = Enumerable
                        .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                    _machine.SliceVeneer(path_slices.ToArray());

                    _machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                    _machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.StatInfo), "stat_info");
                    _machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.Alarms), "alarms");
                    _machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.OpMsgs), "op_msgs");
                    _machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.Block), "code_block");
                    _machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.RdParamLData), "part_count");

                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
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

                        _machine.AddVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                    }

                    dynamic disconnect = _machine.Platform.Disconnect();
                    _machine.VeneersCreated = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }

        public override void Collect()
        {
            dynamic connect = _machine.Platform.Connect();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic info = _machine.Platform.SysInfo();
                _machine.PeelVeneer("sys_info", info);

                dynamic paths = _machine.Platform.GetPath();
                _machine.PeelVeneer("get_path", paths);

                for (short current_path = paths.response.cnc_getpath.path_no;
                    current_path <= paths.response.cnc_getpath.maxpath_no;
                    current_path++)
                {
                    dynamic path = _machine.Platform.SetPath(current_path);
                    dynamic path_marker = new { path.request.cnc_setpath.path_no };
                    _machine.MarkVeneer(current_path, path_marker);

                    //dynamic tool = machine.Platform.Modal(108, 1, 3);
                    //writeJsonToConsole(tool);
                    
                    dynamic stat = _machine.Platform.StatInfo();
                    _machine.PeelAcrossVeneer(current_path, "stat_info", stat);
                    
                    dynamic opmsgs = _machine.Platform.RdOpMsg();
                    _machine.PeelAcrossVeneer(current_path, "op_msgs", opmsgs);
                    
                    dynamic alms = _machine.Platform.RdAlmMsgAll();
                    _machine.PeelAcrossVeneer(current_path, "alarms", alms);

                    dynamic part_count = _machine.Platform.RdParam(6712, 0, 8, 1);
                    _machine.PeelAcrossVeneer(current_path, "part_count", part_count);

                    dynamic prog = _machine.Platform.RdExecProg(512);
                    _machine.PeelAcrossVeneer(current_path, "code_block", prog);
                    
                    dynamic axes = _machine.Platform.RdAxisName();
                    _machine.PeelAcrossVeneer(current_path, "axis_name", axes);
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