using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class SpindleData : FanucCollector2
    {
        public SpindleData(Machine machine, int sweepMs = 1000, params dynamic[] additional_params) : base(machine, sweepMs, additional_params)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            await apply(typeof(fanuc.veneers.CNCId), "cnc_id");
            
            await apply(typeof(fanuc.veneers.RdParamLData), "power_on_time");
        }
        
        public override async Task InitPathsAsync()
        {
            await apply(typeof(fanuc.veneers.SysInfo), "sys_info");
            
            await apply(typeof(fanuc.veneers.StatInfoText), "stat_info");
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            await apply(typeof(fanuc.veneers.SpindleData), "spindle_data", is_compound: true);
        }
        
        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        public override async Task CollectRootAsync()
        {
            await set_native_and_peel("cnc_id", await _platform.CNCIdAsync());
                    
            await set_native_and_peel("power_on_time", await _platform.RdParamDoubleWordNoAxisAsync(6750));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await set_native_and_peel("sys_info", await _platform.SysInfoAsync());
                        
            await set_native_and_peel("stat_info", await _platform.StatInfoAsync());
            
            // main spindle displayed in cnc position screen
            // speed RPM,mm/rev... and feed mm/min...
            //dynamic speed_feed = await _machine["platform"].RdSpeedAsync(0);
            //dynamic speed_speed = await _machine["platform"].RdSpeedAsync(1);
            await set_native("sp_speed", await _platform.RdSpeedAsync(-1));

            // TODO: does not work
            //dynamic spindles_data = await _machine["platform"].Acts2Async(-1);
                        
            // load % and speed RPM
            //dynamic load_meter_all = await _machine["platform"].RdSpMeterAsync(0, spindles.response.cnc_rdspdlname.data_num);
            //dynamic motor_speed_all = await _machine["platform"].RdSpMeterAsync(1, spindles.response.cnc_rdspdlname.data_num);
            //dynamic meter_all = await _machine["platform"].RdSpMeterAsync(-1, spindles.response.cnc_rdspdlname.data_num);
                        
            // TODO: does not work
            //dynamic spload_all = await _machine["platform"].RdSpLoadAsync(-1);
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            // rotational spindle speed
            await set_native("sp_acts", await _platform.Acts2Async(current_spindle));
            
            //dynamic load_meter = await _machine["platform"].RdSpMeterAsync(0, current_spindle);
            //dynamic motor_speed = await _machine["platform"].RdSpMeterAsync(1, current_spindle);
            await set_native("sp_meter", await _platform.RdSpMeterAsync(-1, current_spindle));

            // max rpm ratio
            await set_native("sp_maxrpm", await _platform.RdSpMaxRpmAsync(current_spindle));

            // spindle gear ratio
            await set_native("sp_gear", await _platform.RdSpGearAsync(current_spindle));
            
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
            await set_native("diag_lnk", await _platform.DiagnossByteFirstAxisAsync(400));
            
            // 403 byte             temp of winding spindle motor (C) 0-255
            await set_native("diag_temp", await _platform.DiagnossByteFirstAxisAsync(403));
            
            // 408                  spindle comms (causes of SP0749)
            //  0 CRE               CRC 
            //  1 FRE               Framing
            //  2 SNE               sender/receiver
            //  3 CER               reception
            //  4 CME               no response
            //  5 SCA               comm amplifier alarm
            //  7 SSA               spindle amplifier alarm
            await set_native("diag_comms", await _platform.DiagnossByteFirstAxisAsync(408));
            
            // 410 word             spindle load (%)
            await set_native("diag_load_perc", await _platform.DiagnossWordFirstAxisAsync(410));
            
            // 411 word             spindle load (min)
            await set_native("diag_load_min", await _platform.DiagnossWordFirstAxisAsync(411));
            
            // 417 dword            spindle position coder feedback (detection units)
            await set_native("diag_coder", await _platform.DiagnossDoubleWordFirstAxisAsync(417));
            
            // 418 dword            position loop deviation (detection units)
            await set_native("diag_loop_dev", await _platform.DiagnossDoubleWordFirstAxisAsync(418));
            
            // 425 dword            sync error (detection unit)
            await set_native("diag_sync_error", await _platform.DiagnossDoubleWordFirstAxisAsync(425));
            
            // 445 word             position data (pulse) 0-4095 , valid only when param3117=1
            await set_native("diag_pos_data", await _platform.DiagnossWordFirstAxisAsync(445));
            
            // 710 word             spindle error
            await set_native("diag_error", await _platform.DiagnossWordFirstAxisAsync(710));
            
            // 711 word             spindle warning
            await set_native("diag_warn", await _platform.DiagnossWordFirstAxisAsync(711));
            
            // 1520 dword           spindle rev count 1 (1000 min)
            await set_native("diag_rev_1", await _platform.DiagnossDoubleWordFirstAxisAsync(1520));
            
            // 1521 dword           spindle rev count 2 (1000 min)
            await set_native("diag_rev_2", await _platform.DiagnossDoubleWordFirstAxisAsync(1521));
            
            await peel("spindle_data", 
                current_spindle,
                get("spindle_names"), 
                get("sp_speed"), 
                get("sp_meter"), 
                get("sp_maxrpm"), 
                get("sp_gear"),
                get("diag_lnk"),
                get("diag_temp"),
                get("diag_comms"),
                get("diag_load_perc"),
                get("diag_load_min"),
                get("diag_coder"),
                get("diag_loop_dev"),
                get("diag_sync_error"),
                get("diag_pos_data"),
                get("diag_error"),
                get("diag_warn"),
                get("diag_rev_1"),
                get("diag_rev_2"));
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}