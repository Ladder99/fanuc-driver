using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic05 : FanucCollector
    {
        private Stopwatch _sweepWatch = new Stopwatch();
        
        public Basic05(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!_machine.VeneersApplied)
                {
                    dynamic connect = await _machine["platform"].ConnectAsync();
                    
                    if (connect.success)
                    {
                        // let's add a custom internal veneer used to measure the health of our controller connection
                        _machine.ApplyVeneer(typeof(fanuc.veneers.FocasPerf), "focas_perf", true);
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
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.StatInfo), "stat_info");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdAxisname), "axis_name");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                        
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic path = await _machine["platform"].SetPathAsync(current_path);
                            
                            dynamic axes = await _machine["platform"].RdAxisNameAsync();
                            dynamic spindles = await _machine["platform"].RdSpdlNameAsync();
                            dynamic axis_spindle_slices = new List<dynamic> { };

                            var fields_axes = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();
                            for (int x = 0; x <= axes.response.cnc_rdaxisname.data_num - 1; x++)
                            {
                                var axis = fields_axes[x].GetValue(axes.response.cnc_rdaxisname.axisname);
                                axis_spindle_slices.Add(((char) axis.name).AsAscii() +
                                                        ((char) axis.suff).AsAscii());
                            }
                            
                            var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                            for (int x = 0; x <= spindles.response.cnc_rdspdlname.data_num - 1; x++)
                            {
                                var spindle = fields_spindles[x].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                                axis_spindle_slices.Add(((char) spindle.name).AsAscii() +
                                                        ((char) spindle.suff1).AsAscii() +
                                                        ((char) spindle.suff2).AsAscii() +
                                                        ((char) spindle.suff3).AsAscii());
                            };

                            _machine.SliceVeneer(current_path, axis_spindle_slices.ToArray());

                            _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdDynamic2), "axis_data");
                            _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.RdActs2), "spindle_data");
                        }
                        
                        dynamic disconnect = await _machine["platform"].DisconnectAsync();
                        
                        _machine.VeneersApplied = true;
                    }
                    else
                    {
                        await Task.Delay(_sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector initialization failed.");
            }

            return null;
        }

        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                // add code to trap each Focas API invocation method name, its round trip time, and result code
                _sweepWatch.Restart();

                dynamic focas_invocations = new List<dynamic>();
                
                Action<dynamic> catch_focas_perf = (ret) =>
                {
                    focas_invocations.Add(new
                    {
                        ret.method,
                        ret.invocationMs,
                        ret.rc
                    });
                };
                
                dynamic connect = await _machine["platform"].ConnectAsync();
                await _machine.PeelVeneerAsync("connect", connect);
                // now for every Focas API call we make, add its metrics to our list which we process at the end of the sweep
                catch_focas_perf(connect);

                if (connect.success)
                {
                    dynamic cncid = await _machine["platform"].CNCIdAsync();
                    await _machine.PeelVeneerAsync("cnc_id", cncid);
                    catch_focas_perf(cncid);
                    
                    dynamic poweron = await _machine["platform"].RdTimerAsync(0);
                    await _machine.PeelVeneerAsync("power_on_time", poweron);
                    catch_focas_perf(poweron);
                    
                    dynamic poweron_6750 = await _machine["platform"].RdParamAsync(6750, 0, 8, 1);
                    await _machine.PeelVeneerAsync("power_on_time_6750", poweron_6750);
                    catch_focas_perf(poweron_6750);
                    
                    dynamic paths = await _machine["platform"].GetPathAsync();
                    await _machine.PeelVeneerAsync("get_path", paths);
                    catch_focas_perf(paths);

                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = await _machine["platform"].SetPathAsync(current_path);
                        dynamic path_marker = new {path.request.cnc_setpath.path_no};
                        _machine.MarkVeneer(current_path, path_marker);
                        catch_focas_perf(path);

                        dynamic info = await _machine["platform"].SysInfoAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path,"sys_info", info);
                        catch_focas_perf(info);
                        
                        dynamic stat = await _machine["platform"].StatInfoAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "stat_info", stat);
                        catch_focas_perf(path);
                        
                        dynamic axes = await _machine["platform"].RdAxisNameAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "axis_name", axes);
                        catch_focas_perf(axes);

                        dynamic spindles = await _machine["platform"].RdSpdlNameAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "spindle_name", spindles);
                        catch_focas_perf(spindles);
                        
                        var fields_axes = axes.response.cnc_rdaxisname.axisname.GetType().GetFields();

                        for (short current_axis = 1;
                            current_axis <= axes.response.cnc_rdaxisname.data_num;
                            current_axis++)
                        {
                            dynamic axis = fields_axes[current_axis-1].GetValue(axes.response.cnc_rdaxisname.axisname);
                            dynamic axis_name = ((char) axis.name).AsAscii() + ((char) axis.suff).AsAscii();
                            dynamic axis_marker = new
                            {
                                name = ((char)axis.name).AsAscii(), 
                                suff =  ((char)axis.suff).AsAscii()
                            };
                            
                            _machine.MarkVeneer(new[] { current_path, axis_name }, new[] { path_marker, axis_marker });
                            
                            dynamic axis_data = await _machine["platform"].RdDynamic2Async(current_axis, 44, 2);
                            await _machine.PeelAcrossVeneerAsync(new[] { current_path, axis_name }, "axis_data", axis_data);
                            catch_focas_perf(axis_data);
                        }
                        
                        var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                        
                        for (short current_spindle = 1;
                            current_spindle <= spindles.response.cnc_rdspdlname.data_num;
                            current_spindle++)
                        {
                            var spindle = fields_spindles[current_spindle - 1].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                            dynamic spindle_name = ((char) spindle.name).AsAscii() +
                                                    ((char) spindle.suff1).AsAscii() +
                                                    ((char) spindle.suff2).AsAscii() +
                                                    ((char) spindle.suff3).AsAscii();
                            dynamic spindle_marker = new
                            {
                                name = ((char)spindle.name).AsAscii(), 
                                suff1 =  ((char)spindle.suff1).AsAscii(),
                                suff2 =  ((char)spindle.suff2).AsAscii(),
                                suff3 =  ((char)spindle.suff3).AsAscii()
                            };
                            
                            _machine.MarkVeneer(new[] { current_path, spindle_name }, new[] { path_marker, spindle_marker });
                            
                            dynamic spindle_data = await _machine["platform"].Acts2Async(current_spindle);
                            await _machine.PeelAcrossVeneerAsync(new[] { current_path, spindle_name }, "spindle_data", spindle_data);
                            catch_focas_perf(spindle_data);
                        };
                    }

                    dynamic disconnect = await _machine["platform"].DisconnectAsync();
                    catch_focas_perf(disconnect);
                    // reveal the internal 'focas_perf' observation
                    // internal veneers and observations do not carry Focas API metadata and should be treated as such
                    //  that's what the IsInternal field is for
                    await _machine.PeelVeneerAsync("focas_perf", new
                    {
                        sweepMs = _sweepWatch.ElapsedMilliseconds,
                        focas_invocations
                    });
                    
                    LastSuccess = connect.success;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}