using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using fanuc.veneers;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace fanuc
{
    class Program
    {
        private static IMqttClient _mqtt;
        private static fanuc.Machines _machines = new fanuc.Machines();
        
        static void Main(string[] args)
        {
            var factory = new MqttFactory();
            var options = new MqttClientOptionsBuilder()
                //.WithTcpServer("172.16.10.3")
                .WithTcpServer("10.20.30.102")
                .Build();
            _mqtt = factory.CreateMqttClient();
            var r = _mqtt.ConnectAsync(options).Result;
            
            Action<Veneers, Veneer> on_data_change = (vv, v) =>
            {
                dynamic payload = new
                {
                    observation = new
                    {
                        time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        machine = vv.Machine.Id,
                        name = v.Name,
                        marker = v.Marker
                    },
                    source = new
                    {
                        method = v.LastInput.method,
                        data = v.LastInput.request.GetType().GetProperty(v.LastInput.method).GetValue(v.LastInput.request, null)
                    },
                    delta = new
                    {
                        time = v.ChangeDelta,
                        data = v.DataDelta
                    }
                };

                var topic = $"fanuc/{vv.Machine.Id}/{v.Name}{(v.SliceKey == null ? string.Empty : "/" + v.SliceKey.ToString())}";
                var payload_string = JObject.FromObject(payload).ToString();
                
                Console.WriteLine(topic);
                Console.WriteLine(payload_string);
                Console.WriteLine();
                
                var msg = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload_string)
                    .WithRetainFlag()
                    .Build();
                var r = _mqtt.PublishAsync(msg, CancellationToken.None);
            };

            Action<Veneers, Veneer> on_error = (vv, v) =>
            {
                //Console.WriteLine(new { method = v.LastInput.method, rc = v.LastInput.rc });
            };
            
            var m1 = _machines.Add(false, "naka", "172.16.13.100", 8193, 2);
            m1.Veneers.OnDataChange = on_data_change;
            m1.Veneers.OnError = on_error;
            
            var m2 = _machines.Add(true, "sim", "10.20.30.101", 8193, 2);
            m2.Veneers.OnDataChange = on_data_change;
            m2.Veneers.OnError = on_error;
            
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

                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        machine.SliceVeneer(path_slices.ToArray());

                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.StatInfo), "stat_info");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.Alarms), "alarms");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.OpMsgs), "op_msgs");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.Block), "code_block");
                        machine.AddVeneerAcrossSlices(typeof(fanuc.veneers.RdParamLData), "part_count");
                        
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic axes = machine.Platform.RdAxisName();
                            dynamic axis_slices = new List<dynamic> { };
                            
                            var fields = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                            for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                            {
                                var axis = fields[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                                axis_slices.Add(((char) axis.name).ToString().Trim('\0') +
                                                ((char) axis.suff).ToString().Trim('\0'));
                            }
                            
                            machine.SliceVeneer(current_path, axis_slices.ToArray());
                            
                            machine.AddVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                        }
                        
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
                            dynamic path_marker = new { path.request.cnc_setpath.path_no };
                            machine.MarkVeneer(current_path, path_marker);

                            //dynamic tool = machine.Platform.Modal(108, 1, 3);
                            //writeJsonToConsole(tool);
                            
                            dynamic stat = machine.Platform.StatInfo();
                            machine.PeelAcrossVeneer(current_path, "stat_info", stat);
                            
                            dynamic opmsgs = machine.Platform.RdOpMsg();
                            machine.PeelAcrossVeneer(current_path, "op_msgs", opmsgs);
                            
                            dynamic alms = machine.Platform.RdAlmMsgAll();
                            machine.PeelAcrossVeneer(current_path, "alarms", alms);

                            dynamic part_count = machine.Platform.RdParam(6712, 0, 8, 1);
                            machine.PeelAcrossVeneer(current_path, "part_count", part_count);

                            dynamic prog = machine.Platform.RdExecProg(512);
                            machine.PeelAcrossVeneer(current_path, "code_block", prog);
                            
                            dynamic axes = machine.Platform.RdAxisName();
                            machine.PeelAcrossVeneer(current_path, "axis_name", axes);
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
                                
                                machine.MarkVeneer(new[] { current_path, axis_name }, new[] { path_marker, axis_marker });
                                
                                dynamic axis_data = machine.Platform.RdDynamic2(current_axis, 44, 2);
                                machine.PeelAcrossVeneer(new[] { current_path, axis_name }, "axis_data", axis_data);
                            }
                        }

                        dynamic disconnect = machine.Platform.Disconnect();
                    }
                    
                    dynamic payload = new
                    {
                        observation = new
                        {
                            time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                            machine = machine.Id,
                            name = "PING"
                        },
                        source = new
                        {
                            data = machine.Info
                        },
                        delta = new
                        {
                            data = connect.success ? "OK" : "NOK"
                        }
                    };
                        
                    var msg = new MqttApplicationMessageBuilder()
                        .WithTopic($"fanuc/{machine.Id}/PING")
                        .WithPayload(JObject.FromObject(payload).ToString())
                        .WithRetainFlag()
                        .Build();
                    var r = _mqtt.PublishAsync(msg, CancellationToken.None).Result;
                }
                
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}      