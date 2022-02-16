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
            await strategy.SetNativeKeyed($"sp_speed", 
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
            // rotational spindle speed
            await strategy.SetNativeKeyed($"sp_acts", 
                await strategy.Platform.Acts2Async(current_spindle));
            
            //dynamic load_meter = await machine["platform"].RdSpMeterAsync(0, current_spindle);
            //dynamic motor_speed = await machine["platform"].RdSpMeterAsync(1, current_spindle);
            await strategy.SetNativeKeyed($"sp_meter", 
                await strategy.Platform.RdSpMeterAsync(-1, current_spindle));

            // max rpm ratio
            await strategy.SetNativeKeyed($"sp_maxrpm", 
                await strategy.Platform.RdSpMaxRpmAsync(current_spindle));

            // spindle gear ratio
            await strategy.SetNativeKeyed($"sp_gear", 
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
            await strategy.SetNativeKeyed($"diag_lnk", 
                await strategy.Platform.DiagnossByteAsync(400, current_spindle));
            
            // 403 byte             temp of winding spindle motor (C) 0-255
            await strategy.SetNativeKeyed($"diag_temp", 
                await strategy.Platform.DiagnossByteAsync(403, current_spindle));
            
            // 408                  spindle comms (causes of SP0749)
            //  0 CRE               CRC 
            //  1 FRE               Framing
            //  2 SNE               sender/receiver
            //  3 CER               reception
            //  4 CME               no response
            //  5 SCA               comm amplifier alarm
            //  7 SSA               spindle amplifier alarm
            await strategy.SetNativeKeyed($"diag_comms", 
                await strategy.Platform.DiagnossByteAsync(408, current_spindle));
            
            // 410 word             spindle load (%)
            await strategy.SetNativeKeyed($"diag_load_perc", 
                await strategy.Platform.DiagnossWordAsync(410, current_spindle));
            
            // 411 word             spindle load (min)
            await strategy.SetNativeKeyed($"diag_load_min", 
                await strategy.Platform.DiagnossWordAsync(411, current_spindle));
            
            // 417 dword            spindle position coder feedback (detection units)
            await strategy.SetNativeKeyed($"diag_coder", 
                await strategy.Platform.DiagnossDoubleWordAsync(417, current_spindle));
            
            // 418 dword            position loop deviation (detection units)
            await strategy.SetNativeKeyed($"diag_loop_dev", 
                await strategy.Platform.DiagnossDoubleWordAsync(418, current_spindle));
            
            // 425 dword            sync error (detection unit)
            await strategy.SetNativeKeyed($"diag_sync_error", 
                await strategy.Platform.DiagnossDoubleWordAsync(425, current_spindle));
            
            // 445 word             position data (pulse) 0-4095 , valid only when param3117=1
            await strategy.SetNativeKeyed($"diag_pos_data", 
                await strategy.Platform.DiagnossDoubleWordAsync(445, current_spindle));
            
            // 710 word             spindle error
            await strategy.SetNativeKeyed($"diag_error", 
                await strategy.Platform.DiagnossWordAsync(710, current_spindle));
            
            // 712 word             spindle warning
            await strategy.SetNativeKeyed($"diag_warn", 
                await strategy.Platform.DiagnossWordAsync(712,current_spindle));
            
            // 1520 dword           spindle rev count 1 (1000 min)
            await strategy.SetNativeKeyed($"diag_rev_1", 
                await strategy.Platform.DiagnossDoubleWordAsync(1520, current_spindle));
            
            // 1521 dword           spindle rev count 2 (1000 min)
            await strategy.SetNativeKeyed($"diag_rev_2", 
                await strategy.Platform.DiagnossDoubleWordAsync(1521, current_spindle));
            
            // 4902 dword           spindle power consumption (watt)
            await strategy.SetNativeKeyed($"diag_power", 
                await strategy.Platform.DiagnossDoubleWordAsync(4902, current_spindle));
            
            await strategy.Peel("spindle", 
                current_spindle,
                strategy.Get("spindle_names"), 
                strategy.Get($"sp_speed+{current_path}"), 
                strategy.GetKeyed($"sp_meter"), 
                strategy.GetKeyed($"sp_maxrpm"), 
                strategy.GetKeyed($"sp_gear"),
                strategy.GetKeyed($"diag_lnk"),
                strategy.GetKeyed($"diag_temp"),
                strategy.GetKeyed($"diag_comms"),
                strategy.GetKeyed($"diag_load_perc"),
                strategy.GetKeyed($"diag_load_min"),
                strategy.GetKeyed($"diag_coder"),
                strategy.GetKeyed($"diag_loop_dev"),
                strategy.GetKeyed($"diag_sync_error"),
                strategy.GetKeyed($"diag_pos_data"),
                strategy.GetKeyed($"diag_error"),
                strategy.GetKeyed($"diag_warn"),
                strategy.GetKeyed($"diag_rev_1"),
                strategy.GetKeyed($"diag_rev_2"),
                strategy.GetKeyed($"diag_power"),
                strategy.GetKeyed($"sp_acts"));
        }
    }
}