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
        public Basic03(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!machine.VeneersApplied)
                {
                    dynamic connect = await machine["platform"].ConnectAsync();
                    
                    if (connect.success)
                    {
                        machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                        machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                        machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                        machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                        
                        dynamic paths = await machine["platform"].GetPathAsync();

                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        machine.SliceVeneer(path_slices.ToArray());

                        machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                        machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                        machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                        
                        // below we will be adding axis specific observations
                        // axes are children of a path
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            // set the current path
                            dynamic path = await machine["platform"].SetPathAsync(current_path);
                            // so that we can retrieve the axis names on that path
                            dynamic axes = await machine["platform"].RdAxisNameAsync();
                            // Focas API returns a struct of axis names (e.g. data1,data2,data3,...)
                            dynamic axis_slices = new List<dynamic> { };
                            // use reflection to walk all available axes
                            var fields = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                            for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                            {
                                var axis = fields[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                                // capture each axis name so we can use it to slice each path
                                //  e.g. X1,Y1,Z1,A1
                                axis_slices.Add(((char) axis.name).AsAscii() +
                                                ((char) axis.suff).AsAscii());
                            }
                            
                            // now slice the current path's veneers using the axis names
                            machine.SliceVeneer(current_path, axis_slices.ToArray());
                            // apply the 'axis_data' observation to the current path and its axes
                            machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                        }
                        
                        dynamic disconnect = await machine["platform"].DisconnectAsync();
                        
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
                dynamic connect = await machine["platform"].ConnectAsync();
                await machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    dynamic cncid = await machine["platform"].CNCIdAsync();
                    await machine.PeelVeneerAsync("cnc_id", cncid);
                    
                    dynamic poweron = await machine["platform"].RdTimerAsync(0);
                    await machine.PeelVeneerAsync("power_on_time", poweron);
                    
                    dynamic poweron_6750 = machine["platform"].RdParamDoubleWordNoAxisAsync(6750);
                    await machine.PeelVeneerAsync("power_on_time_6750", poweron_6750);
                    
                    dynamic paths = await machine["platform"].GetPathAsync();
                    await machine.PeelVeneerAsync("get_path", paths);

                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = await machine["platform"].SetPathAsync(current_path);
                        dynamic path_marker = new {path.request.cnc_setpath.path_no};
                        machine.MarkVeneer(current_path, path_marker);

                        dynamic info = await machine["platform"].SysInfoAsync();
                        await machine.PeelAcrossVeneerAsync(current_path,"sys_info", info);

                        dynamic axes = await machine["platform"].RdAxisNameAsync();
                        await machine.PeelAcrossVeneerAsync(current_path, "axis_name", axes);

                        dynamic spindles = await machine["platform"].RdSpdlNameAsync();
                        await machine.PeelAcrossVeneerAsync(current_path, "spindle_name", spindles);
                        
                        var fields = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                        
                        for (short current_axis = 1;
                            current_axis <= axes.response.cnc_rdaxisname.data_num;
                            current_axis++)
                        {
                            dynamic axis = fields[current_axis-1].GetValue(axes.response.cnc_rdaxisname.axisname);
                            dynamic axis_name = ((char) axis.name).AsAscii() + ((char) axis.suff).AsAscii();
                            dynamic axis_marker = new
                            {
                                name = ((char)axis.name).AsAscii(), 
                                suff = ((char)axis.suff).AsAscii()
                            };
                            
                            // mark the current path and axis
                            //  path 1      1, X1;  1, Y1;  1, Z1;  1, A1;
                            //  path 2      2, B1;
                            // with a descriptive marker
                            //  { path_no = 1}, { name = X, suff = 1 }
                            //  ...
                            //  { path_no = 2}, { name = B, suff = 1 }
                            machine.MarkVeneer(new[] { current_path, axis_name }, new[] { path_marker, axis_marker });
                            // retrieve axis position data for the current path and current axis
                            dynamic axis_data = await machine["platform"].RdDynamic2Async(current_axis, 44, 2);
                            // reveal the 'axis_data' observation for the current path and current axis
                            await machine.PeelAcrossVeneerAsync(new[] { current_path, axis_name }, "axis_data", axis_data);
                        }
                    }

                    dynamic disconnect = await machine["platform"].DisconnectAsync();

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