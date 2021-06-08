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
                    dynamic connect = await _platform.ConnectAsync();
                    
                    if (connect.success)
                    {
                        _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                        
                        dynamic paths = await _platform.GetPathAsync();

                        IEnumerable<int> path_slices = Enumerable
                            .Range(paths.response.cnc_getpath.path_no, paths.response.cnc_getpath.maxpath_no);

                        _machine.SliceVeneer(path_slices.ToArray());

                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.SysInfo), "sys_info");
                        _machine.ApplyVeneerAcrossSlices(typeof(fanuc.veneers.RdSpindlename), "spindle_name");
                        
                        for (short current_path = paths.response.cnc_getpath.path_no;
                            current_path <= paths.response.cnc_getpath.maxpath_no;
                            current_path++)
                        {
                            dynamic path = await _platform.SetPathAsync(current_path);
                            
                            dynamic spindles = await _platform.RdSpdlNameAsync();
                            dynamic axis_spindle_slices = new List<dynamic> { };
                            
                            var fields_spindles = spindles.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                            for (int x = 0; x <= spindles.response.cnc_rdspdlname.data_num - 1; x++)
                            {
                                var spindle = fields_spindles[x].GetValue(spindles.response.cnc_rdspdlname.spdlname);
                                axis_spindle_slices.Add(SpindleName(spindle));
                            };

                            _machine.SliceVeneer(current_path, axis_spindle_slices.ToArray());

                            _machine.ApplyVeneerAcrossSlices(current_path, typeof(fanuc.veneers.SpindleData), "spindle_data");
                        }
                        
                        dynamic disconnect = await _platform.DisconnectAsync();
                        
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
                dynamic connect = await _platform.ConnectAsync();
                await _machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    dynamic paths = await _platform.GetPathAsync();
                    await _machine.PeelVeneerAsync("get_path", paths);

                    for (short current_path = paths.response.cnc_getpath.path_no;
                        current_path <= paths.response.cnc_getpath.maxpath_no;
                        current_path++)
                    {
                        dynamic path = await _platform.SetPathAsync(current_path);
                        dynamic path_marker = PathMarker(path);
                        
                        _machine.MarkVeneer(current_path, path_marker);

                        dynamic info = await _platform.SysInfoAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path,"sys_info", info);
                        
                        dynamic spindles = await _platform.RdSpdlNameAsync();
                        await _machine.PeelAcrossVeneerAsync(current_path, "spindle_name", spindles);

                        // main spindle displayed in cnc position screen
                        // speed RPM,mm/rev... and feed mm/min...
                        //dynamic speed_feed = await _machine["platform"].RdSpeedAsync(0);
                        //dynamic speed_speed = await _machine["platform"].RdSpeedAsync(1);
                        dynamic sp_speed = await _platform.RdSpeedAsync(-1);

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
                            dynamic spindle_name = SpindleName(spindle);
                            dynamic spindle_marker = SpindleMarker(spindle);
                            dynamic spindle_split = new[] {current_path, spindle_name};
                            
                            _machine.MarkVeneer(spindle_split, new[] { path_marker, spindle_marker });
                            
                            // rotational spindle speed
                            dynamic sp_acts = await _platform.Acts2Async(current_spindle);
                            
                            //dynamic load_meter = await _machine["platform"].RdSpMeterAsync(0, current_spindle);
                            //dynamic motor_speed = await _machine["platform"].RdSpMeterAsync(1, current_spindle);
                            dynamic sp_meter = await _platform.RdSpMeterAsync(-1, current_spindle);

                            // max rpm ratio
                            dynamic sp_maxrpm = await _platform.RdSpMaxRpmAsync(current_spindle);

                            // spindle gear ratio
                            dynamic sp_gear = await _platform.RdSpGear(current_spindle);
                            
                            // not sure what units the response data is
                            //dynamic spload = await _machine["platform"].RdSpLoadAsync(current_spindle);
                            
                            // TODO create veneer
                            // for single spindle machine
                            //      veneer = RdSpeed + RdSpMeter
                            // for multi spindle machine
                            //      sp 1: veneer = RdSpeed (speed, feed) + RdSpMeter (speed, load)
                            //      sp n: veneer = RdSpMeter (speed, load) (no feed)
                            
                            // diagnose values
                            // 400 bit 7 = LNK/1    comms with spindle control established
                            dynamic diag_lnk = await _platform.DiagnossByteNoAxisAsync(400);
                            
                            // 403 byte             temp of winding spindle motor (C) 0-255
                            dynamic diag_temp = await _platform.DiagnossByteNoAxisAsync(403);
                            
                            // 408                  spindle comms (causes of SP0749)
                            //  0 CRE               CRC 
                            //  1 FRE               Framing
                            //  2 SNE               sender/receiver
                            //  3 CER               reception
                            //  4 CME               no response
                            //  5 SCA               comm amplifier alarm
                            //  7 SSA               spindle amplifier alarm
                            dynamic diag_comms = await _platform.DiagnossByteNoAxisAsync(408);
                            
                            // 410 word             spindle load (%)
                            dynamic diag_load_perc = await _platform.DiagnossWordNoAxisAsync(410);
                            
                            // 411 word             spindle load (min)
                            dynamic diag_load_min = await _platform.DiagnossWordNoAxisAsync(410);
                            
                            // 417 dword            spindle position coder feedback (detection units)
                            dynamic diag_coder = await _platform.DiagnossDoubleWordNoAxisAsync(417);
                            
                            // 418 dword            position loop deviation (detection units)
                            dynamic diag_loop_dev = await _platform.DiagnossDoubleWordNoAxisAsync(418);
                            
                            // 425 dword            sync error (detection unit)
                            dynamic diag_sync_error = await _platform.DiagnossDoubleWordNoAxisAsync(425);
                            
                            // 445 word             position data (pulse) 0-4095 , valid only when param3117=1
                            dynamic diag_pos_data = await _platform.DiagnossWordNoAxisAsync(445);
                            
                            // 710 word             spindle error
                            dynamic diag_error = await _platform.DiagnossWordNoAxisAsync(710);
                            
                            // 711 word             spindle warning
                            dynamic diag_warn = await _platform.DiagnossWordNoAxisAsync(711);
                            
                            // 1520 dword           spindle rev count 1 (1000 min)
                            dynamic diag_rev_1 = await _platform.DiagnossDoubleWordNoAxisAsync(1520);
                            
                            // 1521 dword           spindle rev count 2 (1000 min)
                            dynamic diag_rev_2 = await _platform.DiagnossDoubleWordNoAxisAsync(1521);
                            
                            await _machine.PeelAcrossVeneerAsync(spindle_split, "spindle_data", 
                                current_spindle,
                                spindles, 
                                sp_speed, 
                                sp_meter, 
                                sp_maxrpm, 
                                sp_gear,
                                diag_lnk,
                                diag_temp,
                                diag_comms,
                                diag_load_perc,
                                diag_load_min,
                                diag_coder,
                                diag_loop_dev,
                                diag_sync_error,
                                diag_pos_data,
                                diag_error,
                                diag_warn,
                                diag_rev_1,
                                diag_rev_2);
                        };
                    }

                    dynamic disconnect = await _platform.DisconnectAsync();
                    
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