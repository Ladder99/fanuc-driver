using l99.driver.@base;

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
            if (additionalInputs.All(o => o.success == true))
            {
                var current_spindle = input;
                var spindle_names = additionalInputs[0];
                var sp_speed = additionalInputs[1];
                var sp_meter = additionalInputs[2];
                var sp_maxrpm = additionalInputs[3];
                var sp_gear = additionalInputs[4];
                var diag_lnk = additionalInputs[5];
                var diag_temp = additionalInputs[6];
                var diag_comms = additionalInputs[7];
                var diag_load_perc = additionalInputs[8];
                var diag_load_min = additionalInputs[9];
                var diag_coder = additionalInputs[10];
                var diag_loop_dev = additionalInputs[11];
                var diag_sync_error = additionalInputs[12];
                var diag_pos_data = additionalInputs[13];
                var diag_error = additionalInputs[14];
                var diag_warn = additionalInputs[15];
                var diag_rev_1 = additionalInputs[16];
                var diag_rev_2 = additionalInputs[17];
                var diag_power = additionalInputs[18];
                var sp_acts = additionalInputs[19];

                var spindle_fields = spindle_names.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                var spindle_value = spindle_fields[current_spindle - 1]
                    .GetValue(spindle_names.response.cnc_rdspdlname.spdlname);
                var spindle_name = ((char) spindle_value.name).AsAscii() +
                                   ((char) spindle_value.suff1).AsAscii() +
                                   ((char) spindle_value.suff2).AsAscii();

                var current_value = new
                {
                    number = current_spindle,
                    name = spindle_name,
                    // feed is unnecessary here
                    //feed = sp_speed.response.cnc_rdspeed.speed.actf.data,
                    //feed_eu = speed_feed_EU(sp_speed.response.cnc_rdspeed.speed.actf.unit),
                    //speed = sp_speed.response.cnc_rdspeed.speed.acts.data / Math.Pow(10.0, sp_speed.response.cnc_rdspeed.speed.acts.dec),
                    speed = sp_acts.response.cnc_acts2.actualspindle.data[0],
                    speed_eu = speed_feed_EU(sp_speed.response.cnc_rdspeed.speed.acts.unit),
                    load = sp_meter.response.cnc_rdspmeter.loadmeter.spload1.spload.data / Math.Pow(10.0, sp_meter.response.cnc_rdspmeter.loadmeter.spload1.spload.dec),
                    load_eu = load_EU(sp_meter.response.cnc_rdspmeter.loadmeter.spload1.spload.unit),
                    maxrpm = sp_maxrpm.response.cnc_rdspmaxrpm.serialspindle.data[0],
                    maxrpm_eu = "rpm",
                    gearratio = sp_gear.response.cnc_rdspgear.serialspindle.data[0],
                    temperature = diag_temp.response.cnc_diagnoss.diag.cdata,
                    temperature_eu = "celsius",
                    power = diag_power.response.cnc_diagnoss.diag.ldata,
                    power_eu = "watt",
                    status_lnk = (diag_lnk.response.cnc_diagnoss.diag.cdata & (1 << 7)) != 0,
                    status_ssa = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 7)) != 0,
                    status_sca = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 5)) != 0,
                    status_cme = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 4)) != 0,
                    status_cer = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 3)) != 0,
                    status_sne = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 2)) != 0,
                    status_fre = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 1)) != 0,
                    status_cre = (diag_comms.response.cnc_diagnoss.diag.cdata & (1 << 10)) != 0,
                    coder_feedback = diag_coder.response.cnc_diagnoss.diag.ldata,
                    loop_deviation = diag_loop_dev.response.cnc_diagnoss.diag.ldata,
                    sync_error = diag_sync_error.response.cnc_diagnoss.diag.ldata,
                    position = diag_pos_data.response.cnc_diagnoss.diag.ldata,
                    position_eu = "pulse",
                    error = diag_error.response.cnc_diagnoss.diag.idata,
                    warning = diag_warn.response.cnc_diagnoss.diag.idata
                };

                //Console.WriteLine(JObject.FromObject(current_value).ToString());

                await onDataArrivedAsync(input, current_value);
                
                if (current_value.IsDifferentString((object) lastChangedValue))
                {
                    await onDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}