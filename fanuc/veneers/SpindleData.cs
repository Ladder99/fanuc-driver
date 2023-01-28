using l99.driver.@base;

// ReSharper disable UnusedVariable

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class SpindleData : Veneer
{
    public SpindleData(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
    {
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

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        // let the data output even if incorrect
        // TODO
        //if (additionalInputs.All(o => o.success == true))
        {
            var currentSpindle = additionalInputs[0];
            var spindleNames = nativeInputs[0];
            var spSpeed = nativeInputs[1];
            var spMeter = nativeInputs[2];
            var spMaxRpm = nativeInputs[3];
            var spGear = nativeInputs[4];
            var diagLnk = nativeInputs[5];
            var diagTemp = nativeInputs[6];
            var diagComms = nativeInputs[7];
            var diagLoadPerc = nativeInputs[8];
            var diagLoadMin = nativeInputs[9];
            var diagCoder = nativeInputs[10];
            var diagLoopDev = nativeInputs[11];
            var diagSyncError = nativeInputs[12];
            var diagPosData = nativeInputs[13];
            var diagError = nativeInputs[14];
            var diagWarn = nativeInputs[15];
            var diagRev1 = nativeInputs[16];
            var diagRev2 = nativeInputs[17];
            var diagPower = nativeInputs[18];
            var spActs = nativeInputs[19];

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
                speed = spActs!.response.cnc_acts2.actualspindle.data[0],
                speed_eu = speed_feed_EU(spSpeed!.response.cnc_rdspeed.speed.acts.unit),
                load = spMeter!.response.cnc_rdspmeter.loadmeter.spload1.spload.data /
                       Math.Pow(10.0, spMeter.response.cnc_rdspmeter.loadmeter.spload1.spload.dec),
                load_eu = load_EU(spMeter.response.cnc_rdspmeter.loadmeter.spload1.spload.unit),
                maxrpm = spMaxRpm!.response.cnc_rdspmaxrpm.serialspindle.data[0],
                maxrpm_eu = "rpm",
                gearratio = spGear!.response.cnc_rdspgear.serialspindle.data[0],
                temperature = diagTemp!.response.cnc_diagnoss.diag.cdata,
                temperature_eu = "celsius",
                power = diagPower!.response.cnc_diagnoss.diag.ldata,
                power_eu = "watt",
                status_lnk = (diagLnk!.response.cnc_diagnoss.diag.cdata & (1 << 7)) != 0,
                status_ssa = (diagComms!.response.cnc_diagnoss.diag.cdata & (1 << 7)) != 0,
                status_sca = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 5)) != 0,
                status_cme = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 4)) != 0,
                status_cer = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 3)) != 0,
                status_sne = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 2)) != 0,
                status_fre = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 1)) != 0,
                status_cre = (diagComms.response.cnc_diagnoss.diag.cdata & (1 << 10)) != 0,
                coder_feedback = diagCoder!.response.cnc_diagnoss.diag.ldata,
                loop_deviation = diagLoopDev!.response.cnc_diagnoss.diag.ldata,
                sync_error = diagSyncError!.response.cnc_diagnoss.diag.ldata,
                position = diagPosData!.response.cnc_diagnoss.diag.ldata,
                position_eu = "pulse",
                error = diagError!.response.cnc_diagnoss.diag.idata,
                warning = diagWarn!.response.cnc_diagnoss.diag.idata
            };

            //Console.WriteLine(JObject.FromObject(current_value).ToString());

            await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

            if (currentValue.IsDifferentString((object) LastChangedValue))
                await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
        }
        //else
        {
            //    await OnHandleErrorAsync(input);
        }

        return new {veneer = this};
    }
}