using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class SpindleData : FanucMultiStrategyCollector
    {
        public SpindleData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.SpindleData), "spindle", isCompound: true);
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            // main spindle displayed in cnc position screen
            // speed RPM,mm/rev... and feed mm/min...
            //dynamic speed_feed = await machine["platform"].RdSpeedAsync(0);
            //dynamic speed_speed = await machine["platform"].RdSpeedAsync(1);
            await strategy.SetNative($"sp_speed+{current_path}", 
                await strategy.Platform.RdSpeedAsync(-1));

            // TODO: does not work
            //dynamic spindles_data = await machine["platform"].Acts2Async(-1);
                        
            // load % and speed RPM
            //dynamic load_meter_all = await machine["platform"].RdSpMeterAsync(0, spindles.response.cnc_rdspdlname.data_num);
            //dynamic motor_speed_all = await machine["platform"].RdSpMeterAsync(1, spindles.response.cnc_rdspdlname.data_num);
            //dynamic meter_all = await machine["platform"].RdSpMeterAsync(-1, spindles.response.cnc_rdspdlname.data_num);
                        
            // TODO: does not work
            //dynamic spload_all = await machine["platform"].RdSpLoadAsync(-1);
        }

        public override async Task CollectForEachSpindleAsync(short current_path, short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            var spindle_key = string.Join("/", spindle_split);
            
            // rotational spindle speed
            await strategy.SetNative($"sp_acts+{spindle_key}", 
                await strategy.Platform.Acts2Async(current_spindle));
            
            //dynamic load_meter = await machine["platform"].RdSpMeterAsync(0, current_spindle);
            //dynamic motor_speed = await machine["platform"].RdSpMeterAsync(1, current_spindle);
            await strategy.SetNative($"sp_meter+{spindle_key}", 
                await strategy.Platform.RdSpMeterAsync(-1, current_spindle));

            // max rpm ratio
            await strategy.SetNative($"sp_maxrpm+{spindle_key}", 
                await strategy.Platform.RdSpMaxRpmAsync(current_spindle));

            // spindle gear ratio
            await strategy.SetNative($"sp_gear+{spindle_key}", 
                await strategy.Platform.RdSpGearAsync(current_spindle));
            
            // not sure what units the response data is
            //dynamic spload = await machine["platform"].RdSpLoadAsync(current_spindle);
            
            // for single spindle machine
            //      veneer = RdSpeed + RdSpMeter
            // for multi spindle machine
            //      sp 1: veneer = RdSpeed (speed, feed) + RdSpMeter (speed, load)
            //      sp n: veneer = RdSpMeter (speed, load) (no feed)
            
            // diagnose values
            // 400 bit 7 = LNK/1    comms with spindle control established
            await strategy.SetNative($"diag_lnk+{spindle_key}", 
                await strategy.Platform.DiagnossByteAsync(400, current_spindle));
            
            // 403 byte             temp of winding spindle motor (C) 0-255
            await strategy.SetNative($"diag_temp+{spindle_key}", 
                await strategy.Platform.DiagnossByteAsync(403, current_spindle));
            
            // 408                  spindle comms (causes of SP0749)
            //  0 CRE               CRC 
            //  1 FRE               Framing
            //  2 SNE               sender/receiver
            //  3 CER               reception
            //  4 CME               no response
            //  5 SCA               comm amplifier alarm
            //  7 SSA               spindle amplifier alarm
            await strategy.SetNative($"diag_comms+{spindle_key}", 
                await strategy.Platform.DiagnossByteAsync(408, current_spindle));
            
            // 410 word             spindle load (%)
            await strategy.SetNative($"diag_load_perc+{spindle_key}", 
                await strategy.Platform.DiagnossWordAsync(410, current_spindle));
            
            // 411 word             spindle load (min)
            await strategy.SetNative($"diag_load_min+{spindle_key}", 
                await strategy.Platform.DiagnossWordAsync(411, current_spindle));
            
            // 417 dword            spindle position coder feedback (detection units)
            await strategy.SetNative($"diag_coder+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(417, current_spindle));
            
            // 418 dword            position loop deviation (detection units)
            await strategy.SetNative($"diag_loop_dev+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(418, current_spindle));
            
            // 425 dword            sync error (detection unit)
            await strategy.SetNative($"diag_sync_error+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(425, current_spindle));
            
            // 445 word             position data (pulse) 0-4095 , valid only when param3117=1
            await strategy.SetNative($"diag_pos_data+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(445, current_spindle));
            
            // 710 word             spindle error
            await strategy.SetNative($"diag_error+{spindle_key}", 
                await strategy.Platform.DiagnossWordAsync(710, current_spindle));
            
            // 712 word             spindle warning
            await strategy.SetNative($"diag_warn+{spindle_key}", 
                await strategy.Platform.DiagnossWordAsync(712,current_spindle));
            
            // 1520 dword           spindle rev count 1 (1000 min)
            await strategy.SetNative($"diag_rev_1+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(1520, current_spindle));
            
            // 1521 dword           spindle rev count 2 (1000 min)
            await strategy.SetNative($"diag_rev_2+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(1521, current_spindle));
            
            // 4902 dword           spindle power consumption (watt)
            await strategy.SetNative($"diag_power+{spindle_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(4902, current_spindle));
            
            await strategy.Peel("spindle", 
                current_spindle,
                strategy.Get("spindle_names"), 
                strategy.Get($"sp_speed+{current_path}"), 
                strategy.Get($"sp_meter+{spindle_key}"), 
                strategy.Get($"sp_maxrpm+{spindle_key}"), 
                strategy.Get($"sp_gear+{spindle_key}"),
                strategy.Get($"diag_lnk+{spindle_key}"),
                strategy.Get($"diag_temp+{spindle_key}"),
                strategy.Get($"diag_comms+{spindle_key}"),
                strategy.Get($"diag_load_perc+{spindle_key}"),
                strategy.Get($"diag_load_min+{spindle_key}"),
                strategy.Get($"diag_coder+{spindle_key}"),
                strategy.Get($"diag_loop_dev+{spindle_key}"),
                strategy.Get($"diag_sync_error+{spindle_key}"),
                strategy.Get($"diag_pos_data+{spindle_key}"),
                strategy.Get($"diag_error+{spindle_key}"),
                strategy.Get($"diag_warn+{spindle_key}"),
                strategy.Get($"diag_rev_1+{spindle_key}"),
                strategy.Get($"diag_rev_2+{spindle_key}"),
                strategy.Get($"diag_power+{spindle_key}"),
                strategy.Get($"sp_acts+{spindle_key}"));
        }
    }
}