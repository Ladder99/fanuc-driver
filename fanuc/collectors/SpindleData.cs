using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class SpindleData : FanucCollector
    {
        public SpindleData(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
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
                        _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                        
                        dynamic paths = await _machine["platform"].GetPathAsync();

                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        _machine.SliceVeneer(path_slices.ToArray());

                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                        
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic path = await _machine["platform"].SetPathAsync(current_path);
                            
                            dynamic spindles = await _machine["platform"].RdSpdlNameAsync();
                            dynamic axis_spindle_slices = new List<dynamic> { };
                            
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
                dynamic connect = await _machine["platform"].ConnectAsync();
                await _machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    dynamic paths = await _machine["platform"].GetPathAsync();
                    await _machine.PeelVeneerAsync("get_path", paths);

                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = await _machine["platform"].SetPathAsync(current_path);
                        dynamic path_marker = new {path.request.cnc_setpath.path_no};
                        _machine.MarkVeneer(current_path, path_marker);

                        dynamic info = await _machine["platform"].SysInfoAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path,"sys_info", info);
                        
                        dynamic spindles = await _machine["platform"].RdSpdlNameAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "spindle_name", spindles);

                        // main spindle displayed in cnc position screen
                        // speed RPM,mm/rev... and feed mm/min...
                        //dynamic speed_feed = await _machine["platform"].RdSpeedAsync(0);
                        //dynamic speed_speed = await _machine["platform"].RdSpeedAsync(1);
                        dynamic speed_all = await _machine["platform"].RdSpeedAsync(-1);

                        // TODO: does not work
                        //dynamic spindles_data = await _machine["platform"].Acts2Async(-1);
                        
                        // load % and speed RPM
                        //dynamic load_meter_all = await _machine["platform"].RdSpMeterAsync(0, spindles.response.cnc_rdspdlname.data_num);
                        //dynamic motor_speed_all = await _machine["platform"].RdSpMeterAsync(1, spindles.response.cnc_rdspdlname.data_num);
                        //dynamic meter_all = await _machine["platform"].RdSpMeterAsync(-1, spindles.response.cnc_rdspdlname.data_num);
                        
                        // TODO: does not work
                        //dynamic spload_all = await _machine["platform"].RdSpLoadAsync(-1);
                        
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
                            
                            // rotational spindle speed
                            dynamic spindle_data = await _machine["platform"].Acts2Async(current_spindle);
                            await _machine.PeelAcrossVeneerAsync(new[] { current_path, spindle_name }, "spindle_data", spindle_data);
                            
                            //dynamic load_meter = await _machine["platform"].RdSpMeterAsync(0, current_spindle);
                            //dynamic motor_speed = await _machine["platform"].RdSpMeterAsync(1, current_spindle);
                            dynamic meter = await _machine["platform"].RdSpMeterAsync(-1, current_spindle);
                            
                            // TODO create veneer
                            // for single spindle machine
                            //      veneer = RdSpeed + RdSpMeter
                            // for multi spindle machine
                            //      sp 1: veneer = RdSpeed (speed, feed) + RdSpMeter (speed, load)
                            //      sp n: veneer = RdSpMeter (speed, load) (no feed)
                            
                            // not sure what units the response data is
                            //dynamic spload = await _machine["platform"].RdSpLoadAsync(current_spindle);
                        };
                    }

                    dynamic disconnect = await _machine["platform"].DisconnectAsync();
                    
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