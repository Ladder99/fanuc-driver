using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.veneers
{
    public class SpindleData: Veneer
    {
        public SpindleData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            _lastChangedValue = new
            {
                name = string.Empty,
                
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
                    return "%";
                case 1:
                    return "rpm";
            }

            return string.Empty;
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            
            var current_spindle = input;
            var spindle_names = additional_inputs[0];
            var sp_speed = additional_inputs[1];
            var sp_meter = additional_inputs[2];
            var sp_maxrpm = additional_inputs[3]; 
            var sp_gear = additional_inputs[4];
            var diag_lnk = additional_inputs[5];
            var diag_temp = additional_inputs[6];
            var diag_comms = additional_inputs[7];
            var diag_load_perc = additional_inputs[8];
            var diag_load_min = additional_inputs[9];
            var diag_coder = additional_inputs[10];
            var diag_loop_dev = additional_inputs[11];
            var diag_sync_error = additional_inputs[12];
            var diag_pos_data = additional_inputs[13];
            var diag_error = additional_inputs[14];
            var diag_warn = additional_inputs[15];
            var diag_rev_1 = additional_inputs[16];
            var diag_rev_2 = additional_inputs[17];
            
            var spindle_fields = spindle_names.response.cnc_rdspdlname.spdlname.GetType().GetFields();
            var spindle_value = spindle_fields[current_spindle - 1].GetValue(spindle_names.response.cnc_rdspdlname.spdlname);
            var spindle_name = ((char) spindle_value.name).AsAscii() +
                               ((char) spindle_value.suff1).AsAscii() +
                               ((char) spindle_value.suff2).AsAscii();


            var current_value = new
            {
                number = current_spindle,
                name = spindle_name,
                feed = sp_speed.response.cnc_rdspeed.speed.actf.data,
                feed_eu = speed_feed_EU(sp_speed.response.cnc_rdspeed.speed.actf.unit),
                speed = sp_speed.response.cnc_rdspeed.speed.acts.data,
                speed_eu = speed_feed_EU(sp_speed.response.cnc_rdspeed.speed.acts.unit),
                load = sp_meter.response.cnc_rdspmeter.loadmeter.spload1.spload.data,
                load_eu = load_EU(sp_meter.response.cnc_rdspmeter.loadmeter.spload1.spload.unit),
                maxrpm = sp_maxrpm.response.cnc_rdspmaxrpm.serialspindle.data[0],
                maxrpm_eu = "rpm",
                gearratio = sp_gear.response.cnc_rdspgear.serialspindle.data[0],
                temperature = diag_temp.response.cnc_diagnoss.diag.cdata,
                temperature_eu = "celsius",
                status_lnk = diag_lnk.response.cnc_diagnoss.diag.cdata,
                status_ssa = diag_comms.response.cnc_diagnoss.diag.cdata,
                status_sca = diag_comms.response.cnc_diagnoss.diag.cdata,
                status_cme = diag_comms.response.cnc_diagnoss.diag.cdata,
                status_cer = diag_comms.response.cnc_diagnoss.diag.cdata,
                status_sne = diag_comms.response.cnc_diagnoss.diag.cdata,
                ststus_fre = diag_comms.response.cnc_diagnoss.diag.cdata,
                status_cre = diag_comms.response.cnc_diagnoss.diag.cdata,
                coder_feedback = diag_coder.response.cnc_diagnoss.diag.ldata,
                loop_deviation = diag_loop_dev.response.cnc_diagnoss.diag.ldata,
                sync_error = diag_sync_error.response.cnc_diagnoss.diag.ldata,
                position = diag_pos_data.response.cnc_diagnoss.diag.ldata,
                position_eu = "pulse",
                error = diag_error.response.cnc_diagnoss.diag.idata,
                warning = diag_warn.response.cnc_diagnoss.diag.idata
            };
            
            Console.WriteLine(JObject.FromObject(current_value).ToString());
            
            await onDataArrivedAsync(input, current_value);
            
            if(current_value.IsDifferentString((object)_lastChangedValue))
                await onDataChangedAsync(input, current_value);
            /*
            else
            {
                await onErrorAsync(input);
            }*/
            
            return new { veneer = this };
        }
    }
}