using System;
using System.Collections.Generic;
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
        private static fanuc.Machines _machines = new fanuc.Machines();
        
        static void Main(string[] args)
        {
            _machines.Add(true, "tempco", "172.16.13.100", 8193, 2);
            _machines.Add(false, "sim", "10.20.30.101", 8193, 2);
            
            createVeneers();
            processVeneers();
        }

        static void writeJsonToConsole(dynamic d)
        {
            Console.WriteLine(JObject.FromObject(d).ToString());
        }

        static void createVeneers()
        {
            foreach (var machine in _machines[null])
            {
                Console.WriteLine(machine);
                
                while (!machine.VeneersCreated)
                {
                    Console.WriteLine("fanuc - creating veneers");

                    dynamic connect = machine.Platform.Connect();
                    writeJsonToConsole(connect);

                    if (connect.success)
                    {
                        machine.AddVeneer(typeof(fanuc.veneers.Connect), "connect");
                        machine.AddVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                        machine.AddVeneer(typeof(fanuc.veneers.GetPath), "get_path");

                        dynamic paths = machine.Platform.GetPath();
                        writeJsonToConsole(paths);

                        IEnumerable<int> slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        machine.SliceVeneer(slices.ToArray());

                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.StatInfo), "stat_info");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.Alarms), "alarms");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.Block), "code_block");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.RdParamLData), "part_count");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.RdDynamic2), "axis_data_X");

                        /*
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic axes = _machine.RdAxisName();
                            writeJsonToConsole(axes);
                        }
                        */
                        
                        dynamic disconnect = machine.Platform.Disconnect();
                        machine.VeneersCreated = true;

                        Console.WriteLine("fanuc - created veneers");
                    }
                    else
                    {
                        // not in here
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }
        
        static void processVeneers()
        {
            while (true)
            {
                foreach (var machine in _machines[null])
                {
                    dynamic connect = machine.Platform.Connect();
                    machine.PeelVeneer("connect", connect);

                    if (connect.success)
                    {
                        dynamic info = machine.Platform.SysInfo();
                        machine.PeelVeneer("sys_info", info);

                        dynamic paths = machine.Platform.GetPath();
                        machine.PeelVeneer("get_path", paths);

                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic path = machine.Platform.SetPath(current_path);
                            dynamic path_marker = new {path.request.cnc_setpath.path_no};
                            machine.MarkVeneer(current_path, path_marker);

                            dynamic stat = machine.Platform.StatInfo();
                            machine.PeelAcrossVeneer(current_path, "stat_info", stat);

                            dynamic part_count = machine.Platform.RdParam(6711, 0, 8, 1);
                            machine.PeelAcrossVeneer(current_path, "part_count", part_count);

                            dynamic prog = machine.Platform.RdExecProg(1024);
                            machine.PeelAcrossVeneer(current_path, "code_block", prog);

                            dynamic axis_data_x = machine.Platform.RdDynamic2(1, 44);
                            machine.PeelAcrossVeneer(current_path, "axis_data_X", axis_data_x);

                            dynamic alms = machine.Platform.RdAlmMsgAll();
                            machine.PeelAcrossVeneer(current_path, "alarms", alms);

                            //dynamic axis_data_y = machine.RdDynamic2(2, 44);
                            //parse_axis_data_Y.Peel(axis_data_y);

                            //dynamic axis_data_z = machine.RdDynamic2(3, 44);
                            //parse_axis_data_Z.Peel(axis_data_z);

                            //dynamic axis_data = machine.RdDynamic2(-1, 44);
                        }

                        dynamic disconnect = machine.Platform.Disconnect();
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}      