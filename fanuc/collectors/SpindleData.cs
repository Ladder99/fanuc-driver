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
        public SpindleData(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            await Apply(typeof(fanuc.veneers.CNCId), "cnc_id");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "power_on_time");
        }
        
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.SysInfo), "sys_info");
            
            await Apply(typeof(fanuc.veneers.StatInfoText), "stat_info");
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            await Apply(typeof(fanuc.veneers.SpindleData), "spindle_data", isCompound: true);
        }
        
        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        public override async Task CollectRootAsync()
        {
            await SetNativeAndPeel("cnc_id", await platform.CNCIdAsync());
                    
            await SetNativeAndPeel("power_on_time", await platform.RdParamDoubleWordNoAxisAsync(6750));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await SetNativeAndPeel("sys_info", await platform.SysInfoAsync());
                        
            await SetNativeAndPeel("stat_info", await platform.StatInfoAsync());
            
            // main spindle displayed in cnc position screen
            // speed RPM,mm/rev... and feed mm/min...
            //dynamic speed_feed = await machine["platform"].RdSpeedAsync(0);
            //dynamic speed_speed = await machine["platform"].RdSpeedAsync(1);
            await SetNative("sp_speed", await platform.RdSpeedAsync(-1));

            // TODO: does not work
            //dynamic spindles_data = await machine["platform"].Acts2Async(-1);
                        
            // load % and speed RPM
            //dynamic load_meter_all = await machine["platform"].RdSpMeterAsync(0, spindles.response.cnc_rdspdlname.data_num);
            //dynamic motor_speed_all = await machine["platform"].RdSpMeterAsync(1, spindles.response.cnc_rdspdlname.data_num);
            //dynamic meter_all = await machine["platform"].RdSpMeterAsync(-1, spindles.response.cnc_rdspdlname.data_num);
                        
            // TODO: does not work
            //dynamic spload_all = await machine["platform"].RdSpLoadAsync(-1);
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            // rotational spindle speed
            await SetNative("sp_acts", await platform.Acts2Async(current_spindle));
            
            //dynamic load_meter = await machine["platform"].RdSpMeterAsync(0, current_spindle);
            //dynamic motor_speed = await machine["platform"].RdSpMeterAsync(1, current_spindle);
            await SetNative("sp_meter", await platform.RdSpMeterAsync(-1, current_spindle));

            // max rpm ratio
            await SetNative("sp_maxrpm", await platform.RdSpMaxRpmAsync(current_spindle));

            // spindle gear ratio
            await SetNative("sp_gear", await platform.RdSpGearAsync(current_spindle));
            
            // not sure what units the response data is
            //dynamic spload = await machine["platform"].RdSpLoadAsync(current_spindle);
            
            // TODO create veneer
            // for single spindle machine
            //      veneer = RdSpeed + RdSpMeter
            // for multi spindle machine
            //      sp 1: veneer = RdSpeed (speed, feed) + RdSpMeter (speed, load)
            //      sp n: veneer = RdSpMeter (speed, load) (no feed)
            
            // diagnose values
            // 400 bit 7 = LNK/1    comms with spindle control established
            await SetNative("diag_lnk", await platform.DiagnossByteFirstAxisAsync(400));
            
            // 403 byte             temp of winding spindle motor (C) 0-255
            await SetNative("diag_temp", await platform.DiagnossByteFirstAxisAsync(403));
            
            // 408                  spindle comms (causes of SP0749)
            //  0 CRE               CRC 
            //  1 FRE               Framing
            //  2 SNE               sender/receiver
            //  3 CER               reception
            //  4 CME               no response
            //  5 SCA               comm amplifier alarm
            //  7 SSA               spindle amplifier alarm
            await SetNative("diag_comms", await platform.DiagnossByteFirstAxisAsync(408));
            
            // 410 word             spindle load (%)
            await SetNative("diag_load_perc", await platform.DiagnossWordFirstAxisAsync(410));
            
            // 411 word             spindle load (min)
            await SetNative("diag_load_min", await platform.DiagnossWordFirstAxisAsync(411));
            
            // 417 dword            spindle position coder feedback (detection units)
            await SetNative("diag_coder", await platform.DiagnossDoubleWordFirstAxisAsync(417));
            
            // 418 dword            position loop deviation (detection units)
            await SetNative("diag_loop_dev", await platform.DiagnossDoubleWordFirstAxisAsync(418));
            
            // 425 dword            sync error (detection unit)
            await SetNative("diag_sync_error", await platform.DiagnossDoubleWordFirstAxisAsync(425));
            
            // 445 word             position data (pulse) 0-4095 , valid only when param3117=1
            await SetNative("diag_pos_data", await platform.DiagnossWordFirstAxisAsync(445));
            
            // 710 word             spindle error
            await SetNative("diag_error", await platform.DiagnossWordFirstAxisAsync(710));
            
            // 711 word             spindle warning
            await SetNative("diag_warn", await platform.DiagnossWordFirstAxisAsync(711));
            
            // 1520 dword           spindle rev count 1 (1000 min)
            await SetNative("diag_rev_1", await platform.DiagnossDoubleWordFirstAxisAsync(1520));
            
            // 1521 dword           spindle rev count 2 (1000 min)
            await SetNative("diag_rev_2", await platform.DiagnossDoubleWordFirstAxisAsync(1521));
            
            await Peel("spindle_data", 
                current_spindle,
                Get("spindle_names"), 
                Get("sp_speed"), 
                Get("sp_meter"), 
                Get("sp_maxrpm"), 
                Get("sp_gear"),
                Get("diag_lnk"),
                Get("diag_temp"),
                Get("diag_comms"),
                Get("diag_load_perc"),
                Get("diag_load_min"),
                Get("diag_coder"),
                Get("diag_loop_dev"),
                Get("diag_sync_error"),
                Get("diag_pos_data"),
                Get("diag_error"),
                Get("diag_warn"),
                Get("diag_rev_1"),
                Get("diag_rev_2"));
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}