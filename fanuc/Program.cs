using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using fanuc.veneers;
using Newtonsoft.Json.Linq;

namespace fanuc
{
    class Program
    {
        static void Main(string[] args)
        {
            createVeneers();
            processVeneers();
        }

        static void writeJsonToConsole(dynamic d)
        {
            Console.WriteLine(JObject.FromObject(d).ToString());
        }
        
        
        static fanuc.Machine _machine = new fanuc.Machine("172.16.13.100", 8193, 2);
            
        static fanuc.veneers.Veneers _veneers = new Veneers();

        static void createVeneers()
        {
            bool created = false;

            while (!created)
            {
                Console.WriteLine("fanuc - creating veneers");
                
                dynamic connect = _machine.Connect();
                writeJsonToConsole(connect);

                if (connect.success)
                {
                    _veneers.Add(typeof(fanuc.veneers.Connect), "connect");
                    _veneers.Add(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _veneers.Add(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    dynamic paths = _machine.GetPath();
                    writeJsonToConsole(paths);

                    var slices = Enumerable
                        .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no).ToArray();
                    
                    _veneers.Slice(slices);
                    
                    _veneers.AddAcrossSlices(typeof(fanuc.veneers.StatInfo), "stat_info");
                    _veneers.AddAcrossSlices(typeof(fanuc.veneers.Block), "code_block");
                    _veneers.AddAcrossSlices(typeof(fanuc.veneers.RdParamLData), "part_count");
                    _veneers.AddAcrossSlices(typeof(fanuc.veneers.RdDynamic2), "axis_data_X");
                    
                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic axes = _machine.RdAxisName();
                        writeJsonToConsole(axes);
                    }

                    dynamic disconnect = _machine.Disconnect();
                    created = true;
                    
                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        
        static void processVeneers()
        {
            while (true)
            {
                dynamic connect = _machine.Connect();
                _veneers.Peel("connect", connect);

                if (connect.success)
                {
                    dynamic info = _machine.SysInfo();
                    _veneers.Peel("sys_info", info);
                    
                    dynamic paths = _machine.GetPath();
                    _veneers.Peel("get_path", paths);

                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = _machine.SetPath(current_path);
                        dynamic path_marker = new { path.request.cnc_setpath.path_no };
                        _veneers.Mark(current_path, path_marker);
                        
                        dynamic stat = _machine.StatInfo();
                        _veneers.PeelAcross(current_path, "stat_info", stat);

                        dynamic part_count = _machine.RdParam(6711, 0, 8, 1);
                        _veneers.PeelAcross(current_path, "part_count", part_count);

                        dynamic prog = _machine.RdExecProg(1024);
                        _veneers.PeelAcross(current_path, "code_block", prog);

                        dynamic axis_data_x = _machine.RdDynamic2(1, 44);
                        _veneers.PeelAcross(current_path, "axis_data_X", axis_data_x);
                        
                        
                        
                        //dynamic axis_data_y = machine.RdDynamic2(2, 44);
                        //parse_axis_data_Y.Peel(axis_data_y);

                        //dynamic axis_data_z = machine.RdDynamic2(3, 44);
                        //parse_axis_data_Z.Peel(axis_data_z);

                        //dynamic axis_data = machine.RdDynamic2(-1, 44);
                    }

                    dynamic disconnect = _machine.Disconnect();
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        static void test2()
        {
            var machine = new fanuc.Machine("172.16.13.100", 8193, 2);

            dynamic connect = machine.Connect();
            writeJsonToConsole(connect);

            if (connect.success)
            {
                dynamic paths = machine.GetPath();
                writeJsonToConsole(paths);

                for (short current_path = paths.response.cnc_getpath.path_no; 
                        current_path <= paths.response.cnc_getpath.maxpath_no; 
                        current_path ++)
                {
                    dynamic path = machine.SetPath(current_path);
                    writeJsonToConsole(path);

                    dynamic info = machine.SysInfo();
                    writeJsonToConsole(info);

                    dynamic stat = machine.StatInfo();
                    writeJsonToConsole(stat);

                    dynamic mcode = machine.Modal(106, 0, 3);
                    writeJsonToConsole(mcode);
                    
                    dynamic gcode = machine.Modal(0, 0, 1);
                    writeJsonToConsole(gcode);

                    dynamic tool = machine.Modal(108, 1, 3);
                    writeJsonToConsole(tool);

                    dynamic prog = machine.RdExecProg(32);
                    writeJsonToConsole(prog);

                    dynamic alms = machine.RdAlmMsgAll();
                    writeJsonToConsole(alms);

                    dynamic msgs = machine.RdOpMsg();
                    writeJsonToConsole(msgs);

                    dynamic axes = machine.RdAxisName();
                    writeJsonToConsole(axes);

                    dynamic axes_load = machine.RdSvMeter();
                    writeJsonToConsole(axes_load);

                    dynamic axes_fd_cmd = machine.Modal(103, 1, 4);
                    writeJsonToConsole(axes_fd_cmd);

                    dynamic cmd_pos = machine.Modal(-3, 1, 5);
                    writeJsonToConsole(cmd_pos);
                    
                    dynamic spindles = machine.RdSpdlName();
                    writeJsonToConsole(spindles);

                    dynamic spdl_op = machine.RdOpMode();
                    writeJsonToConsole(spdl_op);

                    dynamic spindles_load = machine.RdSpMeter();
                    writeJsonToConsole(spindles_load);

                    dynamic spindles_speed_act = machine.Acts();
                    writeJsonToConsole(spindles_speed_act);

                    dynamic spindles_speed_act2 = machine.Acts2(1);
                    writeJsonToConsole(spindles_speed_act2);

                    dynamic spindle_feed_com = machine.Modal(107, 1, 4);
                    writeJsonToConsole(spindle_feed_com);

                    dynamic for1 = machine.RdPmcRng(0, 0, 10, 11, 10, 1);
                    writeJsonToConsole(for1);

                    dynamic for2 = machine.RdPmcRng(0, 0, 10, 10, 9, 0);
                    writeJsonToConsole(for2);

                    dynamic for3 = machine.RdPmcRng(0, 0, 96, 96, 9, 0);
                    writeJsonToConsole(for3);

                    dynamic for4 = machine.RdPmcRng(0, 0, 14, 14, 9, 0);
                    writeJsonToConsole(for4);

                    dynamic for5 = machine.RdPmcRng(0, 0, 30, 30, 9, 0);
                    writeJsonToConsole(for5);

                    dynamic macro = machine.RdMacro(1, 10);
                    writeJsonToConsole(macro);

                    dynamic part_count = machine.RdParam(6711, 0, 8, 1);
                    writeJsonToConsole(part_count);
                }

                dynamic disconnect = machine.Disconnect();
                writeJsonToConsole(disconnect);
            }
            else
            {
                Console.WriteLine("connect failed");
            }

            Console.ReadKey();
        }
    }
}

            
            /*
            var parse_connect = new fanuc.veneers.Connect();
            parse_connect.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);

            var parse_sysinfo = new fanuc.veneers.SysInfo("sys_info");
            parse_sysinfo.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            parse_sysinfo.OnError = (input) => error_print(input);
            
            var parse_statinfo = new fanuc.veneers.StatInfo("stat_info");
            parse_statinfo.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            parse_statinfo.OnError = (input) => error_print(input);
            
            var parse_getpath = new fanuc.veneers.GetPath("get_path");
            parse_getpath.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            parse_getpath.OnError = (input) => error_print(input);
            
            var parse_block = new fanuc.veneers.Block("code_block");
            parse_block.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            parse_block.OnError = (input) => error_print(input);

            var parse_part_count = new fanuc.veneers.RdParamLData("part_count");
            parse_part_count.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            parse_part_count.OnError = (input) => error_print(input);

            var parse_axis_data_X = new fanuc.veneers.RdDynamic2(("axis_data_X"));
            parse_axis_data_X.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            parse_axis_data_X.OnError = (input) => error_print(input);
            
            //var parse_axis_data_Y = new fanuc.veneers.RdDynamic2(("axis_data_y"));
            //parse_axis_data_Y.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            //parse_axis_data_Y.OnError = (input) => error_print(input);
            
            //var parse_axis_data_Z = new fanuc.veneers.RdDynamic2(("axis_data_Z"));
            //parse_axis_data_Z.OnChange = (delta, input, note, data) => change_print(delta, input, note, data);
            //parse_axis_data_Z.OnError = (input) => error_print(input);
            */
            
            /*
            Action<TimeSpan, dynamic, string, dynamic> change_print = (delta, input, note, data) =>
            {
                Console.WriteLine(DateTime.UtcNow + "::delta=" + delta + "::method=" + input.method + "::note=" + note + "::data=" + data);
            };

            Action<dynamic> error_print = (input) =>
            {
                Console.WriteLine(JObject.FromObject(input).ToString());
            };
            */