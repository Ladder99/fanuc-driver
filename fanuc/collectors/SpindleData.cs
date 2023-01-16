using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class SpindleData : FanucMultiStrategyCollector
    {
        public SpindleData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitSpindleAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.SpindleData), "spindle", isCompound: true);
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            // main spindle displayed in cnc position screen
            // speed RPM,mm/rev... and feed mm/min...
            //dynamic speed_feed = await machine["platform"].RdSpeedAsync(0);
            //dynamic speed_speed = await machine["platform"].RdSpeedAsync(1);
            await Strategy.SetNativeKeyed($"sp_speed", 
                await Strategy.Platform.RdSpeedAsync(-1));

            // TODO: does not work
            //dynamic spindles_data = await machine["platform"].Acts2Async(-1);
                        
            // load % and speed RPM
            //dynamic load_meter_all = await machine["platform"].RdSpMeterAsync(0, spindles.response.cnc_rdspdlname.data_num);
            //dynamic motor_speed_all = await machine["platform"].RdSpMeterAsync(1, spindles.response.cnc_rdspdlname.data_num);
            //dynamic meter_all = await machine["platform"].RdSpMeterAsync(-1, spindles.response.cnc_rdspdlname.data_num);
                        
            // TODO: does not work
            //dynamic spload_all = await machine["platform"].RdSpLoadAsync(-1);
        }

        public override async Task CollectForEachSpindleAsync(short currentPath, short currentSpindle, string spindleName, dynamic spindleSplit, dynamic spindleMarker)
        {
            // rotational spindle speed
            await Strategy.SetNativeKeyed($"sp_acts", 
                await Strategy.Platform.Acts2Async(currentSpindle));
            
            //dynamic load_meter = await machine["platform"].RdSpMeterAsync(0, current_spindle);
            //dynamic motor_speed = await machine["platform"].RdSpMeterAsync(1, current_spindle);
            await Strategy.SetNativeKeyed($"sp_meter", 
                await Strategy.Platform.RdSpMeterAsync(-1, currentSpindle));

            // max rpm ratio
            await Strategy.SetNativeKeyed($"sp_maxrpm", 
                await Strategy.Platform.RdSpMaxRpmAsync(currentSpindle));

            // spindle gear ratio
            await Strategy.SetNativeKeyed($"sp_gear", 
                await Strategy.Platform.RdSpGearAsync(currentSpindle));
            
            // not sure what units the response data is
            //dynamic spload = await machine["platform"].RdSpLoadAsync(current_spindle);
            
            // for single spindle machine
            //      veneer = RdSpeed + RdSpMeter
            // for multi spindle machine
            //      sp 1: veneer = RdSpeed (speed, feed) + RdSpMeter (speed, load)
            //      sp n: veneer = RdSpMeter (speed, load) (no feed)
            
            // diagnose values
            // 400 bit 7 = LNK/1    comms with spindle control established
            await Strategy.SetNativeKeyed($"diag_lnk", 
                await Strategy.Platform.DiagnossByteAsync(400, currentSpindle));
            
            // 403 byte             temp of winding spindle motor (C) 0-255
            await Strategy.SetNativeKeyed($"diag_temp", 
                await Strategy.Platform.DiagnossByteAsync(403, currentSpindle));
            
            // 408                  spindle comms (causes of SP0749)
            //  0 CRE               CRC 
            //  1 FRE               Framing
            //  2 SNE               sender/receiver
            //  3 CER               reception
            //  4 CME               no response
            //  5 SCA               comm amplifier alarm
            //  7 SSA               spindle amplifier alarm
            await Strategy.SetNativeKeyed($"diag_comms", 
                await Strategy.Platform.DiagnossByteAsync(408, currentSpindle));
            
            // 410 word             spindle load (%)
            await Strategy.SetNativeKeyed($"diag_load_perc", 
                await Strategy.Platform.DiagnossWordAsync(410, currentSpindle));
            
            // 411 word             spindle load (min)
            await Strategy.SetNativeKeyed($"diag_load_min", 
                await Strategy.Platform.DiagnossWordAsync(411, currentSpindle));
            
            // 417 dword            spindle position coder feedback (detection units)
            await Strategy.SetNativeKeyed($"diag_coder", 
                await Strategy.Platform.DiagnossDoubleWordAsync(417, currentSpindle));
            
            // 418 dword            position loop deviation (detection units)
            await Strategy.SetNativeKeyed($"diag_loop_dev", 
                await Strategy.Platform.DiagnossDoubleWordAsync(418, currentSpindle));
            
            // 425 dword            sync error (detection unit)
            await Strategy.SetNativeKeyed($"diag_sync_error", 
                await Strategy.Platform.DiagnossDoubleWordAsync(425, currentSpindle));
            
            // 445 word             position data (pulse) 0-4095 , valid only when param3117=1
            await Strategy.SetNativeKeyed($"diag_pos_data", 
                await Strategy.Platform.DiagnossDoubleWordAsync(445, currentSpindle));
            
            // 710 word             spindle error
            await Strategy.SetNativeKeyed($"diag_error", 
                await Strategy.Platform.DiagnossWordAsync(710, currentSpindle));
            
            // 712 word             spindle warning
            await Strategy.SetNativeKeyed($"diag_warn", 
                await Strategy.Platform.DiagnossWordAsync(712,currentSpindle));
            
            // 1520 dword           spindle rev count 1 (1000 min)
            await Strategy.SetNativeKeyed($"diag_rev_1", 
                await Strategy.Platform.DiagnossDoubleWordAsync(1520, currentSpindle));
            
            // 1521 dword           spindle rev count 2 (1000 min)
            await Strategy.SetNativeKeyed($"diag_rev_2", 
                await Strategy.Platform.DiagnossDoubleWordAsync(1521, currentSpindle));
            
            // 4902 dword           spindle power consumption (watt)
            await Strategy.SetNativeKeyed($"diag_power", 
                await Strategy.Platform.DiagnossDoubleWordAsync(4902, currentSpindle));
            
            await Strategy.Peel("spindle", 
                currentSpindle,
                Strategy.Get("spindle_names"), 
                Strategy.Get($"sp_speed+{currentPath}"), 
                Strategy.GetKeyed($"sp_meter"), 
                Strategy.GetKeyed($"sp_maxrpm"), 
                Strategy.GetKeyed($"sp_gear"),
                Strategy.GetKeyed($"diag_lnk"),
                Strategy.GetKeyed($"diag_temp"),
                Strategy.GetKeyed($"diag_comms"),
                Strategy.GetKeyed($"diag_load_perc"),
                Strategy.GetKeyed($"diag_load_min"),
                Strategy.GetKeyed($"diag_coder"),
                Strategy.GetKeyed($"diag_loop_dev"),
                Strategy.GetKeyed($"diag_sync_error"),
                Strategy.GetKeyed($"diag_pos_data"),
                Strategy.GetKeyed($"diag_error"),
                Strategy.GetKeyed($"diag_warn"),
                Strategy.GetKeyed($"diag_rev_1"),
                Strategy.GetKeyed($"diag_rev_2"),
                Strategy.GetKeyed($"diag_power"),
                Strategy.GetKeyed($"sp_acts"));
        }
    }
}