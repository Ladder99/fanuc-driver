#pragma warning disable CS8602

using l99.driver.@base;

// ReSharper disable UnusedVariable

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class SpindleData: Veneer
    {
        public SpindleData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                
            };
        }

        private string speed_feed_EU(short unit)
        {
            switch (unit)
            {
                case 0:
                    return "mm/min";
                case 1:
                    return "inch/min";
                case 2:
                    return "rpm";
                case 3:
                    return "mm/rev";
                case 4:
                    return "in/rev";
            }

            return string.Empty;
        }
        
        private string load_EU(short unit)
        {
            switch (unit)
            {
                case 0:
                    return "percent";
                case 1:
                    return "rpm";
            }

            return string.Empty;
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            // let the data output even if incorrect
            // TODO
            //if (additionalInputs.All(o => o.success == true))
            {
                var currentSpindle = input;
                var spindleNames = additionalInputs[0];
                var spSpeed = additionalInputs[1];
                var spMeter = additionalInputs[2];
                var spMaxRpm = additionalInputs[3];
                var spGear = additionalInputs[4];
                var diagLnk = additionalInputs[5];
                var diagTemp = additionalInputs[6];
                var diagComms = additionalInputs[7];
                var diagLoadPerc = additionalInputs[8];
                var diagLoadMin = additionalInputs[9];
                var diagCoder = additionalInputs[10];
                var diagLoopDev = additionalInputs[11];
                var diagSyncError = additionalInputs[12];
                var diagPosData = additionalInputs[13];
                var diagError = additionalInputs[14];
                var diagWarn = additionalInputs[15];
                var diagRev1 = additionalInputs[16];
                var diagRev2 = additionalInputs[17];
                var diagPower = additionalInputs[18];
                var spActs = additionalInputs[19];

                var spindleFields = spindleNames.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                var spindleValue = spindleFields[currentSpindle - 1]
                    .GetValue(spindleNames.response.cnc_rdspdlname.spdlname);
                var spindleName = ((char) spindleValue.name).AsAscii() +
                                   ((char) spindleValue.suff1).AsAscii() +
                                   ((char) spindleValue.suff2).AsAscii();

                var currentValue = new
                {
                    number = currentSpindle,
                    name = spindleName,
                    // feed is unnecessary here
                    //feed = sp_speed.response.cnc_rdspeed.speed.actf.data,
                    //feed_eu = speed_feed_EU(sp_speed.response.cnc_rdspeed.speed.actf.unit),
                    //speed = sp_speed.response.cnc_rdspeed.speed.acts.data / Math.Pow(10.0, sp_speed.response.cnc_rdspeed.speed.acts.dec),
                    speed = spActs.response.cnc_acts2.actualspindle.data[0],
                    speed_eu = speed_feed_EU(spSpeed.response.cnc_rdspeed.speed.acts.unit),
                    load = spMeter.response.cnc_rdspmeter.loadmeter.spload1.spload.data / Math.Pow(10.0, spMeter.response.cnc_rdspmeter.loadmeter.spload1.spload.dec),
                    load_eu = load_EU(spMeter.response.cnc_rdspmeter.loadmeter.spload1.spload.unit),
                    maxrpm = spMaxRpm.response.cnc_rdspmaxrpm.serialspindle.data[0],
                    maxrpm_eu = "rpm",
                    gearratio = spGear.response.cnc_rdspgear.serialspindle.data[0],
                    temperature = diagTemp.response.cnc_diagnoss.diag.cdata,
                    temperature_eu = "celsius",
                    power = diagPower.response.cnc_diagnoss.diag.ldata,
                    power_eu = "watt",
                    status_lnk = (diagLnk.response.cnc_diagnoss.diag.cdata & (1 << 7)) != 0,
                    status_ssa = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 7)) != 0,
                    status_sca = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 5)) != 0,
                    status_cme = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 4)) != 0,
                    status_cer = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 3)) != 0,
                    status_sne = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 2)) != 0,
                    status_fre = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 1)) != 0,
                    status_cre = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 10)) != 0,
                    coder_feedback = diagCoder.response.cnc_diagnoss.diag.ldata,
                    loop_deviation = diagLoopDev.response.cnc_diagnoss.diag.ldata,
                    sync_error = diagSyncError.response.cnc_diagnoss.diag.ldata,
                    position = diagPosData.response.cnc_diagnoss.diag.ldata,
                    position_eu = "pulse",
                    error = diagError.response.cnc_diagnoss.diag.idata,
                    warning = diagWarn.response.cnc_diagnoss.diag.idata
                };

                //Console.WriteLine(JObject.FromObject(current_value).ToString());

                await OnDataArrivedAsync(input, currentValue);
                
                if (currentValue.IsDifferentString((object) lastChangedValue))
                {
                    await OnDataChangedAsync(input, currentValue);
                }
            }
            //else
            {
                //    await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}
#pragma warning restore CS8602